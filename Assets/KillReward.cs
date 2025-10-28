using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
public class KillReward : MonoBehaviour
{
    public int reward = 10;
    public GameObject floatingTextPrefab; // assign in Inspector

    PlayerWallet wallet;
    EnemyHealth hp;
    Transform cam; // for screen-space conversion if needed

    void Awake()
    {
        wallet = FindFirstObjectByType<PlayerWallet>();
        cam = Camera.main.transform;
        hp = GetComponent<EnemyHealth>();
        hp.onDied += HandleDeath;
    }

    void HandleDeath()
    {
        // 1) Add money
        if (wallet != null) wallet.Add(reward);

        // 2) Spawn floating text
        if (floatingTextPrefab)
        {
            // Convert world pos to UI screen pos
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);

            var ft = Instantiate(floatingTextPrefab, screenPos, Quaternion.identity, 
                                 FindFirstObjectByType<Canvas>().transform);

            var floating = ft.GetComponent<FloatingText>();
            floating.SetText($"+{reward}");
        }
    }

    void OnDestroy()
    {
        if (hp != null) hp.onDied -= HandleDeath;
    }
}
