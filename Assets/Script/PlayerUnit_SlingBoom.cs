using UnityEngine;
using System.Collections.Generic;

public class PlayerUnit_SlingBoom : GameUnit_SlingBoom
{
    [Header("Player Components")]
    [SerializeField] private DragToShootController_SlingBoom dragController;

    private List<BulletType> hand = new List<BulletType>();
    private BulletType selectedBulletType;
    private bool hasSelectedBullet = false;

    // ✅ FIX BUG 2: Flag để prevent multiple card selections
    private bool isWaitingForShot = false;

    public List<BulletType> CurrentHand => hand;
    public BulletType SelectedBulletType => selectedBulletType;
    public bool IsWaitingForShot => isWaitingForShot; // ✅ Public getter

    protected override void Awake()
    {
        base.Awake();
        team = TeamType.Player;
        if (dragController == null) dragController = GetComponent<DragToShootController_SlingBoom>();
    }

    public void SyncSharedState(List<BulletType> sharedHand, int energy, int maxEnergy)
    {
        hand = new List<BulletType>(sharedHand);
        currentEnergy = energy;
        this.maxEnergy = maxEnergy;
    }

    public override void OnTurnStarted()
    {
        base.OnTurnStarted();

        hasSelectedBullet = false;
        isWaitingForShot = false; // ✅ Reset flag khi turn bắt đầu
        if (dragController) dragController.DisableShootMode();

      
    }

    public void SelectCardFromHand(int index)
    {
        // ✅ FIX BUG 2: Không allow chọn card khi đang chờ bắn
        if (isWaitingForShot)
        {
           
            return;
        }

        if (!isMyTurn) return;
        if (index < 0 || index >= hand.Count) return;

        BulletType typeToCheck = hand[index];

        CardData data = TurnBasedGameManager.Instance.GetCardData(typeToCheck);

        if (data != null && currentEnergy >= data.energyCost)
        {
            ConsumeEnergy(data.energyCost);
            hand.RemoveAt(index);
            TurnBasedGameManager.Instance.UpdateSharedPlayerState(hand, currentEnergy);

            // ✅ Sửa: Lấy UIGameplay từ TurnBasedGameManager
            UIGameplay_SlingBoom uiGameplay = GetUIGameplay();
            if (uiGameplay != null)
            {
                uiGameplay.UpdateEnergy(currentEnergy, maxEnergy);
                uiGameplay.RenderHand(this);
                uiGameplay.ShowPlayerChoice(typeToCheck);
            }

            selectedBulletType = typeToCheck;
            hasSelectedBullet = true;
            isWaitingForShot = true; // ✅ Set flag: đang chờ bắn

            if (dragController) dragController.EnableShootMode();
        }
        else
        {
            Debug.Log("Không đủ năng lượng!");
        }
    }

    public override void Shoot(Vector3 velocity, BulletType type)
    {
        if (!hasSelectedBullet) return;

      
        switch (selectedBulletType)
        {
            case BulletType.Normal: SpawnBullet(normalBulletPrefab, velocity); break;
            case BulletType.Bomb: SpawnBullet(bombBulletPrefab, velocity); break;
            case BulletType.Triple: HandleTripleShot(velocity); break;
        }

        hasSelectedBullet = false;
        isWaitingForShot = false; // ✅ Clear flag: đã bắn xong
        if (dragController) dragController.DisableShootMode();

        TurnBasedGameManager.Instance.UpdateSharedPlayerState(hand, currentEnergy);

        // ✅ Gọi hàm CHECK AUTO END TURN SAU KHI BẮN
        if (TurnBasedGameManager.Instance != null)
        {
            TurnBasedGameManager.Instance.OnPlayerShot();
        }
    }

    public override void OnTurnEnded()
    {
        base.OnTurnEnded();

        hasSelectedBullet = false;
        isWaitingForShot = false; // ✅ Reset flag khi turn kết thúc

        if (dragController) dragController.DisableShootMode();
        TurnBasedGameManager.Instance.UpdateSharedPlayerState(hand, currentEnergy);

        Debug.Log($"[PlayerUnit] {unitName} End Turn.");
    }
}