using UnityEngine;

public class NpcAIController : BaseMove
{
    [Header("Cấu hình AI Tuần Tra")]
    [SerializeField] private float directionChangeTime = 3f; // Tăng lên 3s để NPC đi tự nhiên hơn
    [SerializeField] private float idleChance = 0.4f;        // 40% cơ hội NPC sẽ đứng lại nhún nhảy/nghỉ ngơi

    private float timer;
    private Vector2 lastFacingDirection = Vector2.down;   // Lưu hướng mặt cuối cùng để tránh lỗi về tâm (0,0)

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        // Khởi tạo thời gian đếm ngược ngay khi vào game
        timer = directionChangeTime;
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            ChooseNewAction();
            timer = directionChangeTime;
        }

        // 1. Cập nhật các trạng thái Animation cơ bản (isMoving, x, y) từ lớp cha BaseMove
        UpdateAnimation();

        // 2. Xử lý nâng cao: Ép Animator giữ nguyên hướng mặt cuối cùng khi dừng lại
        // Điều này giúp chấm đỏ trong Blend Tree "ide" hoặc "move" không bị nhảy về tâm (0,0)
        if (animator != null)
        {
            bool isMoving = moveInput != Vector2.zero;
            animator.SetBool("isMoving", isMoving);

            if (isMoving)
            {
                // Đang đi thì cập nhật hướng liên tục
                lastFacingDirection = moveInput;
            }
            else
            {
                // Khi dừng lại, ép Animator dùng lại hướng cũ để đứng yên nhún nhảy đúng hướng mặt
                animator.SetFloat("x", lastFacingDirection.x);
                animator.SetFloat("y", lastFacingDirection.y);
            }

            // Gửi thêm tham số speed phòng trường hợp bạn dùng nó để chuyển trạng thái
            float currentSpeedForAnim = isMoving ? moveSpeed : 0f;
            animator.SetFloat("speed", currentSpeedForAnim);
        }
    }

    // Hàm quản lý quyết định hành vi tiếp theo của NPC
    void ChooseNewAction()
    {
        // Tỷ lệ ngẫu nhiên xem NPC đi tiếp hay đứng lại nghỉ ngơi nhún nhảy
        if (Random.value < idleChance)
        {
            // Đứng im tại chỗ cử động (Nhún nhảy)
            moveInput = Vector2.zero;
        }
        else
        {
            // Chọn 1 trong 4 hướng di chuyển ngẫu nhiên
            moveInput = GetRandomDirection();
        }
    }

    Vector2 GetRandomDirection()
    {
        int dir = Random.Range(0, 4); // Chỉ lấy từ 0 đến 3 cho 4 hướng

        switch (dir)
        {
            case 0: return Vector2.up;
            case 1: return Vector2.down;
            case 2: return Vector2.left;
            case 3: return Vector2.right;
            default: return Vector2.down;
        }
    }
}