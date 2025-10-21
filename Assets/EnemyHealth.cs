
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class EnemyHealth : MonoBehaviour, IDamageable
{

    public float maxHealth = 50f;
    public float flashTime = 0.08f;

    float hp;
    SpriteRenderer sr;
    Color baseColor;

    void Awake()
    {
        hp = maxHealth;
        sr = GetComponent<SpriteRenderer>();
        if (sr) baseColor = sr.color;
    }

    public void TakeDamage(float amount, Vector2 hitPoint, Vector2 knockback)
    {
        hp -= amount;

        //knockback 
        var rb = GetComponent<Rigidbody2D>();
        if (rb) rb.AddForce(knockback, ForceMode2D.Impulse);

        //Hit flash
        if (sr) StartCoroutine(Flash());

        if (hp <= 0f) Die();
    }

    System.Collections.IEnumerator Flash()
    {
        sr.color = Color.white;
        yield return new WaitForSeconds(flashTime);
        sr.color = baseColor;
    }

    public System.Action onDied; // add near fields

    public void RefreshToMax() {
    // Call after scaling to refill HP
        hp = maxHealth;
    }

    void Die() {
        onDied?.Invoke();
        Destroy(gameObject);
    }
}
