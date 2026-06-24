using UnityEngine;

public class NpcAIController : BaseMove
{
    [Header("Cấu hình AI Tự di chuyển")]
    [SerializeField] private float directionChangeTime = 3f;
    [SerializeField] private float idleChance = 0.2f;

    [Header("NPC Talk Settings")]
    [SerializeField] private float talkDuration = 3f;

    private float timer;
    private bool isBusy = false;
    private Vector2 lastFacingDirection = Vector2.down;

    private void Start()
    {
        timer = 0f;
    }

    private void Update()
    {
        if (isBusy)
        {
            StopMoving();
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
        if (animator == null) return;

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
            case 0: return Vector2.up;
            case 1: return Vector2.down;
            case 2: return Vector2.left;
            case 3: return Vector2.right;
            default: return Vector2.down;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isBusy) return;

        if (collision.gameObject.CompareTag("NPC"))
        {
            NpcAIController otherNpc = collision.gameObject.GetComponent<NpcAIController>();

            if (otherNpc != null && !otherNpc.isBusy)
            {
                StartConversation(otherNpc);
                return;
            }
        }

        StopMoving();
        moveInput = GetRandomDirection();
        timer = directionChangeTime;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (isBusy) return;

        StopMoving();
        moveInput = GetRandomDirection();
        timer = directionChangeTime;
    }

    private void StartConversation(NpcAIController otherNpc)
    {
        isBusy = true;
        otherNpc.isBusy = true;

        StopMoving();
        otherNpc.StopMoving();

        FaceTarget(otherNpc.transform.position);
        otherNpc.FaceTarget(transform.position);

        Invoke(nameof(EndConversation), talkDuration);
        otherNpc.Invoke(nameof(EndConversation), talkDuration);

        Debug.Log(gameObject.name + " đang nói chuyện với " + otherNpc.gameObject.name);
    }

    private void EndConversation()
    {
        isBusy = false;
        moveInput = GetRandomDirection();
        timer = directionChangeTime;
    }

    private void FaceTarget(Vector3 targetPosition)
    {
        Vector2 dir = (targetPosition - transform.position).normalized;

        if (dir.sqrMagnitude > 0.01f)
        {
            lastFacingDirection = dir;

            if (animator != null)
            {
                animator.SetFloat("x", dir.x);
                animator.SetFloat("y", dir.y);
            }
        }
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