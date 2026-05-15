using UnityEngine;
using UnityEngine.InputSystem;

public class Move : MonoBehaviour
{
    [Header("Cấu hình tốc độ")]
    [SerializeField] private float walkSpeed = 2f;    
    [SerializeField] private float runSpeed = 5f;     
    [SerializeField] private float acceleration = 35f; 

    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 moveInput;
    private Vector2 currentVelocity;
    private bool isRunning;

    void Awake()
    { 
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void OnMove(InputValue value) => moveInput = value.Get<Vector2>();

    // Khớp với Action "Run" trong hình image_2344e3.jpg
    void OnRun(InputValue value)
    {
        isRunning = value.isPressed;
    }

    void Update()
    {
        if (animator == null) return;

        // Dự phòng: Nếu Input Action bị kẹt, dòng này sẽ cứu bạn
        // Nó kiểm tra trực tiếp trạng thái phím Shift trái
        if (Keyboard.current != null)
        {
            isRunning = Keyboard.current.leftShiftKey.isPressed;
        }

        bool hasInput = moveInput != Vector2.zero;
        animator.SetBool("isMoving", hasInput);

        if (hasInput)
        {
            animator.SetFloat("x", moveInput.x);
            animator.SetFloat("y", moveInput.y);
            
            // Ép Animator cập nhật tốc độ ngay lập tức
            float targetSpeedForAnim = isRunning ? runSpeed : walkSpeed;
            animator.SetFloat("speed", targetSpeedForAnim);
        }
        else
        {
            animator.SetFloat("speed", 0);
        }
    }

    void FixedUpdate()
    {
        if (rb != null)
        {
            float targetSpeed = isRunning ? runSpeed : walkSpeed;
            Vector2 targetVelocity = moveInput.normalized * targetSpeed;
            
        
            float currentAcc = (moveInput == Vector2.zero) ? acceleration * 2f : acceleration;

            currentVelocity = Vector2.Lerp(currentVelocity, targetVelocity, Time.fixedDeltaTime * currentAcc);
            rb.MovePosition(rb.position + currentVelocity * Time.fixedDeltaTime);
        }
    }
}