using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponHUD : MonoBehaviour
{
    [Header("Refs")]
    public PlayerShooting player;   
    public TMP_Text weaponText;    
    public TMP_Text ammoText;     
    public Image weaponIcon;       

    void Awake()
    {
        if (!player) player = FindFirstObjectByType<PlayerShooting>();
    }

    void OnEnable()
    {
        if (!player) return;
        player.OnWeaponChanged += HandleWeaponChanged;
        player.OnAmmoChanged   += HandleAmmoChanged;

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
    }

    void HandleAmmoChanged(int mag, int reserve)
    {
        if (!ammoText) return;

        if (player.CurrentWeapon == PlayerShooting.WeaponType.Knife)
            ammoText.text = "—";
        else
            ammoText.text = $"{mag} / {reserve}";
    }
}