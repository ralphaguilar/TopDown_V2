using UnityEngine;

public class EnemySpawnerTimed : MonoBehaviour
{
    [Header("Clock / Prefabs")]
    public NightClock clock;             // drag your NightClock in Inspector
    public GameObject enemyPrefab;
    public Transform player;

    [Header("Area")]
    public float spawnRadius = 10f;
    public float minDistanceFromPlayer = 6f;

    [Header("Cadence")]
    [Tooltip("Base seconds between spawns (multiplied by the Spawn Interval Curve).")]
    public float baseInterval = 1.0f;

    [Tooltip("Never let the interval go below this (even late at night).")]
    public float minIntervalClamp = 0.6f;

    [Tooltip("Max alive this spawner will try to maintain.")]
    public int simultaneousCap = 30;

    [Header("Staggering")]
    [Tooltip("Randomize the very first spawn time so multiple spawners don't sync.")]
    public Vector2 firstSpawnOffsetMultiplier = new Vector2(0.33f, 1.0f); // e.g., 0.33x..1.0x of current interval

    [Header("Global Cap (recommended if you have multiple spawners)")]
    public bool useGlobalCap = true;
    public int globalAliveCap = 26;

    [Header("Debug")]
    public bool logSpawns;

    // ---- internal ----
    float _timer;
    int _aliveApprox;

    // simple global counter shared by all spawners in play mode
    static int s_globalAlive;
    static bool s_globalInitDone;

    void Awake()
    {
        if (!player)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }

        // randomize the very first tick so 3 spawners don't fire together
        float startMult = Random.Range(firstSpawnOffsetMultiplier.x, firstSpawnOffsetMultiplier.y);
        float mult = clock ? clock.GetSpawnIntervalMultiplier() : 1f;
        float startInterval = Mathf.Max(minIntervalClamp, baseInterval * mult);
        _timer = startInterval * startMult;
    }

    void OnEnable()
    {
        _aliveApprox = 0;

        // initialize global counter once per play session
        if (Application.isPlaying && !s_globalInitDone)
        {
            s_globalAlive = 0;
            s_globalInitDone = true;
        }
    }

    void Update()
    {
        if (!clock || !clock.NightRunning) return;
        if (!enemyPrefab) return;

        // Evaluate current interval from curve and clamp the minimum
        float mult = clock.GetSpawnIntervalMultiplier(); // e.g., 1.0 at 10PM → 0.60 near 6AM (your curve)
        float currentInterval = Mathf.Max(minIntervalClamp, baseInterval * mult);

        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            // respect per-spawner cap
            if (_aliveApprox < simultaneousCap)
            {
                // respect global cap across all spawners (recommended)
                if (!useGlobalCap || s_globalAlive < globalAliveCap)
                {
                    var go = SpawnOne();
                    if (go)
                    {
                        _aliveApprox++;
                        s_globalAlive++;
                        if (logSpawns)
                            Debug.Log($"[Spawner {name}] Spawned. Local={_aliveApprox} Global={s_globalAlive}");
                    }
                }
            }

            // schedule next attempt
            _timer = currentInterval;
        }
    }

    GameObject SpawnOne()
    {
        Vector2 basePos = transform.position;

        for (int tries = 0; tries < 20; tries++)
        {
            // pick a ring around the spawner (avoid clustering right on it)
            float r = Random.Range(spawnRadius * 0.5f, spawnRadius);
            Vector2 pos = basePos + Random.insideUnitCircle.normalized * r;

            if (player && Vector2.Distance(pos, player.position) < minDistanceFromPlayer)
                continue;

            var go = Instantiate(enemyPrefab, pos, Quaternion.identity);
            go.layer = LayerMask.NameToLayer("Enemy");

            // Apply difficulty scaling right now
            var init = go.GetComponent<EnemyInitializer>();
            if (!init) init = go.AddComponent<EnemyInitializer>();
            init.Apply(clock.GetHealthMultiplier(), clock.GetSpeedMultiplier());

            // Track alive counts
            var hp = go.GetComponent<EnemyHealth>();
            if (hp != null)
            {
                hp.onDied += () =>
                {
                    _aliveApprox = Mathf.Max(0, _aliveApprox - 1);
                    s_globalAlive = Mathf.Max(0, s_globalAlive - 1);
                };
            }
            else
            {
                // fallback tracker if no EnemyHealth exists
                var tracker = go.AddComponent<AliveTracker>();
                tracker.onDestroyed += () =>
                {
                    _aliveApprox = Mathf.Max(0, _aliveApprox - 1);
                    s_globalAlive = Mathf.Max(0, s_globalAlive - 1);
                };
            }

            return go;
        }

        return null;
    }

    // Fallback tracker if an enemy lacks EnemyHealth
    private class AliveTracker : MonoBehaviour
    {
        public System.Action onDestroyed;
        void OnDestroy() => onDestroyed?.Invoke();
    }
}