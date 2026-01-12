using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using DG.Tweening;

public class UIGameplay_SlingBoom : UICanvas_SlingBoom
{
    [Header("--- PLAYER CONTROLS ---")]
    [SerializeField] private Button[] cardSlots;
    [SerializeField] private Image[] cardIcons;
    [SerializeField] private TextMeshProUGUI[] cardCostTexts;
    [SerializeField] private TextMeshProUGUI energyText;
    [SerializeField] private Image energyLabelImage;
    [SerializeField] private Image playerChoiceImg;
    [SerializeField] private Button btnEndTurn;

    [Header("--- ENEMY DISPLAY ---")]
    [SerializeField] private Image enemyChoiceImg;

    [Header("--- TURN INDICATOR ---")]
    [SerializeField] private TextMeshProUGUI turnText;

    [Header("--- TIMER ---")]
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("--- HEALTH BARS - IMAGE FILL ---")]
    [SerializeField] private Image player1HealthFill;
    [SerializeField] private Image player2HealthFill;
    [SerializeField] private Image enemy1HealthFill;
    [SerializeField] private Image enemy2HealthFill;

    [Header("--- ✅ DAMAGE TEXT ---")]
    [SerializeField] private TextMeshProUGUI player1DamageText;
    [SerializeField] private TextMeshProUGUI player2DamageText;
    [SerializeField] private TextMeshProUGUI enemy1DamageText;
    [SerializeField] private TextMeshProUGUI enemy2DamageText;

    [Header("--- TOP BUTTONS ---")]
    [SerializeField] private Button btnHome;
    [SerializeField] private Button btnRestart;

    private PlayerUnit_SlingBoom currentPlayer;
    private Dictionary<GameUnit_SlingBoom, Image> healthBarMap = new Dictionary<GameUnit_SlingBoom, Image>();
    private Dictionary<GameUnit_SlingBoom, TextMeshProUGUI> damageTextMap = new Dictionary<GameUnit_SlingBoom, TextMeshProUGUI>();

    protected override void Awake()
    {
        base.Awake();
    }

    public override void Setup()
    {
        base.Setup();

        Debug.Log("[UIGameplay] 🔧 Setup() called");

        if (playerChoiceImg) playerChoiceImg.gameObject.SetActive(false);
        if (enemyChoiceImg) enemyChoiceImg.gameObject.SetActive(false);

        if (btnEndTurn)
        {
            btnEndTurn.onClick.RemoveAllListeners();
            btnEndTurn.onClick.AddListener(() =>
            {
                if (TurnBasedGameManager.Instance != null)
                    TurnBasedGameManager.Instance.EndTurn();
            });
        }

        if (btnHome)
        {
            btnHome.onClick.RemoveAllListeners();
            btnHome.onClick.AddListener(OnHomeButtonClicked);
        }

        if (btnRestart)
        {
            btnRestart.onClick.RemoveAllListeners();
            btnRestart.onClick.AddListener(OnRestartButtonClicked);
        }

        // ✅ RESET MAPS
        healthBarMap.Clear();
        damageTextMap.Clear();

        // ✅ ẨN TẤT CẢ DAMAGE TEXT BAN ĐẦU
        HideAllDamageTexts();
    }

    // ✅ HÀM MỚI: Ẩn tất cả damage text
    private void HideAllDamageTexts()
    {
        if (player1DamageText) player1DamageText.gameObject.SetActive(false);
        if (player2DamageText) player2DamageText.gameObject.SetActive(false);
        if (enemy1DamageText) enemy1DamageText.gameObject.SetActive(false);
        if (enemy2DamageText) enemy2DamageText.gameObject.SetActive(false);
    }

    private void OnHomeButtonClicked()
    {
        Debug.Log("[UIGameplay] 🏠 Home button clicked");

        if (UIManager_SlingBoom.Instance != null)
        {
            UIManager_SlingBoom.Instance.CloseAll();
        }

        if (LevelManager_SlingBoom.Instance != null)
        {
            LevelManager_SlingBoom.Instance.BackToHome();
        }

        
    }

    private void OnRestartButtonClicked()
    {
        

        if (UIManager_SlingBoom.Instance != null)
        {
            UIManager_SlingBoom.Instance.CloseAll();
        }

        StartCoroutine(RestartAfterDelay());

        if (SoundManager_SlingBoom.Instance != null)
        {
            SoundManager_SlingBoom.Instance.PlayVFXSound(1);
        }
    }

    private System.Collections.IEnumerator RestartAfterDelay()
    {
        yield return null;

        if (LevelManager_SlingBoom.Instance != null)
        {
         
            LevelManager_SlingBoom.Instance.RestartCurrentLevel();
        }
    }

    // ✅ UPDATED: RegisterUnit - Thêm damage text vào map
    public void RegisterUnit(GameUnit_SlingBoom unit, int playerIndex = -1, int enemyIndex = -1)
    {
        if (unit == null) return;

        Image fill = null;
        TextMeshProUGUI damageText = null;

        if (unit is PlayerUnit_SlingBoom)
        {
            if (playerIndex == 0)
            {
                fill = player1HealthFill;
                damageText = player1DamageText;
            }
            else if (playerIndex == 1)
            {
                fill = player2HealthFill;
                damageText = player2DamageText;
            }
        }
        else if (unit is EnemyAIUnit_SlingBoom)
        {
            if (enemyIndex == 0)
            {
                fill = enemy1HealthFill;
                damageText = enemy1DamageText;
            }
            else if (enemyIndex == 1)
            {
                fill = enemy2HealthFill;
                damageText = enemy2DamageText;
            }
        }

        if (fill != null)
        {
            healthBarMap[unit] = fill;
            fill.fillAmount = 1f;
        }

        // ✅ ĐĂNG KÝ DAMAGE TEXT
        if (damageText != null)
        {
            damageTextMap[unit] = damageText;
            damageText.gameObject.SetActive(false);
        }
    }

    public void UpdateHealthBar(GameUnit_SlingBoom unit)
    {
        if (unit == null || !healthBarMap.ContainsKey(unit)) return;

        Image fill = healthBarMap[unit];
        if (fill != null)
        {
            fill.fillAmount = (float)unit.CurrentHealth / unit.MaxHealth;
        }
    }

    // ✅ HÀM MỚI: Hiển thị damage text
    public void ShowDamageText(GameUnit_SlingBoom unit, int damage)
    {
        if (unit == null || !damageTextMap.ContainsKey(unit)) return;

        TextMeshProUGUI damageText = damageTextMap[unit];
        if (damageText == null) return;

        // Kill tween cũ nếu có
        damageText.DOKill();

        // Set text
        damageText.text = $"-{damage}";
        damageText.color = Color.red;

        // Show
        damageText.gameObject.SetActive(true);

        // Animation: Fade in -> Wait -> Fade out
        damageText.alpha = 0f;

        Sequence seq = DOTween.Sequence();
        seq.Append(damageText.DOFade(1f, 0.2f)); // Fade in
        seq.AppendInterval(0.8f); // Giữ 0.8s
        seq.Append(damageText.DOFade(0f, 0.3f)); // Fade out
        seq.OnComplete(() => damageText.gameObject.SetActive(false));

        // Scale animation
        damageText.transform.localScale = Vector3.one * 0.5f;
        damageText.transform.DOScale(1.2f, 0.2f).SetEase(Ease.OutBack)
            .OnComplete(() => damageText.transform.DOScale(1f, 0.2f));
    }

    public void ShowHealthBarForUnit(GameUnit_SlingBoom unit)
    {
        if (unit != null)
        {
            UpdateHealthBar(unit);
        }
    }

    public void UpdateEnergy(int current, int max)
    {
        if (energyText) energyText.text = $"{current}/{max}";
      
    }

    public void UpdateTimer(float timeRemaining)
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timeRemaining / 60f);
            int seconds = Mathf.FloorToInt(timeRemaining % 60f);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

            if (timeRemaining <= 10f)
            {
                timerText.color = Color.red;
            }
            else if (timeRemaining <= 20f)
            {
                timerText.color = Color.yellow;
            }
            else
            {
                timerText.color = Color.white;
            }
        }
    }

    public void RenderHand(PlayerUnit_SlingBoom player)
    {
       

        currentPlayer = player;
        List<BulletType> hand = player.CurrentHand;

        for (int i = 0; i < cardSlots.Length; i++)
        {
            if (i < hand.Count)
            {
                cardSlots[i].gameObject.SetActive(true);

                BulletType type = hand[i];
                CardData data = TurnBasedGameManager.Instance.GetCardData(type);

                if (data != null)
                {
                    if (cardIcons[i] != null)
                    {
                        cardIcons[i].sprite = data.cardIcon;
                        Color c = cardIcons[i].color;
                        c.a = 1f;
                        cardIcons[i].color = c;
                    }

                    if (cardCostTexts != null && i < cardCostTexts.Length && cardCostTexts[i] != null)
                    {
                        cardCostTexts[i].text = data.energyCost.ToString();

                        if (player.CurrentEnergy < data.energyCost)
                        {
                            cardCostTexts[i].color = Color.red;
                        }
                        else
                        {
                            cardCostTexts[i].color = Color.white;
                        }
                    }

                    bool canUse = player.CurrentEnergy >= data.energyCost && !player.IsWaitingForShot;
                    cardSlots[i].interactable = canUse;

                    
                }

                int index = i;
                cardSlots[i].onClick.RemoveAllListeners();
                cardSlots[i].onClick.AddListener(() => OnCardClicked(index));
            }
            else
            {
                cardSlots[i].gameObject.SetActive(false);
            }
        }

       
    }

    private void OnCardClicked(int index)
    {
      

        if (currentPlayer != null)
        {
            currentPlayer.SelectCardFromHand(index);
        }
    }

    public void ShowPlayerChoice(BulletType type)
    {
        if (playerChoiceImg)
        {
            CardData data = TurnBasedGameManager.Instance.GetCardData(type);
            if (data != null)
            {
                playerChoiceImg.sprite = data.cardIcon;
                playerChoiceImg.gameObject.SetActive(true);
            }
        }
    }

    public void ShowEnemyChoice(BulletType type)
    {
        if (enemyChoiceImg)
        {
            CardData data = TurnBasedGameManager.Instance.GetCardData(type);
            if (data != null)
            {
                enemyChoiceImg.sprite = data.cardIcon;
                enemyChoiceImg.gameObject.SetActive(true);
            }
        }
    }

    public void HideChoices()
    {
        if (playerChoiceImg) playerChoiceImg.gameObject.SetActive(false);
        if (enemyChoiceImg) enemyChoiceImg.gameObject.SetActive(false);
    }

    public void TogglePlayerControls(bool isPlayerTurn)
    {
        Debug.Log($"[UIGameplay] 🎮 TogglePlayerControls: {isPlayerTurn}");

        ShowEndTurnButton(isPlayerTurn);

        if (isPlayerTurn && cardSlots != null)
        {
            foreach (var slot in cardSlots)
            {
                if (slot != null && slot.gameObject.activeSelf)
                {
                    slot.enabled = true;

                    Image img = slot.GetComponent<Image>();
                    if (img != null)
                    {
                        Color c = img.color;
                        c.a = 1f;
                        img.color = c;
                    }

                  
                }
            }
        }
    }

    public void UpdateTurnText(string text)
    {
        if (turnText) turnText.text = text;
      
    }

    public void HideCards()
    {
      

        for (int i = 0; i < cardSlots.Length; i++)
        {
            if (cardSlots[i] != null && cardSlots[i].gameObject.activeSelf)
            {
                if (cardIcons[i] != null)
                {
                    Color c = cardIcons[i].color;
                    c.a = 0.3f;
                    cardIcons[i].color = c;
                }
            }
        }
    }

    public void HideAllUI()
    {
        

        if (cardSlots != null)
        {
            foreach (var slot in cardSlots)
            {
                if (slot != null)
                {
                    slot.gameObject.SetActive(false);
                }
            }
        }

        if (btnEndTurn) btnEndTurn.gameObject.SetActive(false);
        if (energyText) energyText.gameObject.SetActive(false);
        if (energyLabelImage) energyLabelImage.gameObject.SetActive(false);
        if (playerChoiceImg) playerChoiceImg.gameObject.SetActive(false);
        if (enemyChoiceImg) enemyChoiceImg.gameObject.SetActive(false);

        if (timerText) timerText.text = "00:00";

        // ✅ ẨN DAMAGE TEXT KHI ẨN UI
        HideAllDamageTexts();
    }

    public void ShowEndTurnButton(bool show)
    {
        if (btnEndTurn != null)
        {
            btnEndTurn.gameObject.SetActive(show);
        }

        if (energyText != null)
        {
            energyText.gameObject.SetActive(show);
        }
        if (energyLabelImage != null)
        {
            energyLabelImage.gameObject.SetActive(show);
        }
    }

    public void ShowPlayerUI()
    {
       

        if (btnEndTurn != null) btnEndTurn.gameObject.SetActive(true);
        if (energyText != null) energyText.gameObject.SetActive(true);
        if (energyLabelImage != null) energyLabelImage.gameObject.SetActive(true);
    }

    public void ShowEnemyUI()
    {
       
    }
}