using UnityEngine;

public class NpcAIController : BaseMove
{
    [Header("Cấu hình AI Tự di chuyển")]
    [SerializeField] private float directionChangeTime = 3f;
    [SerializeField] private float idleChance = 0.2f;

    private float timer;
    private float forcedMoveTimer;

    private bool isBusy = false;
    private Vector2 lastFacingDirection = Vector2.down;

    public bool IsBusy => isBusy;

    private void Start()
    {
        timer = Random.Range(0f, directionChangeTime);
    }

    private void Update()
    {
        if (isBusy)
        {
            StopMoving();
            UpdateAnimation();
            return;
        }

        if (forcedMoveTimer > 0f)
        {
            forcedMoveTimer -= Time.deltaTime;

            if (forcedMoveTimer <= 0f)
            {
                StopMoving();
                timer = 0f;
            }

            UpdateAnimation();
            return;
        }

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
        if (animator == null)
            return;

        bool isMoving = moveInput.sqrMagnitude > 0.01f;

        animator.SetBool("isMoving", isMoving);

        if (isMoving)
        {
            lastFacingDirection = moveInput.normalized;
        }

        animator.SetFloat("x", lastFacingDirection.x);
        animator.SetFloat("y", lastFacingDirection.y);
        animator.SetFloat("speed", isMoving ? moveSpeed : 0f);
    }

    private void ChooseNewAction()
    {
        if (Random.value < idleChance)
        {
            StopMoving();
            return;
        }

        moveInput = GetRandomDirection();
    }

    private Vector2 GetRandomDirection()
    {
        int dir = Random.Range(0, 4);

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

            default:
                return Vector2.down;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isBusy)
            return;

        if (collision.gameObject.CompareTag("NPC"))
            return;

        BounceAway();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (isBusy)
            return;

        if (collision.gameObject.CompareTag("NPC"))
            return;

        BounceAway();
    }

    public void SetBusy(bool busy)
    {
        isBusy = busy;

        if (isBusy)
            StopMoving();
    }

    public void ForceStopMovement()
    {
        StopMoving();
    }

    public void FaceTarget(Vector3 targetPosition)
    {
        Vector2 dir = (targetPosition - transform.position).normalized;

        if (dir.sqrMagnitude <= 0.01f)
            return;

        lastFacingDirection = dir;

        if (animator != null)
        {
            animator.SetFloat("x", dir.x);
            animator.SetFloat("y", dir.y);
            animator.SetFloat("speed", 0f);
            animator.SetBool("isMoving", false);
        }
    }

    public void MoveAwayFrom(Vector3 targetPosition, float duration)
    {
        Vector2 awayDirection = transform.position - targetPosition;

        if (awayDirection.sqrMagnitude < 0.01f)
            awayDirection = GetRandomDirection();

        awayDirection.Normalize();

        moveInput = awayDirection;
        forcedMoveTimer = Mathf.Max(0.1f, duration);
        timer = forcedMoveTimer;
    }

    public void MoveDirectionForSeconds(Vector2 direction, float duration)
    {
        if (direction.sqrMagnitude < 0.01f)
            direction = GetRandomDirection();

        moveInput = direction.normalized;
        forcedMoveTimer = Mathf.Max(0.1f, duration);
        timer = forcedMoveTimer;
    }

    public void BounceAway()
    {
        StopMoving();
        moveInput = GetRandomDirection();
        timer = directionChangeTime;
    }

    private void StopMoving()
    {
        moveInput = Vector2.zero;

        if (rb2d != null)
        {
            rb2d.linearVelocity = Vector2.zero;
            rb2d.angularVelocity = 0f;
        }
    }
}