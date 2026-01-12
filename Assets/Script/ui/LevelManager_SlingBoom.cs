using UnityEngine;
using System.Collections;

public class LevelManager_SlingBoom : Singleton<LevelManager_SlingBoom>
{
    [Header("Level Database")]
    [SerializeField] private LevelDatabase levelDatabase;
    
    [Header("Level Parent")]
    [SerializeField] private Transform levelParent;
    
    private GameObject currentLevelInstance;
    private LevelDatabase.LevelInfo currentLoadedLevel;
    
    public LevelDatabase Database => levelDatabase;
    public LevelDatabase.LevelInfo CurrentLevel => currentLoadedLevel;
    
    public override void Awake()
    {
        base.Awake();
        
        if (levelDatabase != null)
        {
            levelDatabase.LoadProgress();
        }
    }
    
    // Load level by index
    public void LoadLevel(int levelIndex)
    {
        if (levelDatabase == null)
        {
            Debug.LogError("LevelDatabase is null!");
            return;
        }
        
        LevelDatabase.LevelInfo levelInfo = levelDatabase.GetLevel(levelIndex);
        
        if (levelInfo == null)
        {
            Debug.LogError($"Level {levelIndex} not found!");
            return;
        }
        
        if (!levelInfo.isUnlocked)
        {
            Debug.LogWarning($"Level {levelIndex} is locked!");
            return;
        }
        
        levelDatabase.currentLevelIndex = levelIndex;
        StartCoroutine(LoadLevelWithDelay(levelInfo));
    }
    
    // Load current level
    public void LoadCurrentLevel()
    {
        if (levelDatabase == null) return;
        
        LevelDatabase.LevelInfo levelInfo = levelDatabase.GetCurrentLevel();
        if (levelInfo != null)
        {
            StartCoroutine(LoadLevelWithDelay(levelInfo));
        }
    }
    
    // ✅ HÀM MỚI: Load level với delay để đảm bảo mọi thứ sẵn sàng
    private IEnumerator LoadLevelWithDelay(LevelDatabase.LevelInfo levelInfo)
    {
        Debug.Log($"[LevelManager] 🎮 Loading level: {levelInfo.levelName}");
        
        // ✅ BƯỚC 1: ĐÓNG TẤT CẢ UI TRƯỚC
        if (UIManager_SlingBoom.Instance != null)
        {
            Debug.Log("[LevelManager] Closing all UI...");
            UIManager_SlingBoom.Instance.CloseAll();
        }
        
        // ✅ CHỜ 1 FRAME để UI đóng hoàn toàn
        yield return null;
        
        // ✅ BƯỚC 2: RESET GAME STATE TRƯỚC KHI CLEAR LEVEL
        if (TurnBasedGameManager.Instance != null)
        {
            Debug.Log("[LevelManager] Resetting game state...");
            TurnBasedGameManager.Instance.ResetGameState();
        }
        
        // ✅ BƯỚC 3: Clear old level
        ClearCurrentLevel();
        
        // ✅ BƯỚC 4: CHỜ 2 FRAMES để Unity cleanup hoàn toàn
        yield return null;
        yield return null;
        
        // Instantiate new level
        if (levelInfo.levelPrefab == null)
        {
            Debug.LogError("[LevelManager] Level prefab is null!");
            yield break;
        }
        
        Transform parent = levelParent != null ? levelParent : null;
        currentLevelInstance = Instantiate(levelInfo.levelPrefab, parent);
        currentLoadedLevel = levelInfo;
        
        Debug.Log($"✅ Level instantiated: {levelInfo.levelName}");
        
        // ✅ BƯỚC 5: CHỜ 2 FRAMES để level được instantiate hoàn toàn
        yield return null;
        yield return null;
        
        // ✅ BƯỚC 6: KIỂM TRA XEM CÓ UNITS TRONG SCENE CHƯA
        var testUnits = FindObjectsByType<GameUnit_SlingBoom>(FindObjectsSortMode.None);
        Debug.Log($"[LevelManager] Found {testUnits.Length} units in scene");
        
        if (testUnits.Length == 0)
        {
            Debug.LogError("[LevelManager] ❌ No units found in scene!");
            yield break;
        }
        
        // ✅ BƯỚC 7: SHOW GAMEPLAY UI
        if (UIManager_SlingBoom.Instance != null)
        {
            Debug.Log("[LevelManager] Showing gameplay UI...");
            UIManager_SlingBoom.Instance.EnableGameplay(true);
        }
        
        // ✅ BƯỚC 8: CHỜ 1 FRAME để UI hiển thị
        yield return null;
        
        // ✅ BƯỚC 9: GỌI TURNBASEDGAMEMANAGER BẮT ĐẦU GAME
        if (TurnBasedGameManager.Instance == null)
        {
            Debug.LogError("❌ TurnBasedGameManager.Instance is null!");
            yield break;
        }
        
        Debug.Log("[LevelManager] Starting game via TurnBasedGameManager...");
        TurnBasedGameManager.Instance.StartGame();
        
        Debug.Log("[LevelManager] ✅ Level load complete!");
    }
    
