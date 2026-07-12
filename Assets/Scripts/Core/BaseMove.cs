using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BaseMove : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] protected float moveSpeed = 2f;

    protected Rigidbody2D rb2d;
    protected Animator animator;

    protected Vector2 moveInput;
    protected Vector2 lastDirection = Vector2.down;

    protected virtual void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        if (rb2d != null)
        {
            rb2d.gravityScale = 0f;
            rb2d.freezeRotation = true;
            rb2d.linearVelocity = Vector2.zero;
            rb2d.angularVelocity = 0f;

            rb2d.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            // Để None trước để test hết giật.
            rb2d.interpolation = RigidbodyInterpolation2D.None;
        }
    }

    protected virtual void FixedUpdate()
    {
        if (rb2d == null) return;

        if (moveInput.sqrMagnitude > 0.01f)
        {
            Vector2 direction = moveInput.normalized;
            rb2d.linearVelocity = direction * moveSpeed;
        }
        else
        {
            moveInput = Vector2.zero;
            rb2d.linearVelocity = Vector2.zero;
        }
    }


    public virtual void StopImmediately()
    {
        moveInput = Vector2.zero;

        if (rb2d != null)
        {
            rb2d.linearVelocity = Vector2.zero;
            rb2d.angularVelocity = 0f;
        }
    }

    protected virtual void UpdateAnimation()
    {
        if (animator == null) return;

        bool isMoving = moveInput.sqrMagnitude > 0.01f;

        animator.SetBool("isMoving", isMoving);

        if (isMoving)
        {
            Vector2 dir = moveInput.normalized;
            lastDirection = dir;

            animator.SetFloat("x", dir.x);
            animator.SetFloat("y", dir.y);
        }
        else
        {
            animator.SetFloat("x", lastDirection.x);
            animator.SetFloat("y", lastDirection.y);
        }

        animator.SetFloat("speed", isMoving ? moveSpeed : 0f);
    
    
    }
    

    protected virtual void OnDisable()
    {
        if (rb2d != null)
        {
            rb2d.linearVelocity = Vector2.zero;
            rb2d.angularVelocity = 0f;
        }
    }
}