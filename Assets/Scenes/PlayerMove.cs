using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : BaseMove
{
    [Header("Run")]
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float runSpeed = 5f;

    private bool isRunning;

    private void Update()
    {
        // Kiểm tra trực tiếp từ phần cứng bàn phím đề phòng Input Action bị kẹt lệnh Key Up
        if (Keyboard.current != null)
        {
            isRunning = Keyboard.current.leftShiftKey.isPressed;
        }

        // Thay đổi tốc độ dựa trên trạng thái chạy
        moveSpeed = isRunning ? runSpeed : walkSpeed;

        // Gọi hàm xử lý hướng và animation từ lớp cha BaseMove
        UpdateAnimation();

        // Cập nhật giá trị tốc độ vào Animator để kích hoạt Blend Tree di chuyển/nhún nhảy nếu cần
        if (animator != null)
        {
            float targetSpeedForAnim = (moveInput != Vector2.zero) ? moveSpeed : 0f;
            animator.SetFloat("speed", targetSpeedForAnim);
        }
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();

        // Chặn input rác từ các cần analog cũ hoặc lỗi thiết bị
        if (moveInput.magnitude < 0.1f)
        {
            moveInput = Vector2.zero;
        }
    }

    public void OnRun(InputValue value)
    {
        isRunning = value.isPressed;
    }
}