using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class EnemyChase : MonoBehaviour
{
    [Header("Chase")]
    public float moveSpeed = 3f;
    public float aggroRange = 8f;
    public float stopDistance = 0.8f;
    [Tooltip("Lose aggro when player is farther than this (must be >= aggroRange).")]
    public float deaggroRange = 11f;

    [Header("Smoothing")]
    [Tooltip("How fast velocity changes toward the target speed.")]
    public float acceleration = 20f;

    [Header("Hit Reaction")]
    [Tooltip("Pause AI briefly after taking knockback so the push is visible.")]
    public float knockbackPause = 0.15f;

    Transform player;
    Rigidbody2D rb;
    Animator anim;

    // Animator parameter names 
    const string P_MOVE_X = "MoveX";
    const string P_MOVE_Y = "MoveY";
    const string P_SPEED  = "Speed";

    // Internal
    bool hasAggro = false;
    float stunTimer = 0f;
    Vector2 lastNonZeroDir = Vector2.down; // default facing

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        var p = GameObject.FindGameObjectWithTag("Player");
        if (p) player = p.transform;

        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        // Ensure deaggro
        if (deaggroRange < aggroRange) deaggroRange = aggroRange;
    }

    void FixedUpdate()
    {
        if (!player) { UpdateAnimator(rb.linearVelocity); return; }

        // Count down knockback
        if (stunTimer > 0f)
        {
            stunTimer -= Time.fixedDeltaTime;
            UpdateAnimator(rb.linearVelocity);
            return; 
        }

        Vector2 pos = rb.position;
        Vector2 toPlayer = (Vector2)player.position - pos;
        float dist = toPlayer.magnitude;

        // If in distance, aggro
        if (!hasAggro && dist <= aggroRange) hasAggro = true;
        if (hasAggro && dist >= deaggroRange) hasAggro = false;

        // Decide desired velocity
        Vector2 desiredVel = Vector2.zero;
        if (hasAggro)
        {
            if (dist > stopDistance)
            {
                Vector2 dir = (dist > 0.0001f) ? (toPlayer / dist) : Vector2.zero;
                desiredVel = dir * moveSpeed;
            }
            else
            {
                desiredVel = Vector2.zero; // if in range, stop
            }
        }

        // Smooth toward desired velocity
        Vector2 newVel = Vector2.MoveTowards(rb.linearVelocity, desiredVel, acceleration * Time.fixedDeltaTime);
        rb.linearVelocity = newVel;

        // Drive Animator params
        UpdateAnimator(newVel);
    }

    void UpdateAnimator(Vector2 currentVelocity)
    {
        float speed = currentVelocity.magnitude;

        if (speed > 0.01f)
        {
            Vector2 dir = currentVelocity / speed;
            lastNonZeroDir = dir;
            anim.SetFloat(P_MOVE_X, dir.x);
            anim.SetFloat(P_MOVE_Y, dir.y);
        }
        else
        {
            // Keep last facing when idle so Idle 4-dir picks right clip
            anim.SetFloat(P_MOVE_X, lastNonZeroDir.x);
            anim.SetFloat(P_MOVE_Y, lastNonZeroDir.y);
        }

        anim.SetFloat(P_SPEED, speed);
    }


    public void ApplyKnockback(float customPause = -1f)
    {
        float pause = (customPause > 0f) ? customPause : knockbackPause;
        stunTimer = Mathf.Max(stunTimer, pause);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aggroRange);
        Gizmos.color = new Color(1f, 0.5f, 0f, 1f);
        Gizmos.DrawWireSphere(transform.position, deaggroRange);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
    }
}