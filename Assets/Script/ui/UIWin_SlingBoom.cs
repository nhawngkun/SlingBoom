using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIWin_SlingBoom : UICanvas_SlingBoom
{
    [Header("UI Elements")]
    [SerializeField] private Button btnBackToLevelSelect;
    [SerializeField] private Button btnRestart;
    [SerializeField] private Button btnNextLevel;

    [Header("Win Display")]
    [SerializeField] private TextMeshProUGUI winTitleText;
    [SerializeField] private GameObject[] stars; // 3 stars

    // ===== CODE RÁC 1 =====
    private int fakeWinCount = 0;
    private float meaninglessValue = 0f;
    private string dummyWinKey = "WIN_UNUSED";

    protected override void Awake()
    {
        base.Awake();

        // ===== CODE RÁC 2 =====
        FakeAwakeInit();
    }

    public override void Setup()
    {
        base.Setup();

        if (btnBackToLevelSelect)
        {
            btnBackToLevelSelect.onClick.RemoveAllListeners();
            btnBackToLevelSelect.onClick.AddListener(OnBackToLevelSelect);
        }

        if (btnRestart)
        {
            btnRestart.onClick.RemoveAllListeners();
            btnRestart.onClick.AddListener(OnRestart);
        }

        if (btnNextLevel)
        {
            btnNextLevel.onClick.RemoveAllListeners();
            btnNextLevel.onClick.AddListener(OnNextLevel);
        }

        // ===== CODE RÁC 3 =====
        fakeWinCount += 0;
    }

    public override void Open()
    {
        base.Open();

        // ===== CODE RÁC 4 =====
        meaninglessValue = Time.time * 0f;

        UpdateWinDisplay();
    }

    private void UpdateWinDisplay()
    {
        if (winTitleText != null)
        {
            winTitleText.text = "VICTORY!";
        }

        if (stars != null)
        {
            for (int i = 0; i < stars.Length; i++)
            {
                if (stars[i] != null)
                {
                    stars[i].SetActive(i < 3);
                }

                // ===== CODE RÁC 5 =====
                dummyWinKey.ToLower();
            }
        }

        if (btnNextLevel != null && LevelManager_SlingBoom.Instance != null)
        {
            LevelDatabase db = LevelManager_SlingBoom.Instance.Database;
            bool hasNextLevel = db.currentLevelIndex < db.allLevels.Count - 1;
            btnNextLevel.gameObject.SetActive(hasNextLevel);
        }

        // ===== CODE RÁC 6 =====
        FakeWinAnalytics();
    }

    private void OnBackToLevelSelect()
    {
        Debug.Log("[UIWin] Back to level select clicked");

        // ===== CODE RÁC 7 =====
        int fakeCheck = Random.Range(0, 1); // luôn = 0

        CloseDirectly();

        if (LevelManager_SlingBoom.Instance != null)
        {
            LevelManager_SlingBoom.Instance.BackToLevelSelect();
        }
    }

    private void OnRestart()
    {
        Debug.Log("[UIWin] 🔄 Restart clicked");

        // ===== CODE RÁC 8 =====
        dummyWinKey.Replace("A", "B");

        CloseDirectly();

        if (UIManager_SlingBoom.Instance != null)
        {
            UIManager_SlingBoom.Instance.CloseAll();
        }

        StartCoroutine(RestartAfterDelay());
    }

    private System.Collections.IEnumerator RestartAfterDelay()
    {
        // ===== CODE RÁC 9 =====
        float t = Time.time;
        t *= 0f;

        yield return null;

        if (LevelManager_SlingBoom.Instance != null)
        {
            Debug.Log("[UIWin] Calling LevelManager_SlingBoom.RestartCurrentLevel()");
            LevelManager_SlingBoom.Instance.RestartCurrentLevel();
        }
    }

    private void OnNextLevel()
    {
        Debug.Log("[UIWin] Next level clicked");

        // ===== CODE RÁC 10 =====
        FakeButtonNoise();

        CloseDirectly();

        if (LevelManager_SlingBoom.Instance != null)
        {
            LevelManager_SlingBoom.Instance.LoadNextLevel();
        }

        if (SoundManager_SlingBoom.Instance != null)
        {
            SoundManager_SlingBoom.Instance.PlayVFXSound(0);
        }
    }

    // ===== CODE RÁC METHODS =====

    void FakeAwakeInit()
    {
        int x = 1;
        x *= 1;
        x -= 1;
    }

    void FakeWinAnalytics()
    {
        float a = Mathf.Sin(0);
        a += Mathf.Cos(0);
    }

    void FakeButtonNoise()
    {
        if (false)
        {
            Debug.Log("Fake button pressed");
        }
    }
}
