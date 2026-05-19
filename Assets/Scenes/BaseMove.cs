using UnityEngine;

public class BaseMove : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] protected float moveSpeed = 2f;
    [SerializeField] protected float acceleration = 35f;

    protected Rigidbody2D rb;
    protected Animator animator;
    protected Vector2 moveInput;
    protected Vector2 currentVelocity; // Biến phụ để xử lý Lerp gia tốc mượt mà

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    protected virtual void UpdateAnimation()
    {
        if (animator == null) return;

        bool hasInput = moveInput != Vector2.zero;
        animator.SetBool("isMoving", hasInput);

        if (hasInput)
        {
            animator.SetFloat("x", moveInput.x);
            animator.SetFloat("y", moveInput.y);
        }
    }

    protected virtual void FixedUpdate()
    {
        // KIỂM TRA AN TOÀN: Nếu không có Rigidbody2D thì bỏ qua, tránh NullReferenceException
        if (rb != null)
        {
            Vector2 targetVelocity = moveInput.normalized * moveSpeed;
            
            // Nếu buông phím di chuyển, tăng gấp đôi gia tốc để phanh khựng lại ngay lập tức
            float currentAcc = (moveInput == Vector2.zero) ? acceleration * 2f : acceleration;

            // Tính toán vận tốc mượt mà dựa trên gia tốc
            currentVelocity = Vector2.Lerp(currentVelocity, targetVelocity, Time.fixedDeltaTime * currentAcc);
            
            // Di chuyển vị trí bằng Rigidbody2D
            rb.MovePosition(rb.position + currentVelocity * Time.fixedDeltaTime);
        }
    }
}