using UnityEngine;

public interface IDamageable
{
    void TakeDamage(float amount, UnityEngine.Vector2 hitPoint, UnityEngine.Vector2 knockback);
}
