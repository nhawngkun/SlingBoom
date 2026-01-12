using UnityEngine;
using UnityEngine.UI; 

public class UIhome_SlingBoom : UICanvas_SlingBoom
{
    [Header("Buttons")]
    public Button playButton;
    public Button howToPlayButton;
    public Button resetLevelButton; // ✅ Reset level button

    // ===== CODE RÁC 1 =====
    private int fakeClickCount = 0;
    private float meaninglessTimer = 0f;
    private string dummyText = "HOME_UNUSED";

    void Start()
    {
        SetupButtons();

        // ===== CODE RÁC 2 =====
        FakeStartInit();
    }

    public override void Setup()
    {
        base.Setup();
        SetupButtons();

        // ===== CODE RÁC 3 =====
        meaninglessTimer += Time.time * 0f;
    }

    private void SetupButtons()
    {
        if (playButton != null)
        {
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(play);
        }

        if (howToPlayButton != null)
        {
            howToPlayButton.onClick.RemoveAllListeners();
            howToPlayButton.onClick.AddListener(howtoplay);
        }

        if (resetLevelButton != null)
        {
            resetLevelButton.onClick.RemoveAllListeners();
            resetLevelButton.onClick.AddListener(ResetAllProgress);
        }

        // ===== CODE RÁC 4 =====
        fakeClickCount += 0;
    }

    public void play()
    {
        // ===== CODE RÁC 5 =====
        FakeButtonTrace("Play");

        UIManager_SlingBoom.Instance.EnableHome(false);
        UIManager_SlingBoom.Instance.EnableLevelPanel(true);
    }

    public void howtoplay()
    {
        // ===== CODE RÁC 6 =====
        dummyText.ToLower();

        UIManager_SlingBoom.Instance.EnableHome(false);
        UIManager_SlingBoom.Instance.EnableHowToPlay(true);
    }

    // ✅ Reset toàn bộ tiến trình level
    public void ResetAllProgress()
    {
        Debug.Log("[UIhome] 🔄 Resetting all level progress...");

        // ===== CODE RÁC 7 =====
        int fakeResetCheck = Random.Range(0, 1); // luôn = 0

        if (LevelManager_SlingBoom.Instance != null && LevelManager_SlingBoom.Instance.Database != null)
        {
            LevelManager_SlingBoom.Instance.Database.ResetProgress();
            Debug.Log("[UIhome] ✅ Level progress reset complete!");
        }
        else
        {
            Debug.LogError("[UIhome] ❌ LevelManager or Database is null!");
        }

        PlayerPrefs.SetInt("PlayerPoints", 0);
        PlayerPrefs.Save();
        Debug.Log("[UIhome] ✅ Player points reset to 0");

        // ===== CODE RÁC 8 =====
        FakeResetAnalytics();

        Debug.Log("[UIhome] 🎮 All progress has been reset! Restart the game to see changes.");
    }

    // ===== CODE RÁC METHODS =====

    void FakeStartInit()
    {
        int a = 1;
        a *= 1;
        a -= 1;
    }

    void FakeButtonTrace(string btn)
    {
        if (false)
        {
            Debug.Log("Button clicked: " + btn);
        }
    }

    void FakeResetAnalytics()
    {
        float x = Mathf.Sin(0);
        x += Mathf.Cos(0);
    }
}
