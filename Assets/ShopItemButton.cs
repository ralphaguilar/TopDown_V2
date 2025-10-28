using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class ShopItemButton : MonoBehaviour
{
    public enum ItemType { Shotgun, Machinegun, Dash, HealthPack, Grenades, EnergyDrink }

    [Header("Setup")]
    public ItemType itemType;
    public int price = 100;
    public int quantity = 1;        // HP bonus / grenade amount / heal
    public float speedMult = 1.10f; // energy drink multiplier

    [Header("UI (optional)")]
    public TMP_Text label;
    public TMP_Text priceText;
    public GameObject ownedBadge;

    Button btn;
    PlayerWallet wallet;
    PlayerUpgrades upg;

    void Awake()
    {
        btn = GetComponent<Button>();
        if (priceText) priceText.text = $"${price}";

        // Don't touch singletons yet; some scenes spawn them later this frame.
        // Defer to Start (or end of frame) to resolve and refresh.
    }

    void Start()
    {
        StartCoroutine(DeferredInit());
    }

    System.Collections.IEnumerator DeferredInit()
    {
        // wait a frame so Bootstrap/singletons can appear
        yield return null;
        TryResolve();
        SafeRefresh();
    }

    void OnEnable()
    {
        // try resolve again in case this object was re-enabled later
        TryResolve();
        SafeRefresh();
    }

    void TryResolve()
    {
        if (!wallet) wallet = PlayerWallet.Instance ?? FindFirstObjectByType<PlayerWallet>(UnityEngine.FindObjectsInactive.Include);

        if (!upg) upg = PlayerUpgrades.Instance ?? FindFirstObjectByType<PlayerUpgrades>(UnityEngine.FindObjectsInactive.Include);
    }


    public void OnClickBuy()
    {
        TryResolve();

        if (!wallet || !upg)
        {
            Debug.LogWarning("[ShopItemButton] Missing Wallet or Upgrades; cannot buy yet.", this);
            return;
        }

        if (IsOwnedOneTime()) return;

        if (!wallet.CanAfford(price))
        {
            // TODO: feedback (shake/flash)
            return;
        }

        if (wallet.Spend(price))
        {
            ApplyPurchase();
            SafeRefresh();
            // TODO: play SFX
        }
    }

    bool IsOwnedOneTime()
    {
        if (upg == null) return false; // null-safe
        return itemType switch
        {
            ItemType.Shotgun    => upg.hasShotgun,
            ItemType.Machinegun => upg.hasMachinegun,
            ItemType.Dash       => upg.hasDash,
            _                   => false
        };
    }

    void ApplyPurchase()
    {
        if (upg == null) return;

        switch (itemType)
        {
            case ItemType.Shotgun:    upg.GrantShotgun(); break;
            case ItemType.Machinegun: upg.GrantMachinegun(); break;
            case ItemType.Dash:       upg.GrantDash(); break;

            case ItemType.HealthPack:
                upg.AddMaxHealthBonus(quantity);
                upg.AddPendingHeal(quantity);
                break;

            case ItemType.Grenades:
                upg.AddGrenades(quantity);
                break;

            case ItemType.EnergyDrink:
                upg.MultiplySpeed(speedMult, 1.6f);
                break;
        }
    }

    void SafeRefresh()
    {
        bool owned = upg ? IsOwnedOneTime() : false;

        if (ownedBadge) ownedBadge.SetActive(owned);
        if (btn) btn.interactable = !owned;

        if (label)
        {
            string name = itemType.ToString();
            if (itemType == ItemType.HealthPack) name += $" (+{quantity} Max HP)";
            if (itemType == ItemType.Grenades)   name += $" (+{quantity})";
            if (itemType == ItemType.EnergyDrink)name += $" (x{speedMult:0.00})";
            label.text = name;
        }
    }
}
