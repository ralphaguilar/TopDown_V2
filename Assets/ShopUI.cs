using UnityEngine;
using UnityEngine.EventSystems;

public class ShopUI : MonoBehaviour
{
    [Header("Assign this to your ShopPanel (the parent of the 6 buttons)")]
    public GameObject shopRoot;

    [Header("Optional")]
    public GameObject firstSelect;     // first button to highlight
    public bool pauseOnOpen = false;
    public int canvasSortingOrder = 50; // bump above other UIs

    Canvas shopCanvas;
    CanvasGroup cg;

    void Awake()
    {
        if (!shopRoot)
            Debug.LogError("[ShopUI] shopRoot is NOT assigned. Drag your ShopPanel here.", this);

        // Try to find Canvas/CanvasGroup automatically
        if (shopRoot)
        {
            cg = shopRoot.GetComponent<CanvasGroup>();
            if (!cg) cg = shopRoot.AddComponent<CanvasGroup>(); // ensure we can control alpha

            shopCanvas = shopRoot.GetComponentInParent<Canvas>();
            if (shopCanvas == null)
                Debug.LogWarning("[ShopUI] No Canvas found above shopRoot. UI may not render.", this);
        }
    }

    public bool IsOpen { get; private set; }

    public void Open()
    {
        if (!shopRoot) { Debug.LogError("[ShopUI] Open() called but shopRoot is null", this); return; }

        // Ensure Canvas renders on top
        if (shopCanvas != null)
        {
            shopCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            shopCanvas.sortingOrder = canvasSortingOrder;
            if (!shopCanvas.enabled) shopCanvas.enabled = true;
        }

        // Make sure nothing else is hiding it (e.g., FadeScreen)
        // Tip: temporarily disable other full-screen Images/CanvasGroups to confirm.

        // Activate GameObject and make it visible & interactive
        if (!shopRoot.activeSelf) shopRoot.SetActive(true);

        cg.alpha = 1f;
        cg.blocksRaycasts = true;
        cg.interactable = true;

        if (pauseOnOpen) Time.timeScale = 0f;

        // Select first button for keyboard/controller
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

        // You can keep the panel active with alpha 0, or deactivate it:
        shopRoot.SetActive(false);

        if (pauseOnOpen) Time.timeScale = 1f;
        IsOpen = false;
        Debug.Log("[ShopUI] Close: deactivated / alpha=0", this);
    }

    public void Toggle() { if (IsOpen) Close(); else Open(); }
}
