using UnityEngine;

public class BaseMove : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] protected float moveSpeed = 2f;

    protected Rigidbody2D rb;
    protected Animator animator;
    protected Vector2 moveInput;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    protected virtual void UpdateAnimation()
    {
        if (animator == null) return;

        bool isMoving = moveInput != Vector2.zero;

        animator.SetBool("isMoving", isMoving);

        if (isMoving)
        {
            animator.SetFloat("x", moveInput.x);
            animator.SetFloat("y", moveInput.y);
        }

        animator.SetFloat("speed", isMoving ? moveSpeed : 0f);
    }

    protected virtual void FixedUpdate()
    {
        if (rb == null) return;

        Vector2 nextPosition =
            rb.position + moveInput.normalized * moveSpeed * Time.fixedDeltaTime;

        rb.MovePosition(nextPosition);
    }
}