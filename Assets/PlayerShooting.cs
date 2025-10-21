using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    public enum WeaponType { Pistol, Shotgun, MachineGun, Knife }

    // -------- Events for HUD --------
    public System.Action<WeaponType> OnWeaponChanged;
    public System.Action<int, int>   OnAmmoChanged;   // (mag, reserve)

    [System.Serializable]
    public class AmmoState {
        public int mag;
        public int magSize;
        public int reserve;
    }

    // -------- References --------
    [Header("References")]
    public Transform firePoint;              // where projectiles spawn (child of player)
    public GameObject bulletPrefab;          // projectile prefab (must have RB2D + Collider2D; Bullet.cs recommended)
    public GameObject grenadePrefab;         // grenade prefab (with Grenade.cs)
    public SpriteRenderer bodySprite;        // optional: flip for left/right facing

    [Header("General")]
    public WeaponType currentWeapon = WeaponType.Pistol;
    public LayerMask damageLayers = ~0;      // set to Enemy (and walls if desired)
    public float bulletLifetime = 2f;        // fallback if Bullet.cs not used
    public float spriteAngleOffsetDeg = 0f;  // if your sprite faces up, try +90

    // -------- Pistol --------
    [Header("Pistol")]
    public float pistolDamage = 35f;
    public float pistolSpeed = 18f;
    public float pistolFireCooldown = 0.25f;

    // -------- Shotgun --------
    [Header("Shotgun")]
    public int   shotgunPellets = 6;
    public float shotgunPelletDamage = 12f;
    public float shotgunPelletSpeed = 16f;
    [Tooltip("Total spread angle in degrees (centered around aim).")]
    public float shotgunSpread = 18f;
    public float shotgunFireCooldown = 0.8f;

    // -------- Machine Gun --------
    [Header("Machine Gun")]
    public float mgDamage = 20f;
    public float mgSpeed = 20f;
    public float mgFireCooldown = 0.08f;     // smaller = faster
    public float mgBaseInaccuracy = 2.5f;    // degrees random spread per shot

    // -------- Knife (melee) --------
    [Header("Knife (Melee)")]
    public float knifeDamage = 50f;
    public float knifeRange = 1.2f;          // radius of hit
    public float knifeKnockback = 8f;
    public float knifeCooldown = 0.45f;
    public Vector2 knifeOffset = new Vector2(0.8f, 0f); // forward offset from player
    public bool knifeDebugGizmo = true;

    // -------- Grenade Drop --------
    [Header("Grenade (Drop)")]
    public float dropOffsetDown = 0.08f;     // small nudge below feet
    public float ownerCollisionIgnoreTime = 0.2f;

    // -------- Private state --------
    Camera mainCamera;
    Vector2 lastAimDir = Vector2.right;
    float fireTimer = 0f;

    Dictionary<WeaponType, AmmoState> _ammo;

    public WeaponType CurrentWeapon => currentWeapon;

    void Awake()
    {
        mainCamera = Camera.main;
        transform.rotation = Quaternion.identity;

        // Initialize ammo (tune to taste)
        _ammo = new Dictionary<WeaponType, AmmoState> {
            { WeaponType.Pistol,     new AmmoState { magSize = 12, mag = 12, reserve = 72 } },
            { WeaponType.Shotgun,    new AmmoState { magSize =  2, mag =  2, reserve = 24 } },
            { WeaponType.MachineGun, new AmmoState { magSize = 30, mag = 30, reserve = 180 } },
            // Knife has no ammo entry
        };

        // Notify HUD of initial state
        OnWeaponChanged?.Invoke(currentWeapon);
        var a0 = GetCurrentAmmo();
        OnAmmoChanged?.Invoke(a0.mag, a0.reserve);
    }

    void Update()
    {
        AimAtMouse();

        var kb = Keyboard.current;
        if (kb != null)
        {
            // Weapon switch
            if (kb.digit1Key.wasPressedThisFrame) SetWeapon(WeaponType.Pistol);
            if (kb.digit2Key.wasPressedThisFrame) SetWeapon(WeaponType.Shotgun);
            if (kb.digit3Key.wasPressedThisFrame) SetWeapon(WeaponType.MachineGun);
            if (kb.digit4Key.wasPressedThisFrame) SetWeapon(WeaponType.Knife);

            // Reload
            if (kb.rKey.wasPressedThisFrame) Reload();

            // Grenade drop
            if (kb.eKey.wasPressedThisFrame) DropGrenadeAtFeet();
        }

        fireTimer -= Time.deltaTime;

        var mouse = Mouse.current;
        if (mouse == null) return;

        // Machine gun fires while held; others fire on click
        bool wantsFire = (currentWeapon == WeaponType.MachineGun)
            ? mouse.leftButton.isPressed
            : mouse.leftButton.wasPressedThisFrame;

        if (wantsFire && fireTimer <= 0f)
        {
            switch (currentWeapon)
            {
                case WeaponType.Pistol:
                    FirePistol();
                    fireTimer = pistolFireCooldown;
                    break;

                case WeaponType.Shotgun:
                    FireShotgun();
                    fireTimer = shotgunFireCooldown;
                    break;

                case WeaponType.MachineGun:
                    FireMachineGun();
                    fireTimer = mgFireCooldown;
                    break;

                case WeaponType.Knife:
                    DoKnifeAttack();
                    fireTimer = knifeCooldown;
                    break;
            }
        }
    }

    // ------------------- Aiming -------------------
    void AimAtMouse()
    {
        if (!firePoint || mainCamera == null || Mouse.current == null) return;

        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouseWorld.z = 0f;

        Vector2 dir = (Vector2)mouseWorld - (Vector2)firePoint.position;
        if (dir.sqrMagnitude < 0.000001f) return;

        dir.Normalize();
        lastAimDir = dir;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        firePoint.rotation = Quaternion.Euler(0f, 0f, angle + spriteAngleOffsetDeg);
        transform.rotation = Quaternion.identity;

        if (bodySprite != null)
        {
            bool facingLeft = (angle > 90f || angle < -90f);
            bodySprite.flipX = facingLeft;
        }

        Debug.DrawRay(firePoint.position, (Vector3)dir * 1.0f, Color.cyan);
    }

    void SetWeapon(WeaponType w)
    {
        currentWeapon = w;
        fireTimer = 0f; // allow immediate fire after switching
        OnWeaponChanged?.Invoke(w);
        var a = GetCurrentAmmo();
        OnAmmoChanged?.Invoke(a.mag, a.reserve);
    }

    // ------------------- Ammo helpers -------------------
    public AmmoState GetCurrentAmmo()
    {
        if (_ammo != null && _ammo.TryGetValue(currentWeapon, out var a))
            return a;
        return new AmmoState { mag = 0, magSize = 0, reserve = 0 };
    }

    void Reload()
    {
        if (!_ammo.TryGetValue(currentWeapon, out var a)) return; // Knife or untracked

        if (a.mag >= a.magSize) return;
        if (a.reserve <= 0) return;

        int needed = a.magSize - a.mag;
        int take = Mathf.Min(needed, a.reserve);
        a.mag += take;
        a.reserve -= take;

        OnAmmoChanged?.Invoke(a.mag, a.reserve);
        // TODO: play reload SFX/animation
    }

    // ------------------- Weapons -------------------
    void FirePistol()
    {
        if (!_ammo.TryGetValue(WeaponType.Pistol, out var aP)) return;
        if (aP.mag <= 0) { Reload(); return; }
        aP.mag--; OnAmmoChanged?.Invoke(aP.mag, aP.reserve);

        FireSingle(pistolDamage, pistolSpeed);
    }

    void FireShotgun()
    {
        if (!_ammo.TryGetValue(WeaponType.Shotgun, out var aS)) return;
        if (aS.mag <= 0) { Reload(); return; }
        aS.mag--; OnAmmoChanged?.Invoke(aS.mag, aS.reserve);

        float half = shotgunSpread * 0.5f;
        for (int i = 0; i < shotgunPellets; i++)
        {
            float t = (shotgunPellets == 1) ? 0f : (i / Mathf.Max(1f, shotgunPellets - 1f));
            float offset = Mathf.Lerp(-half, half, t);
            Vector2 dir = Rotate(lastAimDir, offset);

            SpawnBullet(dir, shotgunPelletDamage, shotgunPelletSpeed, bulletLifetime * 0.8f);
        }
    }

    void FireMachineGun()
    {
        if (!_ammo.TryGetValue(WeaponType.MachineGun, out var aM)) return;
        if (aM.mag <= 0) { Reload(); return; }
        aM.mag--; OnAmmoChanged?.Invoke(aM.mag, aM.reserve);

        float offset = Random.Range(-mgBaseInaccuracy, mgBaseInaccuracy);
        Vector2 dir = Rotate(lastAimDir, offset);

        SpawnBullet(dir, mgDamage, mgSpeed, bulletLifetime);
    }

    void DoKnifeAttack()
    {
        // center the hit a bit forward
        Vector2 center = (Vector2)transform.position + (lastAimDir.normalized * knifeOffset.x) + new Vector2(0f, knifeOffset.y);
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, knifeRange, damageLayers);

        foreach (var h in hits)
        {
            var d = h.GetComponent<IDamageable>();
            if (d == null) d = h.GetComponentInParent<IDamageable>();
            if (d == null) d = h.GetComponentInChildren<IDamageable>();
            if (d == null) continue;

            Vector2 hitPoint = h.bounds.ClosestPoint(center);
            Vector2 dir = ((Vector2)h.transform.position - (Vector2)transform.position).normalized;
            Vector2 kb = dir * knifeKnockback;
            d.TakeDamage(knifeDamage, hitPoint, kb);
        }
        // TODO: play swing anim/SFX
    }

    // ------------------- Bullet helpers -------------------
    void FireSingle(float damage, float speed)
    {
        SpawnBullet(lastAimDir, damage, speed, bulletLifetime);
    }

    void SpawnBullet(Vector2 dir, float damage, float speed, float life)
    {
        if (!bulletPrefab || !firePoint) return;

        var go = Instantiate(bulletPrefab, firePoint.position, Quaternion.Euler(0, 0, AngleOf(dir)));

        var b = go.GetComponent<Bullet>();
        if (b != null)
        {
            b.damage = damage;
            b.lifetime = life;
            b.hitLayers = damageLayers;
            b.SetVelocity(dir * speed);
        }
        else
        {
            var rb = go.GetComponent<Rigidbody2D>();
            if (rb) { rb.gravityScale = 0f; rb.linearVelocity = dir * speed; }
            Destroy(go, life);
        }

        IgnoreSelfBriefly(go);
    }

    // ------------------- Grenade drop -------------------
    void DropGrenadeAtFeet()
    {
        if (!grenadePrefab) { Debug.LogError("[PlayerShooting] grenadePrefab not assigned"); return; }

        Vector3 spawnPos = transform.position + Vector3.down * dropOffsetDown;
        var go = Instantiate(grenadePrefab, spawnPos, Quaternion.identity);

        var rb = go.GetComponent<Rigidbody2D>();
        if (rb) { rb.gravityScale = 0f; rb.linearVelocity = Vector2.zero; rb.angularVelocity = 0f; }

        var playerCol = GetComponent<Collider2D>();
        var grenadeCol = go.GetComponent<Collider2D>();
        if (playerCol && grenadeCol)
            StartCoroutine(TemporarilyIgnoreCollision(playerCol, grenadeCol, ownerCollisionIgnoreTime));
    }

    IEnumerator TemporarilyIgnoreCollision(Collider2D a, Collider2D b, float time)
    {
        Physics2D.IgnoreCollision(a, b, true);
        yield return new WaitForSeconds(time);
        if (a && b) Physics2D.IgnoreCollision(a, b, false);
    }

    void IgnoreSelfBriefly(GameObject proj)
    {
        var playerCol = GetComponent<Collider2D>();
        var projCol = proj ? proj.GetComponent<Collider2D>() : null;
        if (playerCol && projCol)
            StartCoroutine(TemporarilyIgnoreCollision(playerCol, projCol, 0.12f));
    }

    // ------------------- Utils & Gizmos -------------------
    static Vector2 Rotate(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float ca = Mathf.Cos(rad);
        float sa = Mathf.Sin(rad);
        return new Vector2(ca * v.x - sa * v.y, sa * v.x + ca * v.y);
    }

    static float AngleOf(Vector2 v) => Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;

    void OnDrawGizmosSelected()
    {
        if (knifeDebugGizmo)
        {
            Vector2 center = (Vector2)transform.position + (lastAimDir.normalized * knifeOffset.x) + new Vector2(0f, knifeOffset.y);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(center, knifeRange);
        }
        if (firePoint)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(firePoint.position, firePoint.position + firePoint.right * 0.75f);
        }
    }
}