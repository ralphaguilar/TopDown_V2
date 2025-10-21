using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform player;
    public float spawnRadius = 10f;
    public float minDistanceFromPlayer = 6f;

    [Header("Cadence")]
    public float baseInterval = 0.6f; // time between staggered spawns
    public float CurrentInterval { get; private set; }

    void Awake()
    {
        if (!player)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }
        CurrentInterval = baseInterval;
    }

    public void CacheBaseInterval() => CurrentInterval = baseInterval;

    public void SetRoundIntervalMultiplier(float m)
    {
        CurrentInterval = Mathf.Max(0.05f, baseInterval * m);
    }

    public GameObject SpawnOneImmediate(GameObject overridePrefab = null)
    {
        GameObject prefab = overridePrefab ? overridePrefab : enemyPrefab;
        if (!prefab) return null;

        Vector2 basePos = transform.position;
        for (int tries = 0; tries < 20; tries++)
        {
            Vector2 offset = Random.insideUnitCircle.normalized * Random.Range(spawnRadius * 0.5f, spawnRadius);
            Vector2 pos = basePos + offset;

            if (player && Vector2.Distance(pos, player.position) < minDistanceFromPlayer) continue;

            var go = Instantiate(prefab, pos, Quaternion.identity);
            go.layer = LayerMask.NameToLayer("Enemy");
            return go;
        }
        return null;
    }
}
