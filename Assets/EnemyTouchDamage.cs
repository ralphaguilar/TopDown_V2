using UnityEngine;

public class EnemyTouchDamage : MonoBehaviour
{
    public float touchDamage = 10f;
    public float knockbackForce = 6f;

    void OnCollisionEnter2D(Collision2D col)
    {
        var player = col.collider.GetComponentInParent<PlayerHealth>();
        if (player == null) return;

        Vector2 hitPoint = col.GetContact(0).point;
        Vector2 direction = ((Vector2)player.transform.position - (Vector2)transform.position).normalized;

        player.TakeDamage(touchDamage, hitPoint, direction * knockbackForce);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.GetComponentInParent<PlayerHealth>();
        if (player == null) return;

        Vector2 hitPoint = other.ClosestPoint(transform.position);
        Vector2 direction = ((Vector2)player.transform.position - (Vector2)transform.position).normalized;

        player.TakeDamage(touchDamage, hitPoint, direction * knockbackForce);
    }
}