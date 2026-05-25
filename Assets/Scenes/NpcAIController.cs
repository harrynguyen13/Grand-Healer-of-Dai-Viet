using UnityEngine;

public class NpcAIController : BaseMove
{
    [Header("Cấu hình AI Tự di chuyển")]
    [SerializeField] private float directionChangeTime = 3f;
    [SerializeField] private float idleChance = 0.2f;

    private float timer;
    private Vector2 lastFacingDirection = Vector2.down;

    private void Start()
    {
        timer = 0f;
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            ChooseNewAction();
            timer = directionChangeTime;
        }

        UpdateAnimation();
    }

    protected override void UpdateAnimation()
    {
        if (animator == null) return;

        bool isMoving = moveInput != Vector2.zero;

        animator.SetBool("isMoving", isMoving);

        if (isMoving)
        {
            lastFacingDirection = moveInput.normalized;
            animator.SetFloat("x", lastFacingDirection.x);
            animator.SetFloat("y", lastFacingDirection.y);
        }
        else
        {
            animator.SetFloat("x", lastFacingDirection.x);
            animator.SetFloat("y", lastFacingDirection.y);
        }

        animator.SetFloat("speed", isMoving ? moveSpeed : 0f);
    }

    private void ChooseNewAction()
    {
        if (Random.value < idleChance)
        {
            moveInput = Vector2.zero;
            return;
        }

        moveInput = GetRandomDirection();
    }

    private Vector2 GetRandomDirection()
    {
        int dir = Random.Range(0, 8);

        switch (dir)
        {
            case 0:
                return Vector2.up;

            case 1:
                return Vector2.down;

            case 2:
                return Vector2.left;

            case 3:
                return Vector2.right;

            case 4:
                return new Vector2(1, 1).normalized;

            case 5:
                return new Vector2(-1, 1).normalized;

            case 6:
                return new Vector2(1, -1).normalized;

            case 7:
                return new Vector2(-1, -1).normalized;

            default:
                return Vector2.down;
        }
    }
}