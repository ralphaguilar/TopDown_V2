using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
public class KillReward : MonoBehaviour
{
    public int reward = 10;
    public GameObject floatingTextPrefab; 

    PlayerWallet wallet;
    EnemyHealth hp;
    Transform cam; 

    void Awake()
    {
        wallet = FindFirstObjectByType<PlayerWallet>();
        cam = Camera.main.transform;
        hp = GetComponent<EnemyHealth>();
        hp.onDied += HandleDeath;
    }

    void HandleDeath()
    {
        //Add money
        if (wallet != null) wallet.Add(reward);

        // Spawn floating text
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
