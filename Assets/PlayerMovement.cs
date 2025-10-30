using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private bool normalizeDiagonal = true;

    private Rigidbody2D rb;
    private Animator animator;

    // current input vector from Input System callback
    private Vector2 moveInput;

    // cache if we’re moving (for animator)
    private bool isMoving;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // 2D top-down defaults (ensure no gravity influence)
        rb.gravityScale = 0f;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void Update()
    {
        // Animator updates should be in Update (frame-rate), not FixedUpdate
        Vector2 animVec = moveInput;

        if (normalizeDiagonal && animVec.sqrMagnitude > 1f)
            animVec = animVec.normalized;

        isMoving = animVec.sqrMagnitude > 0.0001f;

        if (animator)
        {
            animator.SetBool("isWalking", isMoving);
            animator.SetFloat("InputX", animVec.x);
            animator.SetFloat("InputY", animVec.y);
        }
    }

    void FixedUpdate()
    {
        // Physics velocity set in FixedUpdate
        Vector2 v = moveInput;
        if (normalizeDiagonal && v.sqrMagnitude > 1f)
            v = v.normalized;

        rb.linearVelocity = v * moveSpeed;
    }

    // Input System callback (PlayerInput will call this if your action is named "Move")
    // Action must be Value(Vector2) with a 2D Vector composite
    public void Move(InputAction.CallbackContext ctx)
    {
        // Read the current value on started/performed/canceled — it changes continuously
        moveInput = ctx.ReadValue<Vector2>();

        // When movement stops, store the last facing dir (once) for idle pose
        if (ctx.canceled && animator)
        {
            animator.SetBool("isWalking", false);

            // Only update last facing if we actually had a non-zero input recently
            // (prevents overwriting with (0,0))
            if (moveInput.sqrMagnitude > 0.0001f)
            {
                animator.SetFloat("LastInputX", moveInput.x);
                animator.SetFloat("LastInputY", moveInput.y);
            }
            else
            {
                // If canceled gave us zero, reuse current displayed InputX/Y
                float ix = animator.GetFloat("InputX");
                float iy = animator.GetFloat("InputY");
                animator.SetFloat("LastInputX", ix);
                animator.SetFloat("LastInputY", iy);
            }
        }
    }
}