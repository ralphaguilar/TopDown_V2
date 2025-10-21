
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damage = 25f;
    public float knockback = 4f;
    public float lifetime = 2f;
    public LayerMask hitLayers;

    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnEnable()
    {
        Invoke(nameof(Die), lifetime);
    }

    public void SetVelocity(Vector2 v)
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = v;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & hitLayers.value) == 0)
        {
            return;
        }

        var d = other.GetComponent<IDamageable>();
        if (d == null)
        {
            d = other.GetComponentInParent<IDamageable>();
            if (d == null) d = other.GetComponentInChildren<IDamageable>();
        }

        Vector2 hitPoint = other.bounds.ClosestPoint(transform.position);
        Vector2 dir = (other.transform.position - transform.position).normalized;
        Vector2 kb = dir * knockback;

        if (d != null)
        {
            d.TakeDamage(damage, hitPoint, kb);
            Die();
            return;
        }

        Die();
    }

    void Die()
    {
        CancelInvoke();
        Destroy(gameObject);
    }
}
