using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIShop_SlingBoom : UICanvas_SlingBoom
{
    [Header("UI Elements")]
    [SerializeField] private Button btnBack;
    
    [Header("Card Display - Left Side")]
    [SerializeField] private Image[] smallCardIcons; // 3 small cards
    [SerializeField] private Button[] smallCardButtons; // Buttons for small cards
    
    [Header("Selected Card Display - Right Side")]
    [SerializeField] private Image bigCardIcon; // Large card preview
    [SerializeField] private TextMeshProUGUI cardNameText; // Card name
    [SerializeField] private TextMeshProUGUI cardDescriptionText; // Card description
    
    private int selectedCardIndex = 0;
    
    protected override void Awake()
    {
        base.Awake();
    }
    
    public override void Setup()
    {
        base.Setup();
        
        if (btnBack)
        {
            btnBack.onClick.RemoveAllListeners();
            btnBack.onClick.AddListener(OnBackClicked);
        }
        
        // Setup small card buttons
        if (smallCardButtons != null)
        {
            for (int i = 0; i < smallCardButtons.Length; i++)
            {
                int index = i;
                if (smallCardButtons[i] != null)
                {
                    smallCardButtons[i].onClick.RemoveAllListeners();
                    smallCardButtons[i].onClick.AddListener(() => OnCardSelected(index));
                }
            }
        }
        
        DisplayAllCards();
        SelectCard(0); // Select first card by default
    }
    
    public override void Open()
    {
        base.Open();
        DisplayAllCards();
        SelectCard(selectedCardIndex);
    }
    
    private void DisplayAllCards()
    {
        if (TurnBasedGameManager.Instance == null) return;
        
        var cardDatabase = TurnBasedGameManager.Instance.cardDatabase;
        
        if (cardDatabase == null || cardDatabase.Count == 0) return;
        
        // Display small cards on the left
        for (int i = 0; i < cardDatabase.Count && i < smallCardIcons.Length; i++)
        {
            CardData card = cardDatabase[i];
            
            if (smallCardIcons[i] != null && card.cardIcon != null)
            {
                smallCardIcons[i].sprite = card.cardIcon;
            }
        }
    }
    
    private void OnCardSelected(int index)
    {
        SelectCard(index);
        
        
    }
    
    private void SelectCard(int index)
    {
        if (TurnBasedGameManager.Instance == null) return;
        
        var cardDatabase = TurnBasedGameManager.Instance.cardDatabase;
        
        if (cardDatabase == null || index < 0 || index >= cardDatabase.Count) return;
        
        selectedCardIndex = index;
        CardData selectedCard = cardDatabase[index];
        
        // Display big card preview
        if (bigCardIcon != null && selectedCard.cardIcon != null)
        {
            bigCardIcon.sprite = selectedCard.cardIcon;
        }
        
        // Display card name
        if (cardNameText != null)
        {
            cardNameText.text = GetCardName(selectedCard.type);
        }
        
        // Display card description
        if (cardDescriptionText != null)
        {
            cardDescriptionText.text = GetCardDescription(selectedCard.type);
        }
    }
    
    private string GetCardName(BulletType type)
    {
        switch (type)
        {
            case BulletType.Normal:
                return "Normal Shot";
            case BulletType.Bomb:
                return "Bomb";
            case BulletType.Triple:
                return "Triple Shot";
            default:
                return "Unknown";
        }
    }
    
    private string GetCardDescription(BulletType type)
    {
        switch (type)
        {
            case BulletType.Normal:
                return "A standard bullet with balanced stats";
            case BulletType.Bomb:
                return "A powerful explosive with large blast radius";
            case BulletType.Triple:
                return "Fires three bullets at once";
            default:
                return "";
        }
    }
    
    private void OnBackClicked()
    {
        if (UIManager_SlingBoom.Instance != null)
        {
            UIManager_SlingBoom.Instance.EnableShop(false);
            UIManager_SlingBoom.Instance.EnableLevelPanel(true);
        }
        
       
    }
}