using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICanvas_SlingBoom : MonoBehaviour
{
    [SerializeField] bool isDestroyOnClose = false;
    private CanvasGroup canvasGroup;

    protected virtual void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public virtual void Setup()
    {

    }

    public virtual void Open()
    {
        gameObject.SetActive(true);
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public virtual void Close(float time)
    {
        Invoke(nameof(CloseDirectly), time);
    }

    public virtual void CloseDirectly()
    {
        if (isDestroyOnClose)
        {
            Destroy(gameObject);
        }
        else
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}