using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarUI : MonoBehaviour
{
    [Header("References")]
    public PlayerHealth player;     // Drag your Player (with PlayerHealth)
    public Image fill;              // Drag the HP_Bar_Fill (Image Type = Filled, Horizontal, Origin Left)
    public TMP_Text hpText;         // Optional: drag a TMP text that sits over the bar

    [Header("Animation")]
    [Tooltip("How fast the bar moves toward the new value (units per second).")]
    public float lerpSpeed = 8f;    // 0 = instant

    [Header("Colors")]
    public Color fullColor = new Color(0.17f, 0.85f, 0.29f); // green
    public Color midColor  = new Color(0.95f, 0.80f, 0.17f); // yellow
    public Color lowColor  = new Color(0.90f, 0.20f, 0.20f); // red

    float target01 = 1f;   // target normalized health
    float current01 = 1f;  // displayed normalized health

    void Awake()
    {
        if (!player) player = FindFirstObjectByType<PlayerHealth>();
        if (!fill)
        {
            // Try to find child by name as a convenience
            var t = transform.Find("HP_Bar_Fill");
            if (t) fill = t.GetComponent<Image>();
        }
    }

    void OnEnable()
    {
        if (player)
        {
            player.onHealthChanged += OnHealthChanged;
            // initialize from current values
            OnHealthChanged(player.CurrentHealth, player.maxHealth);
        }
    }

    void OnDisable()
    {
        if (player) player.onHealthChanged -= OnHealthChanged;
    }

    void Update()
    {
        // Smoothly animate toward target
        if (lerpSpeed <= 0f) current01 = target01;
        else current01 = Mathf.MoveTowards(current01, target01, lerpSpeed * Time.unscaledDeltaTime);

        if (fill)
        {
            fill.fillAmount = current01;
            fill.color = EvaluateColor(current01);
        }
    }

    void OnHealthChanged(float hp, float max)
    {
        max = Mathf.Max(1f, max);
        target01 = Mathf.Clamp01(hp / max);

        if (hpText)
            hpText.text = $"{Mathf.CeilToInt(hp)}/{Mathf.CeilToInt(max)}";
    }

    Color EvaluateColor(float t)
    {
        // 0..0.5: red -> yellow, 0.5..1: yellow -> green
        if (t < 0.5f)
        {
            float k = t / 0.5f;
            return Color.Lerp(lowColor, midColor, k);
        }
        else
        {
            float k = (t - 0.5f) / 0.5f;
            return Color.Lerp(midColor, fullColor, k);
        }
    }

    // Optional helper to force-refresh from code (e.g., after loading)
    public void RefreshNow()
    {
        if (!player) return;
        OnHealthChanged(player.CurrentHealth, player.maxHealth);
        current01 = target01;
        if (fill) fill.fillAmount = current01;
    }
}