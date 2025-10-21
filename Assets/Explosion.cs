
using UnityEngine;

public class Explosion : MonoBehaviour
{
    [Header("Damage")]
    public float radius = 3f;
    public float maxDamage = 80f;
    public float minDamage = 10f;
    public bool useFalloff = true;

    [Header("Physics")]
    public float knockbackStrength = 6f;
    public LayerMask hitLayers;
    public float lifeTime = 0.5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DoDamage();
        Destroy(gameObject, lifeTime);
    }

    // Update is called once per frame
    void DoDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, hitLayers);
        foreach (var h in hits)
        {
            Vector2 center = transform.position;
            Vector2 target = h.bounds.ClosestPoint(center);
            float dist = Vector2.Distance(center, target);
            float t = Mathf.Clamp01(dist / radius);

            float dmg = useFalloff ? Mathf.Lerp(maxDamage, minDamage, t) : maxDamage;
            Vector2 dir = ((Vector2)h.transform.position - center).normalized;
            Vector2 kb = dir * Mathf.Lerp(knockbackStrength, knockbackStrength * 0.3f, t);

            var d = h.GetComponent<IDamageable>();
            if (d != null) d.TakeDamage(dmg, target, kb);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
  }
}