    // Clear current level
    public void ClearCurrentLevel()
    {
        Debug.Log("[LevelManager] 🗑️ Clearing current level...");
        
        if (currentLevelInstance != null)
        {
            Destroy(currentLevelInstance);
            currentLevelInstance = null;
            Debug.Log("[LevelManager] Level instance destroyed");
        }
        
        currentLoadedLevel = null;
    }
    
    // Complete current level
    public void CompleteCurrentLevel(int stars = 3)
    {
        if (levelDatabase == null || currentLoadedLevel == null) return;
        
        int levelIndex = levelDatabase.allLevels.IndexOf(currentLoadedLevel);
        
        if (levelIndex >= 0)
        {
            levelDatabase.CompleteLevel(levelIndex, stars);
            Debug.Log($"[LevelManager] Level {levelIndex} completed with {stars} stars");
        }
    }
    
    // Restart current level
    public void RestartCurrentLevel()
    {
        if (currentLoadedLevel != null)
        {
            StartCoroutine(LoadLevelWithDelay(currentLoadedLevel));
        }
    }
    
    // Next level
    public void LoadNextLevel()
    {
        if (levelDatabase == null) return;
        
        Debug.Log("[LevelManager] Loading next level...");
        
        // ✅ MOVE TO NEXT LEVEL TRƯỚC
        levelDatabase.MoveToNextLevel();
        
        // ✅ SAU ĐÓ LOAD LEVEL ĐÓ
        LoadCurrentLevel();
    }
    
    // Back to level select
    public void BackToLevelSelect()
    {
        Debug.Log("[LevelManager] 🔙 Back to level select");
        
        // ✅ STOP GAME TRƯỚC KHI CLEAR
        if (TurnBasedGameManager.Instance != null)
        {
            TurnBasedGameManager.Instance.StopGame();
        }
        
        ClearCurrentLevel();
        
        if (UIManager_SlingBoom.Instance != null)
        {
            UIManager_SlingBoom.Instance.CloseAll();
            UIManager_SlingBoom.Instance.EnableLevelPanel(true);
        }
    }
    
    // ✅ FIXED: Back to home - STOP GAME TRƯỚC
    public void BackToHome()
    {
        Debug.Log("[LevelManager] 🏠 Back to home");
        
        // ✅ QUAN TRỌNG: STOP GAME TRƯỚC KHI CLEAR LEVEL
        if (TurnBasedGameManager.Instance != null)
        {
            Debug.Log("[LevelManager] Stopping game...");
            TurnBasedGameManager.Instance.StopGame();
        }
        
        // ✅ Clear level
        ClearCurrentLevel();
        
        // ✅ Close all UI và về home
        if (UIManager_SlingBoom.Instance != null)
        {
            UIManager_SlingBoom.Instance.CloseAll();
            UIManager_SlingBoom.Instance.EnableHome(true);
        }
        
        Debug.Log("[LevelManager] ✅ Back to home complete");
    }
}