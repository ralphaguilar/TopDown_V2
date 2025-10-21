using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;              // TextMeshPro support
using UnityEngine.UI;    // (optional) legacy Text fallback

public class RoundManager : MonoBehaviour
{
    [Header("Spawners")]
    [Tooltip("Add one or more EnemySpawner objects from your scene.")]
    public List<EnemySpawner> spawners = new List<EnemySpawner>();

    [Header("Round Settings")]
    [Tooltip("Round to start on (1 = first round).")]
    public int startRound = 1;

    [Tooltip("How many normal enemies in round 1.")]
    public int baseEnemies = 5;

    [Tooltip("Add this many normal enemies each round.")]
    public int enemiesPerRound = 3;

    [Tooltip("Intermission seconds between rounds.")]
    public float intermissionTime = 5f;

    [Header("Difficulty Scaling (per round)")]
    [Tooltip("Enemy maxHealth increases by this fraction each round. 0.20 = +20%/round.")]
    [Range(0f, 1f)] public float healthScalePerRound = 0.20f;

    [Tooltip("Enemy moveSpeed increases by this fraction each round. 0.05 = +5%/round.")]
    [Range(0f, 0.5f)] public float speedScalePerRound  = 0.05f;

    [Tooltip("Spawner interval multiplier per round. 0.90 means 10% faster each round.")]
    [Range(0.5f, 1.0f)] public float spawnIntervalScale = 0.90f;

    [Header("Boss Rounds")]
    [Tooltip("Spawn a boss round on every Nth round (0 = disabled).")]
    public int bossEveryN = 10;

    [Tooltip("Boss prefab to spawn on boss rounds.")]
    public GameObject bossPrefab;

    [Tooltip("How many bosses to spawn on boss rounds.")]
    public int bossCount = 1;

    [Tooltip("Multiply round-scaled health for bosses.")]
    public float bossHealthMultiplier = 3f;

    [Tooltip("Multiply round-scaled speed for bosses.")]
    public float bossSpeedMultiplier  = 1.2f;

    [Header("UI (assign any you use)")]
    public TMP_Text roundTextTMP;    // TextMeshPro (preferred)
    public TMP_Text timerTextTMP;    // TextMeshPro (optional)
    public Text roundTextLegacy;     // Legacy Text (fallback)
    public Text timerTextLegacy;     // Legacy Text (fallback)

    public int CurrentRound { get; private set; }
    public bool InIntermission { get; private set; }

    // --- internal state ---
    int _alive;                      // enemies alive in current round
    bool _running;
    float _lastIntervalMultiplier = 1f;

    void Start()
    {
        if (spawners == null || spawners.Count == 0)
            Debug.LogWarning("[RoundManager] No spawners assigned.");

        // Cache/normalize spawner intervals
        foreach (var s in spawners)
            s.CacheBaseInterval();

        CurrentRound = Mathf.Max(1, startRound);
        UpdateUI();
        if (!_running) StartCoroutine(RunLoop());
    }

    IEnumerator RunLoop()
    {
        _running = true;

        while (true)
        {
            // Intermission (skip before the very first round if you want)
            if (CurrentRound > startRound && intermissionTime > 0f)
                yield return StartCoroutine(Intermission(intermissionTime));
            else
                UpdateUI();

            // Compute round params
            int totalNormal = baseEnemies + enemiesPerRound * (CurrentRound - 1);
            bool isBossRound = (bossEveryN > 0 && bossPrefab != null && (CurrentRound % bossEveryN == 0));
            int bosses = isBossRound ? Mathf.Max(1, bossCount) : 0;

            float healthMult = 1f + healthScalePerRound * (CurrentRound - 1);
            float speedMult  = 1f + speedScalePerRound  * (CurrentRound - 1);
            float intervalMult = Mathf.Pow(spawnIntervalScale, (CurrentRound - 1));
            _lastIntervalMultiplier = intervalMult;

            foreach (var s in spawners)
                s.SetRoundIntervalMultiplier(intervalMult);

            // Spawn round
            _alive = 0;
            int remaining = Mathf.Max(0, totalNormal);
            int idx = 0;

            // Stagger normal enemies
            while (remaining > 0 && spawners.Count > 0)
            {
                var s = spawners[idx % spawners.Count];
                SpawnAndTrack(s, healthMult, speedMult, null);
                remaining--;
                idx++;
                yield return new WaitForSeconds(s.CurrentInterval);
            }

            // Spawn bosses (after or interleave if desired)
            for (int i = 0; i < bosses && spawners.Count > 0; i++)
            {
                var s = spawners[i % spawners.Count];
                SpawnAndTrack(s, healthMult * bossHealthMultiplier, speedMult * bossSpeedMultiplier, bossPrefab);
                yield return new WaitForSeconds(s.CurrentInterval);
            }

            UpdateUI();

            // Wait until all enemies are dead
            while (_alive > 0)
                yield return null;

            // Next round
            CurrentRound++;
        }
    }

    IEnumerator Intermission(float seconds)
    {
        InIntermission = true;
        float t = Mathf.Max(0f, seconds);
        while (t > 0f)
        {
            UpdateUI(t);
            t -= Time.deltaTime;
            yield return null;
        }
        InIntermission = false;
        UpdateUI();
    }

    void SpawnAndTrack(EnemySpawner spawner, float healthMult, float speedMult, GameObject overridePrefab)
    {
        if (spawner == null) return;

        var go = spawner.SpawnOneImmediate(overridePrefab);
        if (!go) return;

        _alive++;

        // Apply per-round scaling
        var init = go.GetComponent<EnemyInitializer>();
        if (!init) init = go.AddComponent<EnemyInitializer>();
        init.Apply(healthMult, speedMult);

        // Subscribe to death
        var hp = go.GetComponent<EnemyHealth>();
        if (hp != null)
        {
            // make sure EnemyHealth has: public System.Action onDied;
            hp.onDied += () => { _alive = Mathf.Max(0, _alive - 1); UpdateUI(); };
        }
        else
        {
            // Fallback: if no EnemyHealth, auto-decrement on destroy
            var tracker = go.AddComponent<RoundDeathTracker>();
            tracker.onDestroyed += () => { _alive = Mathf.Max(0, _alive - 1); UpdateUI(); };
        }
    }

    // ---- UI helpers ----
    void UpdateUI(float intermissionRemaining = -1f)
    {
        string roundStr = $"Round {CurrentRound}";
        string timerStr = (intermissionRemaining >= 0f) ? $"Next round in {intermissionRemaining:0.0}s" : "";

        if (roundTextTMP)   roundTextTMP.text = roundStr;
        if (timerTextTMP)   timerTextTMP.text = timerStr;
        if (roundTextLegacy) roundTextLegacy.text = roundStr;
        if (timerTextLegacy) timerTextLegacy.text = timerStr;
    }

    // Expose for other systems if they need to decrement alive counter
    public void NotifyEnemyDied()
    {
        _alive = Mathf.Max(0, _alive - 1);
        UpdateUI();
    }
}

/// <summary>
/// Fallback tracker if an enemy lacks EnemyHealth.onDied.
/// Decrements alive count when destroyed.
/// </summary>
public class RoundDeathTracker : MonoBehaviour
{
    public System.Action onDestroyed;
    void OnDestroy() => onDestroyed?.Invoke();
}