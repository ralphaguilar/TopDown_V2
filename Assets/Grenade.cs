using UnityEngine;

public class Grenade : MonoBehaviour
{
    public float fuseTime = 1.5f;
    public GameObject explosionPrefab;

    void OnEnable()
    {
        Invoke(nameof(Explode), fuseTime);
    }

    void Explode()
    {
        if (explosionPrefab)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }
}
