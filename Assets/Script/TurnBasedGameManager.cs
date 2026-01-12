using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public enum TeamType { Player, Enemy }
public enum BulletType { Normal, Bomb, Triple }

[System.Serializable]
public class CardData
{
    public BulletType type;
    public Sprite cardIcon;
    public int energyCost;
}

public class TurnBasedGameManager : MonoBehaviour
{
    public static TurnBasedGameManager Instance { get; private set; }
    public List<CardData> cardDatabase;

    [Header("Game State")]
    [SerializeField] private int maxHandSize = 4;
    [SerializeField] private int baseMaxEnergy = 3;

    [Header("Turn Timer")]
    [SerializeField] private float turnDuration = 30f;
    private float currentTurnTime;
    private bool isTimerRunning = false;

    [Header("Auto End Turn Settings")]
    [SerializeField] private float autoEndTurnDelay = 3f;
    private bool isWaitingForAutoEnd = false;

    [Header("Rank Points System")]
    [SerializeField] private int pointsForWin = 50;
    [SerializeField] private int pointsForLose = 30;

    private List<BulletType> sharedPlayerHand = new List<BulletType>();
    private int sharedPlayerEnergy = 0;
    private int sharedPlayerMaxEnergy = 0;
    private int playerTurnsPlayed = 0;

    private List<GameUnit_SlingBoom> turnOrder = new List<GameUnit_SlingBoom>();
    private int currentTurnIndex = 0;
    private GameUnit_SlingBoom currentUnit;
    private bool isGameStarted = false;
    private bool isGameOver = false;

    private UIGameplay_SlingBoom uiGameplay;

    public List<BulletType> SharedPlayerHand => sharedPlayerHand;
    public int SharedPlayerEnergy => sharedPlayerEnergy;
    public int SharedPlayerMaxEnergy => sharedPlayerMaxEnergy;

