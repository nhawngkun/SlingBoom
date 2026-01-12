using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UILoss_SlingBoom : UICanvas_SlingBoom
{
    [Header("UI Elements")]
    [SerializeField] private Button btnBackToHome;
    [SerializeField] private Button btnRestart;
    
    [Header("Loss Display")]
    [SerializeField] private TextMeshProUGUI lossTitleText;
    
    protected override void Awake()
    {
        base.Awake();
    }
    
    public override void Setup()
    {
        base.Setup();
        
        if (btnBackToHome)
        {
            btnBackToHome.onClick.RemoveAllListeners();
            btnBackToHome.onClick.AddListener(OnBackToHome);
        }
        
        if (btnRestart)
        {
            btnRestart.onClick.RemoveAllListeners();
            btnRestart.onClick.AddListener(OnRestart);
        }
    }
    
    public override void Open()
    {
        base.Open();
        
        if (lossTitleText != null)
        {
            lossTitleText.text = "DEFEAT!";
        }
    }
    
    private void OnBackToHome()
    {
        Debug.Log("[UILoss] Back to home clicked");
        
        // ✅ ĐÓNG LOSS UI TRƯỚC
        CloseDirectly();
        
        if (LevelManager_SlingBoom.Instance != null)
        {
            LevelManager_SlingBoom.Instance.BackToHome();
        }
        
        if (SoundManager_SlingBoom.Instance != null)
        {
            SoundManager_SlingBoom.Instance.PlayVFXSound(1);
        }
    }
    
    private void OnRestart()
    {
        Debug.Log("[UILoss] 🔄 Restart clicked");
        
        // ✅ ĐÓNG LOSS UI TRƯỚC
        CloseDirectly();
        
        // ✅ ĐÓNG TẤT CẢ UI KHÁC
        if (UIManager_SlingBoom.Instance != null)
        {
            UIManager_SlingBoom.Instance.CloseAll();
        }
        
        // ✅ CHỜ 1 FRAME RỒI MỚI RESTART
        StartCoroutine(RestartAfterDelay());
        
       
    }
    
    private System.Collections.IEnumerator RestartAfterDelay()
    {
        yield return null;
        
        if (LevelManager_SlingBoom.Instance != null)
        {
            Debug.Log("[UILoss] Calling LevelManager.RestartCurrentLevel()");
            SoundManager_SlingBoom.Instance.PlayVFXSound(2);
            
            LevelManager_SlingBoom.Instance.RestartCurrentLevel();
        }
    }
}