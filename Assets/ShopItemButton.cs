using System.Collections;
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
    public int quantity = 1;          // used for consumables (HP/Grenades)
    public float speedMult = 1.10f;   // used for EnergyDrink

    [Header("UI (optional)")]
    public TMP_Text label;
    public TMP_Text priceText;
    public GameObject ownedBadge;     // e.g., a checkmark or “Owned” ribbon

    private Button btn;
    private PlayerWallet wallet;
    private PlayerUpgrades upg;

    void Awake()
    {
        btn = GetComponent<Button>();
        if (priceText) priceText.text = $"${price}";

        // Button click -> buy
        btn.onClick.AddListener(OnClickBuy);
    }

    void Start()
    {
        // Allow singletons to spawn in other Awake/Start before resolving
        StartCoroutine(DeferredInit());
    }

    IEnumerator DeferredInit()
    {
        yield return null;
        TryResolve();
        Subscribe();
        SafeRefresh();
    }

    void OnEnable()
    {
        TryResolve();
        Subscribe();
        SafeRefresh();
    }

    void OnDisable()
    {
        Unsubscribe();
    }

    void OnDestroy()
    {
        if (btn != null) btn.onClick.RemoveListener(OnClickBuy);
        Unsubscribe();
    }

    private void Subscribe()
    {
        if (wallet != null)
            wallet.onMoneyChanged -= OnMoneyChanged; // avoid double-subscribe

        wallet = PlayerWallet.Instance ?? FindFirstObjectByType<PlayerWallet>(FindObjectsInactive.Include);
        if (wallet != null)
            wallet.onMoneyChanged += OnMoneyChanged;

        if (upg == null)
            upg = PlayerUpgrades.Instance ?? FindFirstObjectByType<PlayerUpgrades>(FindObjectsInactive.Include);
    }

    private void Unsubscribe()
    {
        if (wallet != null)
            wallet.onMoneyChanged -= OnMoneyChanged;
    }

    private void TryResolve()
    {
        if (!wallet)
            wallet = PlayerWallet.Instance ?? FindFirstObjectByType<PlayerWallet>(FindObjectsInactive.Include);

        if (!upg)
            upg = PlayerUpgrades.Instance ?? FindFirstObjectByType<PlayerUpgrades>(FindObjectsInactive.Include);
    }

    private void OnMoneyChanged(int _)
    {
        SafeRefresh();
    }

    public void OnClickBuy()
    {
        TryResolve();

        if (!wallet || !upg)
        {
            Debug.LogWarning("[ShopItemButton] Missing Wallet or Upgrades; cannot buy yet.", this);
            return;
        }

        // one-time unlocks already owned? do nothing
        if (IsOwnedOneTime()) return;

        if (!wallet.CanAfford(price))
        {
            // Optional: play “cant afford” SFX or flash price red here
            SafeRefresh();
            return;
        }

        if (wallet.Spend(price))
        {
            ApplyPurchase();
            SafeRefresh();
        }
    }

    private bool IsOwnedOneTime()
    {
        if (upg == null) return false;
        return itemType switch
        {
            ItemType.Shotgun    => upg.hasShotgun,
            ItemType.Machinegun => upg.hasMachinegun,
            ItemType.Dash       => upg.hasDash,
            _                   => false
        };
    }

    private void ApplyPurchase()
    {
        if (upg == null) return;

        switch (itemType)
        {
            case ItemType.Shotgun:
                upg.GrantShotgun();
                break;

            case ItemType.Machinegun:
                upg.GrantMachinegun();
                break;

            case ItemType.Dash:
                upg.GrantDash();
                break;

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

    private void SafeRefresh()
    {
        bool owned = upg ? IsOwnedOneTime() : false;

        if (ownedBadge) ownedBadge.SetActive(owned);

        if (btn)
        {
            bool canAfford = wallet ? wallet.CanAfford(price) : false;
            btn.interactable = !owned && canAfford;
        }

        if (label)
        {
            string name = itemType.ToString();
            if (itemType == ItemType.HealthPack)  name += $" (+{quantity} Max HP)";
            if (itemType == ItemType.Grenades)    name += $" (+{quantity})";
            if (itemType == ItemType.EnergyDrink) name += $" (x{speedMult:0.00})";
            label.text = name;
        }

        if (priceText)
            priceText.text = $"${price}";
    }
}