    public bool IsGameOver() => isGameOver;
    public UIGameplay_SlingBoom GetUIGameplay() => uiGameplay;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Không tự động start game nữa
    }

    private void Update()
    {
        // ✅ CRITICAL FIX: CHECK isGameStarted TRƯỚC
        if (!isGameStarted || isGameOver)
        {
            return;
        }

        if (isTimerRunning)
        {
            currentTurnTime -= Time.deltaTime;

            if (uiGameplay != null)
            {
                uiGameplay.UpdateTimer(currentTurnTime);
            }

            if (currentTurnTime <= 0)
            {
                currentTurnTime = 0;
                StopTimer();
                Debug.Log(">>> TIME IS UP! Auto ending turn...");
                EndTurn();
            }
        }
    }

    // ✅ HÀM MỚI: StartGame - Gọi từ bên ngoài khi level đã load xong
    public void StartGame()
    {
        if (isGameStarted)
        {
            Debug.LogWarning("⚠️ Game already started! Resetting...");
            ResetGameState();
        }

        if (isGameOver)
        {
            Debug.LogWarning("⚠️ Game đang ở trạng thái GameOver! Forcing reset...");
            ResetGameState();
        }

        Debug.Log("🎮 [TurnBasedGameManager] StartGame() called!");
        StartCoroutine(InitializeGameSequence());
    }

    // ✅ CRITICAL FIX: StopGame - Dừng hoàn toàn game
    public void StopGame()
    {
        Debug.Log("🛑 [TurnBasedGameManager] StopGame() called!");

        // ✅ Stop tất cả coroutines
        StopAllCoroutines();

        // ✅ Reset flags NGAY LẬP TỨC
        isGameStarted = false;
        isGameOver = false;
        isTimerRunning = false;
        isWaitingForAutoEnd = false;

        // ✅ Clear current unit
        if (currentUnit != null)
        {
            currentUnit.OnTurnEnded();
            currentUnit = null;
        }

        // ✅ Clear UI
        if (uiGameplay != null)
        {
            uiGameplay.HideAllUI();
        }

        Debug.Log("✅ [TurnBasedGameManager] Game stopped successfully");
    }

    private void StartTimer()
    {
        currentTurnTime = turnDuration;
        isTimerRunning = true;

        Debug.Log($"[Manager] ⏰ Timer started: {turnDuration}s");

        if (uiGameplay != null)
        {
            uiGameplay.UpdateTimer(currentTurnTime);
        }
        else
        {
            Debug.LogError("[Manager] ❌ UIGameplay is null when starting timer!");
        }
    }

    private void StopTimer()
    {
        isTimerRunning = false;
        Debug.Log("[Manager] ⏸️ Timer stopped");
    }

    private IEnumerator InitializeGameSequence()
    {
        Debug.Log("=== GAME INITIALIZATION START ===");

        yield return null;
        yield return new WaitForSeconds(0.5f);

        uiGameplay = FindFirstObjectByType<UIGameplay_SlingBoom>();

        if (uiGameplay == null)
        {
            Debug.LogError("❌ UIGameplay not found!");
            yield break;
        }
        else
        {
            Debug.Log("✅ UIGameplay found");
        }

        var allUnits = FindObjectsByType<GameUnit_SlingBoom>(FindObjectsSortMode.None);
        int retryCount = 0;

        while (allUnits.Length == 0 && retryCount < 5)
        {
            Debug.LogWarning($"Chưa tìm thấy units, retry {retryCount + 1}/5...");
            yield return new WaitForSeconds(0.2f);
            allUnits = FindObjectsByType<GameUnit_SlingBoom>(FindObjectsSortMode.None);
            retryCount++;
        }

        var players = allUnits.OfType<PlayerUnit_SlingBoom>().ToList();
        var enemies = allUnits.OfType<EnemyAIUnit_SlingBoom>().ToList();

        Debug.Log($"Found {players.Count} Players and {enemies.Count} Enemies");

        if (players.Count == 0 && enemies.Count == 0)
        {
            Debug.LogError("❌ No units found after retry!");
            yield break;
        }

        if (uiGameplay != null)
        {
            for (int i = 0; i < players.Count; i++)
            {
                uiGameplay.RegisterUnit(players[i], playerIndex: i);
            }
            for (int i = 0; i < enemies.Count; i++)
            {
                uiGameplay.RegisterUnit(enemies[i], enemyIndex: i);
            }

            uiGameplay.HideAllUI();
        }

        turnOrder = CreateAlternatingTurnOrder(players, enemies);

        bool playerGoesFirst = Random.value > 0.5f;
        if (!playerGoesFirst && turnOrder.Count > 1)
        {
            var firstUnit = turnOrder[0];
            turnOrder.RemoveAt(0);
            turnOrder.Add(firstUnit);
        }

        Debug.Log($"Turn Order: {string.Join(" -> ", turnOrder.Select(u => u.UnitName))}");

        InitializePlayerState();

        PlayerUnit_SlingBoom anyPlayer = players.FirstOrDefault();
        if (anyPlayer != null && uiGameplay != null)
        {
            anyPlayer.SyncSharedState(sharedPlayerHand, sharedPlayerEnergy, sharedPlayerMaxEnergy);

            if (turnOrder.Count > 0 && turnOrder[0] is EnemyAIUnit_SlingBoom)
            {
                uiGameplay.HideCards();
            }
        }

        if (CameraController_SlingBoom.Instance != null)
        {
            Debug.Log(">>> Starting with OVERVIEW mode");
            CameraController_SlingBoom.Instance.ShowOverview(() =>
            {
                Debug.Log(">>> Overview complete, starting first turn");
                currentTurnIndex = 0;
                isGameStarted = true;
                StartNewTurn();
            });
        }
        else
        {
            currentTurnIndex = 0;
            isGameStarted = true;
            StartNewTurn();
        }
    }

    private List<GameUnit_SlingBoom> CreateAlternatingTurnOrder(List<PlayerUnit_SlingBoom> players, List<EnemyAIUnit_SlingBoom> enemies)
    {
        List<GameUnit_SlingBoom> order = new List<GameUnit_SlingBoom>();
        int maxCount = Mathf.Max(players.Count, enemies.Count);

        for (int i = 0; i < maxCount; i++)
        {
            if (i < players.Count)
            {
                order.Add(players[i]);
            }

            if (i < enemies.Count)
            {
                order.Add(enemies[i]);
            }
        }

        return order;
    }

    private void InitializePlayerState()
    {
        playerTurnsPlayed = 0;
        sharedPlayerMaxEnergy = baseMaxEnergy;
        sharedPlayerEnergy = sharedPlayerMaxEnergy;

        while (sharedPlayerHand.Count < maxHandSize)
        {
            if (cardDatabase != null && cardDatabase.Count > 0)
            {
                var randomCard = cardDatabase[Random.Range(0, cardDatabase.Count)];
                sharedPlayerHand.Add(randomCard.type);
            }
            else
            {
                sharedPlayerHand.Add((BulletType)Random.Range(0, 3));
            }
        }
    }

    private void StartNewTurn()
    {
        // ✅ CHECK isGameStarted TRƯỚC KHI LÀM BẤT CỨ ĐIỀU GÌ
        if (!isGameStarted || isGameOver) return;

        Debug.Log("=== START NEW TURN ===");

        RemoveDeadUnits();

        if (isGameOver) return;

        if (turnOrder.Count == 0)
        {
            Debug.Log("Game Over - No units left!");
            return;
        }

        if (currentTurnIndex >= turnOrder.Count)
        {
            currentTurnIndex = 0;
        }

        currentUnit = turnOrder[currentTurnIndex];

        if (currentUnit == null || currentUnit.IsDead)
        {
            Debug.LogWarning($"Current unit is dead/null, skipping to next turn...");
            currentTurnIndex = (currentTurnIndex + 1) % turnOrder.Count;
            StartNewTurn();
            return;
        }

        Debug.Log($">>> CURRENT UNIT: {currentUnit.UnitName}");

        if (CameraController_SlingBoom.Instance != null)
        {
            CameraController_SlingBoom.Instance.FocusOnUnit(currentUnit, () =>
            {
                StartTurnForCurrentUnit();
            });
        }
        else
        {
            StartTurnForCurrentUnit();
        }
    }

    private void StartTurnForCurrentUnit()
    {
        // ✅ CHECK AGAIN
        if (!isGameStarted || isGameOver) return;

        Debug.Log($"[Manager] StartTurnForCurrentUnit: {currentUnit.UnitName}");

        if (uiGameplay != null)
        {
            uiGameplay.HideChoices();
        }

        PlayerUnit_SlingBoom player = currentUnit as PlayerUnit_SlingBoom;

        if (player != null)
        {
            Debug.Log($"[Manager] Starting PLAYER turn for {player.UnitName}");
            if (SoundManager_SlingBoom.Instance != null)
            {
                SoundManager_SlingBoom.Instance.PlayVFXSound(4);
            }

            playerTurnsPlayed++;
            sharedPlayerMaxEnergy = baseMaxEnergy + playerTurnsPlayed;
            sharedPlayerEnergy = sharedPlayerMaxEnergy;

            Debug.Log($"[Manager] Player energy: {sharedPlayerEnergy}/{sharedPlayerMaxEnergy}");

            while (sharedPlayerHand.Count < maxHandSize)
            {
                if (cardDatabase != null && cardDatabase.Count > 0)
                {
                    var randomCard = cardDatabase[Random.Range(0, cardDatabase.Count)];
                    sharedPlayerHand.Add(randomCard.type);
                }
                else
                {
                    sharedPlayerHand.Add((BulletType)Random.Range(0, 3));
                }
            }

            Debug.Log($"[Manager] Player hand size: {sharedPlayerHand.Count}");

            player.SyncSharedState(sharedPlayerHand, sharedPlayerEnergy, sharedPlayerMaxEnergy);
            player.OnTurnStarted();

            if (uiGameplay != null)
            {
                Debug.Log("[Manager] Updating UI for player turn...");

                uiGameplay.UpdateTurnText($"{player.UnitName} Turn");
                uiGameplay.UpdateEnergy(sharedPlayerEnergy, sharedPlayerMaxEnergy);
                uiGameplay.ShowPlayerUI();
                uiGameplay.TogglePlayerControls(true);
                uiGameplay.RenderHand(player);

                Debug.Log("[Manager] UI updated successfully");
            }

            StartTimer();
            Debug.Log("[Manager] Timer started");
        }
        else
        {
            Debug.Log($"[Manager] Starting ENEMY turn for {currentUnit.UnitName}");

            currentUnit.OnTurnStarted();

            if (uiGameplay != null)
            {
                uiGameplay.ShowEnemyUI();
                uiGameplay.UpdateTurnText($"{currentUnit.UnitName} Turn");
                uiGameplay.TogglePlayerControls(false);
                uiGameplay.HideCards();
            }

            StartTimer();
        }
    }

    public void OnUnitDied(GameUnit_SlingBoom deadUnit)
    {
        // ✅ CRITICAL: Không check game over nếu game chưa start hoặc đã over
        if (!isGameStarted || isGameOver) return;

        Debug.Log($"[Manager] UNIT DIED: {deadUnit.UnitName}. Checking Win Condition immediately...");

        if (turnOrder.Contains(deadUnit))
        {
            turnOrder.Remove(deadUnit);
        }

        CheckGameOver();
    }

    private void RemoveDeadUnits()
    {
        // ✅ CHECK
        if (!isGameStarted || isGameOver) return;

        int beforeCount = turnOrder.Count;
        turnOrder.RemoveAll(unit => unit == null || unit.IsDead);
        int afterCount = turnOrder.Count;

        if (beforeCount != afterCount)
        {
            Debug.Log($"Removed {beforeCount - afterCount} dead units");
            CheckGameOver();
        }
    }

    private void CheckGameOver()
    {
        // ✅ CRITICAL FIX: Không check nếu game chưa start
        if (!isGameStarted || isGameOver) return;

        bool hasPlayer = false;
        bool hasEnemy = false;

        foreach (var unit in turnOrder)
        {
            if (unit is PlayerUnit_SlingBoom) hasPlayer = true;
            if (unit is EnemyAIUnit_SlingBoom) hasEnemy = true;
        }

        if (!hasEnemy && hasPlayer)
        {
            HandleGameOver(true);
        }
        else if (!hasPlayer && hasEnemy)
        {
            HandleGameOver(false);
        }
        else if (!hasPlayer && !hasEnemy)
        {
            HandleGameOver(false);
        }
    }

    private void HandleGameOver(bool playerWon)
    {
        isGameOver = true;
        isGameStarted = false; // ✅ Set false để stop mọi logic
        StopTimer();

        Debug.Log($"========== {(playerWon ? "YOU WIN!" : "YOU LOSE!")} ==========");

        UpdatePlayerRankPoints(playerWon);

        StopAllCoroutines();

        if (uiGameplay != null)
        {
            uiGameplay.HideAllUI();
        }

        if (playerWon && LevelManager_SlingBoom.Instance != null)
        {
            LevelManager_SlingBoom.Instance.CompleteCurrentLevel(3);
        }

        StartCoroutine(ShowGameOverDelayed(playerWon));
    }

    private void UpdatePlayerRankPoints(bool playerWon)
    {
        int currentPoints = PlayerPrefs.GetInt("PlayerPoints", 0);
        int pointsChange = playerWon ? pointsForWin : -pointsForLose;
        int newPoints = currentPoints + pointsChange;

        newPoints = Mathf.Max(0, newPoints);

        PlayerPrefs.SetInt("PlayerPoints", newPoints);
        PlayerPrefs.Save();

        Debug.Log($"[TurnBasedGameManager] Rank Points Updated:");
        Debug.Log($"  Previous: {currentPoints}");
        Debug.Log($"  Change: {(playerWon ? "+" : "-")}{(playerWon ? pointsForWin : pointsForLose)}");
        Debug.Log($"  New Total: {newPoints}");
    }

    private IEnumerator ShowGameOverDelayed(bool playerWon)
    {
        yield return new WaitForSeconds(1f);

        // ✅ CHECK trước khi show UI
        if (!isGameOver) yield break;

        if (UIManager_SlingBoom.Instance != null)
        {
            if (playerWon)
            {
                UIManager_SlingBoom.Instance.EnableWin(true);
            }
            else
            {
                UIManager_SlingBoom.Instance.EnableLoss(true);
            }
        }
    }

    public void ScheduleGameOverCheck()
    {
        if (!isGameStarted || isGameOver) return;
        StartCoroutine(DelayedGameOverCheck());
    }

    private IEnumerator DelayedGameOverCheck()
    {
        yield return new WaitForSeconds(0.5f);
        CheckGameOverDirectly();
    }

    private void CheckGameOverDirectly()
    {
        if (!isGameStarted || isGameOver) return;

        bool hasPlayer = false;
        bool hasEnemy = false;

        var allUnits = FindObjectsByType<GameUnit_SlingBoom>(FindObjectsSortMode.None);

        foreach (var unit in allUnits)
        {
            if (unit == null || unit.IsDead) continue;

            if (unit is PlayerUnit_SlingBoom) hasPlayer = true;
            if (unit is EnemyAIUnit_SlingBoom) hasEnemy = true;
        }

        if (!hasEnemy && hasPlayer)
        {
            HandleGameOver(true);
        }
        else if (!hasPlayer && hasEnemy)
        {
            HandleGameOver(false);
        }
    }

    public void EndTurn()
    {
        if (!isGameStarted || isGameOver) return;

        Debug.Log($"[Manager] ⏭️ END TURN called");

        StopTimer();

        if (isWaitingForAutoEnd)
        {
            StopCoroutine(nameof(AutoEndTurnAfterDelay));
            isWaitingForAutoEnd = false;
        }

        if (currentUnit != null)
        {
            currentUnit.OnTurnEnded();
        }

        if (uiGameplay != null)
        {
            uiGameplay.HideAllUI();
        }

        currentTurnIndex = (currentTurnIndex + 1) % turnOrder.Count;

        RemoveDeadUnits();
        if (isGameOver) return;

        if (CameraController_SlingBoom.Instance != null)
        {
            CameraController_SlingBoom.Instance.ShowOverview(() =>
            {
                if (isGameOver) return;
                StartNewTurn();
            });
        }
        else
        {
            StartNewTurn();
        }
    }

    public void UpdateSharedPlayerState(List<BulletType> hand, int energy)
    {
        sharedPlayerHand = new List<BulletType>(hand);
        sharedPlayerEnergy = energy;

        if (uiGameplay != null)
        {
            uiGameplay.UpdateEnergy(sharedPlayerEnergy, sharedPlayerMaxEnergy);
        }
    }

    public void OnPlayerShot()
    {
        if (currentUnit is PlayerUnit_SlingBoom && isGameStarted && !isGameOver)
        {
            Debug.Log("[Manager] Player shot detected, starting auto-end timer");
            StartCoroutine(AutoEndTurnAfterDelay());
        }
    }

    public CardData GetCardData(BulletType type)
    {
        return cardDatabase.Find(c => c.type == type);
    }

    public void BuffPlayerMaxEnergy(int buffAmount)
    {
        Debug.Log($"[Manager] 💪 Buffing player max energy by {buffAmount}");

        if (isWaitingForAutoEnd)
        {
            Debug.Log("[Manager] Cancelling auto-end turn due to buff");
            StopAllCoroutines();
            isWaitingForAutoEnd = false;
        }

        baseMaxEnergy += buffAmount;
        sharedPlayerMaxEnergy += buffAmount;
        sharedPlayerEnergy = sharedPlayerMaxEnergy;

        Debug.Log($"[Manager] New max energy: {sharedPlayerMaxEnergy}");

        var allPlayers = FindObjectsByType<PlayerUnit_SlingBoom>(FindObjectsSortMode.None);
        foreach (var player in allPlayers)
        {
            if (player != null && !player.IsDead)
            {
                player.SyncSharedState(sharedPlayerHand, sharedPlayerEnergy, sharedPlayerMaxEnergy);
            }
        }

        if (uiGameplay != null)
        {
            uiGameplay.UpdateEnergy(sharedPlayerEnergy, sharedPlayerMaxEnergy);

            if (currentUnit is PlayerUnit_SlingBoom playerUnit)
            {
                uiGameplay.RenderHand(playerUnit);
            }
        }

        if (isTimerRunning)
        {
            Debug.Log("[Manager] Restarting timer due to buff");
            StartTimer();
        }
    }

    private IEnumerator AutoEndTurnAfterDelay()
    {
        if (isWaitingForAutoEnd)
        {
            Debug.Log("[Manager] Auto-end already in progress, skipping");
            yield break;
        }

        isWaitingForAutoEnd = true;
        Debug.Log($"[Manager] ⏳ Starting auto-end countdown: {autoEndTurnDelay}s");

        float elapsed = 0f;
        while (elapsed < autoEndTurnDelay)
        {
            if (!isWaitingForAutoEnd || !isGameStarted || isGameOver)
            {
                Debug.Log("[Manager] Auto-end cancelled");
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (!isWaitingForAutoEnd || !isGameStarted || isGameOver)
        {
            Debug.Log("[Manager] Auto-end cancelled at end");
            yield break;
        }

        int cheapestCost = int.MaxValue;
        foreach (var bulletType in sharedPlayerHand)
        {
            CardData card = GetCardData(bulletType);
            if (card != null && card.energyCost < cheapestCost)
            {
                cheapestCost = card.energyCost;
            }
        }

        Debug.Log($"[Manager] Cheapest card cost: {cheapestCost}, Current energy: {sharedPlayerEnergy}");

        if (sharedPlayerHand.Count == 0 || sharedPlayerEnergy < cheapestCost)
        {
            if (!isGameOver && isGameStarted)
            {
                Debug.Log("[Manager] Auto-ending turn (no cards or energy)");
                isWaitingForAutoEnd = false;
                EndTurn();
            }
            else
            {
                Debug.Log("[Manager] Game over or not started, not auto-ending");
                isWaitingForAutoEnd = false;
            }
        }
        else
        {
            Debug.Log("[Manager] Player still has playable cards, not auto-ending");
            isWaitingForAutoEnd = false;
        }
    }

    // ✅ IMPROVED RESET
    public void ResetGameState()
    {
        Debug.Log("[TurnBasedGameManager] 🔄 Resetting game state...");

        // Stop all coroutines
        StopAllCoroutines();

        // Reset flags
        isGameStarted = false;
        isGameOver = false;
        isTimerRunning = false;
        isWaitingForAutoEnd = false;

        // Reset turn data
        currentTurnIndex = 0;
        currentUnit = null;
        playerTurnsPlayed = 0;

        // Clear collections
        turnOrder.Clear();
        sharedPlayerHand.Clear();

        // Reset energy
        baseMaxEnergy = 3;
        sharedPlayerEnergy = 0;
        sharedPlayerMaxEnergy = baseMaxEnergy;

        // Reset timer
        currentTurnTime = 0;

        // Reset UI
        if (uiGameplay != null)
        {
            uiGameplay.HideAllUI();
        }

        Debug.Log("[TurnBasedGameManager] ✅ Reset complete");
    }
}