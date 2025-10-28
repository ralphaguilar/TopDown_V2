using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerInteract : MonoBehaviour
{
    [Header("Detection")]
    [Tooltip("How far the player can interact.")]
    public float interactRadius = 1.6f;

    [Tooltip("Layers that contain interactables (put your Shopkeeper on one of these). Use Everything to test.")]
    public LayerMask interactableLayers = ~0; 

    [Header("Prompt UI")]
    [Tooltip("TMP text that shows e.g. 'Press E to open shop'.")]
    public TMP_Text promptText;

    [Tooltip("CanvasGroup on the same prompt to fade/show/hide.")]
    public CanvasGroup promptCanvas;

    [Tooltip("Fallback text if the interactable doesn’t provide one.")]
    public string defaultPrompt = "Press E to interact";

    // --- internals ---
    private IInteractable current;
    private float interactCooldown; 
    private const float CooldownTime = 0.15f;

    void Update()
    {
        
        if (interactCooldown > 0f)
            interactCooldown -= Time.unscaledDeltaTime;

        current = FindNearestInteractable();

        // updates UI
        if (current != null)
        {
            string t = current.GetPrompt();
            ShowPrompt(string.IsNullOrEmpty(t) ? defaultPrompt : t);
        }
        else
        {
            ShowPrompt(null);
        }
    }

    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        if (interactCooldown > 0f) return;
        interactCooldown = CooldownTime;

        if (current != null)
        {
            current.Interact();
        }
    }

    private IInteractable FindNearestInteractable()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactRadius, interactableLayers);
        if (hits == null || hits.Length == 0)
            return null;

        float best = float.MaxValue;
        IInteractable chosen = null;

        foreach (var col in hits)
        {
            if (!col) continue;
            var ia = col.GetComponent<IInteractable>() ?? col.GetComponentInParent<IInteractable>();
            if (ia == null) continue;

            float d2 = (col.bounds.ClosestPoint(transform.position) - transform.position).sqrMagnitude;
            if (d2 < best)
            {
                best = d2;
                chosen = ia;
            }
        }

        return chosen;
    }

    private void ShowPrompt(string text)
    {
        if (promptText)
            promptText.text = string.IsNullOrEmpty(text) ? "" : text;

        if (promptCanvas)
            promptCanvas.alpha = string.IsNullOrEmpty(text) ? 0f : 1f;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}