using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIChooseLevel_SlingBoom : UICanvas_SlingBoom
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI levelProgressText; // "3/15"
    [SerializeField] private Button btnPlay; // PLAY button
    [SerializeField] private Button btnDeck; // DECK button (shop)
    [SerializeField] private Button btnBack; // BACK button
    [SerializeField] private Button btnSound; // Sound toggle
    [SerializeField] private Button btnVibrate; // Vibrate toggle

    [Header("Progress System")]
    [SerializeField] private Image levelProgressFillImage; // Image fill hiển thị tiến độ level (0-100%)
    [SerializeField] private Image progressFillImage; // Image fill hiển thị tiến độ rank (0-100%)
    [SerializeField] private TextMeshProUGUI rankStartText; // Text "0" đầu
    [SerializeField] private TextMeshProUGUI rankEndText; // Text "100" cuối
    [SerializeField] private TextMeshProUGUI currentPointsText; // Text hiện tại có bao nhiêu điểm
    [SerializeField] private Image rankBadgeImage; // Ảnh thể hiện rank

    [Header("Sound & Music Toggle")]
    [SerializeField] private Button btnMusicToggle; // Toggle Music
    [SerializeField] private Image musicToggleImage; // Ảnh toggle Music
    [SerializeField] private Sprite musicOnSprite; // Ảnh khi bật
    [SerializeField] private Sprite musicOffSprite; // Ảnh khi tắt

    [SerializeField] private Button btnSoundToggle; // Toggle Sound
    [SerializeField] private Image soundToggleImage; // Ảnh toggle Sound
    [SerializeField] private Sprite soundOnSprite; // Ảnh khi bật
    [SerializeField] private Sprite soundOffSprite; // Ảnh khi tắt

    [Header("Rank System")]
    [SerializeField] private int[] rankThresholds = { 0, 100, 300, 600, 1000 }; // Điểm cần để lên rank
    [SerializeField] private Sprite[] rankBadges; // Array chứa ảnh rank từ 0-4

    // Trạng thái audio riêng biệt
    private bool isMusicOn = true;
    private bool isSoundOn = true;

    protected override void Awake()
    {
        base.Awake();
    }

    public override void Setup()
    {
        base.Setup();

        if (btnPlay)
        {
            btnPlay.onClick.RemoveAllListeners();
            btnPlay.onClick.AddListener(OnPlayClicked);
        }

        if (btnDeck)
        {
            btnDeck.onClick.RemoveAllListeners();
            btnDeck.onClick.AddListener(OnDeckClicked);
        }

        if (btnBack)
        {
            btnBack.onClick.RemoveAllListeners();
            btnBack.onClick.AddListener(OnBackClicked);
        }

        if (btnSound)
        {
            btnSound.onClick.RemoveAllListeners();
            btnSound.onClick.AddListener(OnSoundClicked);
        }

        if (btnVibrate)
        {
            btnVibrate.onClick.RemoveAllListeners();
            btnVibrate.onClick.AddListener(OnVibrateClicked);
        }

        // âœ… Setup Music Toggle
        if (btnMusicToggle)
        {
            btnMusicToggle.onClick.RemoveAllListeners();
            btnMusicToggle.onClick.AddListener(OnMusicToggleClicked);
        }

        // âœ… Setup Sound Toggle
        if (btnSoundToggle)
        {
            btnSoundToggle.onClick.RemoveAllListeners();
            btnSoundToggle.onClick.AddListener(OnSoundToggleClicked);
        }

        UpdateLevelInfo();
        UpdateProgressDisplay();
        UpdateAudioToggleUI();
    }

    public override void Open()
    {
        base.Open();
        UpdateLevelInfo();
        UpdateProgressDisplay();
        UpdateAudioToggleUI();
    }

    // âœ… HÃ€M Cáº­p nháº­t tiến Ä'á»™ rank vÃ  score
    private void UpdateProgressDisplay()
    {
        int currentPoints = GetCurrentPoints(); // LÃ¡y Ä'iá»ƒm hiá»‡n táº¡i (từ PlayerPrefs hoặc game manager)
        int currentRank = GetCurrentRank(currentPoints);

        // âœ… Láº¥y range Ä'iá»ƒm cho rank hiá»‡n táº¡i
        int rankStart = rankThresholds[currentRank];
        int rankEnd = (currentRank < rankThresholds.Length - 1) ? rankThresholds[currentRank + 1] : rankThresholds[currentRank] + 500;

        // âœ… Update text rank (0-100, 100-300, etc.)
        if (rankStartText) rankStartText.text = rankStart.ToString();
        if (rankEndText) rankEndText.text = rankEnd.ToString();

        // âœ… Update text Ä'iá»ƒm hiá»‡n táº¡i
        if (currentPointsText) currentPointsText.text = currentPoints.ToString();

        // âœ… Update progress fill (0-1)
        float progress = (float)(currentPoints - rankStart) / (rankEnd - rankStart);
        progress = Mathf.Clamp01(progress); // Giá»›i háº¡n từ 0-1

        if (progressFillImage)
        {
            progressFillImage.fillAmount = progress;
        }

        // âœ… Update level progress fill (báº­c level Ä'Ã£ hoÃ n thÃ nh)
        UpdateLevelProgressFill();

        // âœ… Update rank badge image (nếu cÃ³)
        if (rankBadgeImage && rankBadges != null && currentRank < rankBadges.Length)
        {
            rankBadgeImage.sprite = rankBadges[currentRank];
        }

        Debug.Log($"[UIChooseLevel] Rank: {currentRank}, Points: {currentPoints}, Progress: {progress:F2}");
    }

    // âœ… HÃ€M Cáº­p nháº­t Level Progress Fill
    private void UpdateLevelProgressFill()
    {
        if (LevelManager_SlingBoom.Instance == null || LevelManager_SlingBoom.Instance.Database == null)
        {
            return;
        }

        LevelDatabase db = LevelManager_SlingBoom.Instance.Database;
        int currentLevelIndex = db.currentLevelIndex;
        int totalLevels = db.allLevels.Count;

        float levelProgress = totalLevels > 0 ? (float)currentLevelIndex / totalLevels : 0f;

        if (levelProgressFillImage)
        {
            levelProgressFillImage.fillAmount = levelProgress;
        }

        Debug.Log($"[UIChooseLevel] Level Progress: {currentLevelIndex}/{totalLevels} = {levelProgress:F2}");
    }

    // âœ… HÃ€M LÃ¡y Ä'iá»ƒm hiá»‡n táº¡i
    private int GetCurrentPoints()
    {
        // LÃ¡y từ PlayerPrefs (lÆ°u theo cáº­u hÃ¬nh game)
        return PlayerPrefs.GetInt("PlayerPoints", 0);
    }

    // âœ… HÃ€M TÃ­nh rank báº­t Ä'áº§u từ Ä'iá»ƒm hiá»‡n táº¡i
    private int GetCurrentRank(int points)
    {
        for (int i = rankThresholds.Length - 1; i >= 0; i--)
        {
            if (points >= rankThresholds[i])
            {
                return i;
            }
        }
        return 0;
    }

    // âœ… HÃ€M Cáº­p nháº­t UI audio toggle
    private void UpdateAudioToggleUI()
    {
        // Update Music Toggle Image
        if (musicToggleImage)
        {
            musicToggleImage.sprite = isMusicOn ? musicOnSprite : musicOffSprite;
        }

        // Update Sound Toggle Image
        if (soundToggleImage)
        {
            soundToggleImage.sprite = isSoundOn ? soundOnSprite : soundOffSprite;
        }

        Debug.Log($"[UIChooseLevel] Audio UI Updated - Music: {isMusicOn}, Sound: {isSoundOn}");
    }

    // âœ… HÃ€M Music Toggle - CHáº¶ Táº¯T/Báº¬T MUSIC (nhạc nền)
    private void OnMusicToggleClicked()
    {
        isMusicOn = !isMusicOn;

        if (SoundManager_SlingBoom.Instance != null)
        {
            if (isMusicOn)
            {
                // Báº¬T MUSIC
                SoundManager_SlingBoom.Instance.SetMusicVolume(0.3f);
            }
            else
            {
                // Táº¯T MUSIC
                SoundManager_SlingBoom.Instance.SetMusicVolume(0f);
            }
        }

        UpdateAudioToggleUI();

        // Phát sound confirm (nếu Sound bật)
        if (isSoundOn && SoundManager_SlingBoom.Instance != null)
        {
            SoundManager_SlingBoom.Instance.PlayVFXSound(1);
        }

        Debug.Log($"[UIChooseLevel] Music toggled: {isMusicOn}");
    }

    // âœ… HÃ€M Sound Toggle - CHáº¶ Táº¯T/Báº¬T SOUND VFX (Ã¢m thanh hiá»‡u Ãºng)
    private void OnSoundToggleClicked()
    {
        isSoundOn = !isSoundOn;

        if (SoundManager_SlingBoom.Instance != null)
        {
            if (isSoundOn)
            {
                // Báº¬T SOUND
                SoundManager_SlingBoom.Instance.SetSFXVolume(0.5f);
            }
            else
            {
                // Táº¯T SOUND
                SoundManager_SlingBoom.Instance.SetSFXVolume(0f);
            }
        }

        UpdateAudioToggleUI();

        // Phát sound confirm (nếu Sound bật)
        if (isSoundOn && SoundManager_SlingBoom.Instance != null)
        {
            SoundManager_SlingBoom.Instance.PlayVFXSound(1);
        }

        Debug.Log($"[UIChooseLevel] Sound toggled: {isSoundOn}");
    }

    private void UpdateLevelInfo()
    {
        if (LevelManager_SlingBoom.Instance == null || LevelManager_SlingBoom.Instance.Database == null)
        {
            Debug.LogWarning("LevelManager or Database is null!");
            return;
        }

        LevelDatabase db = LevelManager_SlingBoom.Instance.Database;
        LevelDatabase.LevelInfo currentLevel = db.GetCurrentLevel();

        if (currentLevel == null)
        {
            Debug.LogWarning("Current level is null!");
            return;
        }

        // Update progress text (e.g., "3/15")
        if (levelProgressText != null)
        {
            int currentIndex = db.currentLevelIndex + 1;
            int totalLevels = db.allLevels.Count;
            levelProgressText.text = $"{currentIndex}/{totalLevels}";
        }

        if (btnPlay != null)
        {
            btnPlay.interactable = currentLevel.isUnlocked;
        }
    }

    private void OnPlayClicked()
    {
        if (LevelManager_SlingBoom.Instance != null)
        {
            LevelManager_SlingBoom.Instance.LoadCurrentLevel();
        }

        if (SoundManager_SlingBoom.Instance != null)
        {
            SoundManager_SlingBoom.Instance.PlayVFXSound(1);
        }
    }

    private void OnDeckClicked()
    {
        if (UIManager_SlingBoom.Instance != null)
        {
            UIManager_SlingBoom.Instance.EnableLevelPanel(false);
            UIManager_SlingBoom.Instance.EnableShop(true);
        }

        
    }

    private void OnBackClicked()
    {
        if (UIManager_SlingBoom.Instance != null)
        {
            UIManager_SlingBoom.Instance.EnableLevelPanel(false);
            UIManager_SlingBoom.Instance.EnableHome(true);
        }

       
    }

    private void OnSoundClicked()
    {
        if (SoundManager_SlingBoom.Instance != null)
        {
            SoundManager_SlingBoom.Instance.TurnOn = !SoundManager_SlingBoom.Instance.TurnOn;
            UpdateAudioToggleUI();
        }
    }

    private void OnVibrateClicked()
    {
        // Toggle vibrate setting
        Debug.Log("Vibrate toggled");
    }
}