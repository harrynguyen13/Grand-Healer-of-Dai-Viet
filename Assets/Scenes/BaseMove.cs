using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BaseMove : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] protected float moveSpeed = 2f;
    [SerializeField] protected float acceleration = 35f;

    protected Rigidbody2D rb;
    protected Animator animator;

    protected Vector2 moveInput;
    protected Vector2 currentVelocity;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    protected virtual void FixedUpdate()
    {
        if (rb == null) return;

        Vector2 targetVelocity = moveInput.normalized * moveSpeed;

        float currentAcc = moveInput == Vector2.zero
            ? acceleration * 2f
            : acceleration;

        currentVelocity = Vector2.Lerp(
            currentVelocity,
            targetVelocity,
            Time.fixedDeltaTime * currentAcc
        );

        Vector2 nextPosition = rb.position + currentVelocity * Time.fixedDeltaTime;

        rb.MovePosition(nextPosition);
    }

    protected virtual void UpdateAnimation()
    {
        if (animator == null) return;

        bool isMoving = moveInput.sqrMagnitude > 0.01f;

        animator.SetBool("isMoving", isMoving);

        if (isMoving)
        {
            Vector2 dir = moveInput.normalized;

            animator.SetFloat("x", dir.x);
            animator.SetFloat("y", dir.y);
        }

        animator.SetFloat("speed", isMoving ? moveSpeed : 0f);
    }
}