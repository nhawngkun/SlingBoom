using UnityEngine;

public class uihowtoplay : UICanvas_SlingBoom
{
    public void back()
    {
        UIManager_SlingBoom.Instance.EnableHowToPlay(false);
        UIManager_SlingBoom.Instance.EnableHome(true);
        
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
