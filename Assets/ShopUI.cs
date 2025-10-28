using UnityEngine;
using UnityEngine.EventSystems;

public class ShopUI : MonoBehaviour
{
    [Header("Assign this to your ShopPanel (the parent of the 6 buttons)")]
    public GameObject shopRoot;

    [Header("Optional")]
    public GameObject firstSelect;     
    public bool pauseOnOpen = false;
    public int canvasSortingOrder = 50; 

    Canvas shopCanvas;
    CanvasGroup cg;

    void Awake()
    {
        if (!shopRoot)
            Debug.LogError("[ShopUI] shopRoot is NOT assigned. Drag your ShopPanel here.", this);

        if (shopRoot)
        {
            cg = shopRoot.GetComponent<CanvasGroup>();
            if (!cg) cg = shopRoot.AddComponent<CanvasGroup>(); 

            shopCanvas = shopRoot.GetComponentInParent<Canvas>();
            if (shopCanvas == null)
                Debug.LogWarning("[ShopUI] No Canvas found above shopRoot. UI may not render.", this);
        }
    }

    public bool IsOpen { get; private set; }

    public void Open()
    {
        if (!shopRoot) { Debug.LogError("[ShopUI] Open() called but shopRoot is null", this); return; }

        
        if (shopCanvas != null)
        {
            shopCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            shopCanvas.sortingOrder = canvasSortingOrder;
            if (!shopCanvas.enabled) shopCanvas.enabled = true;
        }

        if (!shopRoot.activeSelf) shopRoot.SetActive(true);

        cg.alpha = 1f;
        cg.blocksRaycasts = true;
        cg.interactable = true;

        if (pauseOnOpen) Time.timeScale = 0f;

     
        if (firstSelect) EventSystem.current?.SetSelectedGameObject(firstSelect);

        IsOpen = true;
        Debug.Log("[ShopUI] Open: set active + alpha=1", this);
    }

    public void Close()
    {
        if (!shopRoot) return;

        cg.alpha = 0f;
        cg.blocksRaycasts = false;
        cg.interactable = false;

        shopRoot.SetActive(false);

        if (pauseOnOpen) Time.timeScale = 1f;
        IsOpen = false;
        Debug.Log("[ShopUI] Close: deactivated / alpha=0", this);
    }

    public void Toggle() { if (IsOpen) Close(); else Open(); }
}
