using UnityEngine;

public class EnemyInitializer : MonoBehaviour
{
    public void Apply(float healthMult, float speedMult)
    {
        var hp = GetComponent<EnemyHealth>();
        if (hp)
        {
            hp.maxHealth = Mathf.Round(hp.maxHealth * healthMult);
            hp.RefreshToMax(); 
        }

        var chase = GetComponent<EnemyChase>();
        if (chase)
            chase.moveSpeed *= speedMult;
    }
}