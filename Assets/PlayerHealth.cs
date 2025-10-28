using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Health")]
    public float maxHealth = 100f;
    [SerializeField] private float hp;
    public float CurrentHealth => hp;

    [Header("Invulnerability")]
    public float invulnTime = 0.4f;
    public int flashCount = 4;

    [Header("Knockback")]
    public float knockbackMultiplier = 1.0f;
    public float knockbackDrag = 10f;

    [Header("On Death Behavior")]
    [Tooltip("If true, shows Game Over screen via GameOverManager instead of reloading scene.")]
    public bool showGameOverScreen = true;

    [Tooltip("If true (and showGameOverScreen is false), reloads the scene on death.")]
    public bool reloadSceneOnDeath = false;

    [Tooltip("Optional: scene name to load if reloading. Leave blank to reload current.")]
    public string sceneToLoad = "";

    [Tooltip("Delay before reloading or showing Game Over screen (seconds).")]
    public float deathDelay = 0.5f;

    [Header("Flash Visual (Optional)")]
    public SpriteRenderer spriteToFlash;
    public Color flashColor = new Color(1, 1, 1, 0.35f);

    // events
    public event Action<float,float> onHealthChanged; // hp, max
    public event Action onDamaged;
    public event Action onDied;

    // private
    Rigidbody2D rb;
    bool invulnerable;
    Color originalColor;

    [Header("References")]
    public GameOverManager gameOverManager; 

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        if (!spriteToFlash) spriteToFlash = GetComponentInChildren<SpriteRenderer>();
        if (spriteToFlash) originalColor = spriteToFlash.color;

        hp = maxHealth;
        onHealthChanged?.Invoke(hp, maxHealth);

        if (!gameOverManager)
        {
            gameOverManager = FindFirstObjectByType<GameOverManager>(UnityEngine.FindObjectsInactive.Include);
        }
    }

    void Update()
    {
        // Debug damage key
        if (Keyboard.current != null && Keyboard.current.kKey.wasPressedThisFrame)
            TakeDamage(10f, transform.position, Vector2.zero);
    }

    void FixedUpdate()
    {
        // stops knockback drift
        rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, Vector2.zero, knockbackDrag * Time.fixedDeltaTime);
    }

    //Healing
    public void Heal(float amount)
    {
        if (hp <= 0) return;
        hp = Mathf.Min(maxHealth, hp + amount);
        onHealthChanged?.Invoke(hp, maxHealth);
    }

    // Taking Damage 
    public void TakeDamage(float amount, Vector2 hitPoint, Vector2 knockback)
    {
        if (hp <= 0 || invulnerable) return;

        hp = Mathf.Max(0f, hp - amount);
        onDamaged?.Invoke();
        onHealthChanged?.Invoke(hp, maxHealth);

        // Knockback
        if (knockback.sqrMagnitude > 0f)
            rb.AddForce(knockback * knockbackMultiplier, ForceMode2D.Impulse);

        // Invulnerability frames
        if (invulnTime > 0f)
            StartCoroutine(DoInvulnerability());

        // Death
        if (hp <= 0)
            Die();
    }

    // Invulnerability
    IEnumerator DoInvulnerability()
    {
        invulnerable = true;

        if (spriteToFlash && flashCount > 0)
        {
            for (int i = 0; i < flashCount; i++)
            {
                spriteToFlash.color = flashColor;
                yield return new WaitForSeconds(invulnTime / (flashCount * 2f));
                spriteToFlash.color = originalColor;
                yield return new WaitForSeconds(invulnTime / (flashCount * 2f));
            }
        }
        else
        {
            yield return new WaitForSeconds(invulnTime);
        }

        invulnerable = false;
    }

    // Death 
    void Die()
    {
        onDied?.Invoke();

        // Stop player motion
        rb.linearVelocity = Vector2.zero;

        if (showGameOverScreen)
        {
            StartCoroutine(ShowGameOverAfterDelay());
        }
        else if (reloadSceneOnDeath)
        {
            StartCoroutine(ReloadScene());
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    IEnumerator ShowGameOverAfterDelay()
    {
        
        if (deathDelay > 0f)
            yield return new WaitForSecondsRealtime(deathDelay);

        if (!gameOverManager)
        {
            gameOverManager = FindFirstObjectByType<GameOverManager>(UnityEngine.FindObjectsInactive.Include);
        }

        if (gameOverManager)
        {
            gameOverManager.ShowGameOver();
        }
        else
        {
            Debug.LogWarning("[PlayerHealth] No GameOverManager found; consider enabling reloadSceneOnDeath as fallback.");
            if (reloadSceneOnDeath)
                StartCoroutine(ReloadScene());
        }
    }

    IEnumerator ReloadScene()
    {
        if (deathDelay > 0f)
            yield return new WaitForSecondsRealtime(deathDelay);

        string target = string.IsNullOrEmpty(sceneToLoad) ? SceneManager.GetActiveScene().name : sceneToLoad;
        Time.timeScale = 1f;
        SceneManager.LoadScene(target);
    }
}