using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponHUD : MonoBehaviour
{
    [Header("Refs")]
    public PlayerShooting player;   // drag your Player here
    public TMP_Text weaponText;     // drag HUD_Weapon
    public TMP_Text ammoText;       // drag HUD_Ammo
    public Image weaponIcon;        // optional

    void Awake()
    {
        if (!player) player = FindFirstObjectByType<PlayerShooting>();
    }

    void OnEnable()
    {
        if (!player) return;
        player.OnWeaponChanged += HandleWeaponChanged;
        player.OnAmmoChanged   += HandleAmmoChanged;

        // force an initial refresh
        HandleWeaponChanged(player.CurrentWeapon);
        var a = player.GetCurrentAmmo();
        HandleAmmoChanged(a.mag, a.reserve);
    }

    void OnDisable()
    {
        if (!player) return;
        player.OnWeaponChanged -= HandleWeaponChanged;
        player.OnAmmoChanged   -= HandleAmmoChanged;
    }

    void HandleWeaponChanged(PlayerShooting.WeaponType wt)
    {
        if (weaponText) weaponText.text = wt.ToString();
        // if you have icons: weaponIcon.sprite = player.GetIconFor(wt);
    }

    void HandleAmmoChanged(int mag, int reserve)
    {
        if (!ammoText) return;

        // Knife has no ammo; show a dash
        if (player.CurrentWeapon == PlayerShooting.WeaponType.Knife)
            ammoText.text = "—";
        else
            ammoText.text = $"{mag} / {reserve}";
    }
}