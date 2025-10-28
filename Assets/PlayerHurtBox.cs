using UnityEngine;

public class PlayerHurtbox : MonoBehaviour
{
    public float touchDamage = 10f;
    public float touchKnockback = 6f;
    public LayerMask enemyLayers; // set to Enemy

    PlayerHealth player;
    Transform root;
    void Awake()
    {
        player = GetComponentInParent<PlayerHealth>();
        root = player ? player.transform : transform.root;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & enemyLayers) == 0) return;
        if (!player) return;

        Vector2 hitPoint = other.ClosestPoint(root.position);
        Vector2 dir = ((Vector2)root.position - (Vector2)other.transform.position).normalized;
        Vector2 kb = dir * touchKnockback;

        player.TakeDamage(touchDamage, hitPoint, kb);
    }
}

