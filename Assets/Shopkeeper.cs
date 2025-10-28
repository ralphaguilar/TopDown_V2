using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Shopkeeper : MonoBehaviour, IInteractable
{
    public ShopUI shop;                 // drag the ShopUI object here
    [TextArea] public string prompt = "Press E to open shop";

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;           // make it a trigger zone
    }

    public void Interact()
    {
        if (shop == null) return;
        if (!shop.IsOpen) shop.Open(); else shop.Close();
    }

    public string GetPrompt() => prompt;
}
