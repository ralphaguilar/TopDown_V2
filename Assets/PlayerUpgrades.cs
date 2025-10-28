using UnityEngine;

public class PlayerUpgrades : MonoBehaviour
{
    public static PlayerUpgrades Instance { get; private set; }

    [Header("Unlocked Weapons / Abilities")]
    public bool hasShotgun = false;
    public bool hasMachinegun = false;
    public bool hasDash = false;

    [Header("Stats / Consumables")]
    public int   grenadeStock = 0;          // adds to next night
    public int   maxHealthBonus = 0;        // adds to base max
    public float speedMultiplier = 1f;      // multiplies player move speed

    [Header("Optional: immediate heal on next load")]
    public int pendingHeal = 0;             // e.g., from health pack purchase

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // sane clamps
        speedMultiplier = Mathf.Max(0.5f, speedMultiplier);
        grenadeStock = Mathf.Max(0, grenadeStock);
        maxHealthBonus = Mathf.Max(0, maxHealthBonus);
    }

    // Helpers for the shop to call
    public bool GrantShotgun()     { if (hasShotgun) return false; hasShotgun = true; return true; }
    public bool GrantMachinegun()  { if (hasMachinegun) return false; hasMachinegun = true; return true; }
    public bool GrantDash()        { if (hasDash) return false; hasDash = true; return true; }

    public void AddGrenades(int amount)     { grenadeStock = Mathf.Max(0, grenadeStock + amount); }
    public void AddMaxHealthBonus(int amt)  { maxHealthBonus = Mathf.Max(0, maxHealthBonus + amt); }
    public void AddPendingHeal(int amt)     { pendingHeal = Mathf.Max(0, pendingHeal + amt); }
    public void MultiplySpeed(float mult, float maxCap = 1.6f)
    {
        speedMultiplier = Mathf.Min(maxCap, speedMultiplier * mult);
    }
}
