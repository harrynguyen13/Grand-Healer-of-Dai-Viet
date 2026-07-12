using UnityEngine;

public enum GuardQuestState
{
    Patrol,
    FollowPlayer,
    GoToMedicinePoint,
    WaitForMedicine,
    Leave
}

public enum GuardPatrolLine
{
    Guard1,
    Guard2
}

public class NpcGuardQuestController : BaseMove
{
    [Header("Trạng thái")]
    [SerializeField] private GuardQuestState currentState = GuardQuestState.Patrol;

    [Header("Chọn đường tuần")]
    [SerializeField] private GuardPatrolLine patrolLine = GuardPatrolLine.Guard1;
    [SerializeField] private bool startMoveToRight = true;

    [Header("4 điểm tuần trước phủ huyện")]
    [SerializeField] private Transform guard1LeftPoint;
    [SerializeField] private Transform guard1RightPoint;
    [SerializeField] private Transform guard2LeftPoint;
    [SerializeField] private Transform guard2RightPoint;

    [Header("Cấu hình đi tuần")]
    [SerializeField] private float patrolArriveDistance = 0.15f;
    [SerializeField] private float waitAtPatrolPoint = 0f;
    [SerializeField] private bool lockPatrolY = true;

    [Header("Đi theo Player khi có nhiệm vụ đặc biệt")]
    [SerializeField] private Transform playerTarget;
    [SerializeField] private float followStopDistance = 1.1f;

    [Header("Điểm nhận thuốc trong phòng thuốc")]
    [SerializeField] private Transform medicineReceivePoint;
    [SerializeField] private Transform leavePoint;
    [SerializeField] private float arriveDistance = 0.15f;

    [Header("Tránh va chạm khi làm nhiệm vụ")]
    [SerializeField] private float avoidMoveTime = 0.35f;
    [SerializeField] private float collisionAvoidCooldown = 0.25f;
    [SerializeField] private bool ignoreNpcCollision = true;

    private Transform patrolLeftPoint;
    private Transform patrolRightPoint;
    private Transform currentPatrolTarget;

    private float patrolWaitTimer;
    private float patrolY;

    private float avoidTimer;
    private float nextAvoidTime;
    private Vector2 avoidDirection;

    protected override void Awake()
    {
        base.Awake();
        SetupPatrolLine();
    }

    protected override void FixedUpdate()
    {
        if (avoidTimer > 0f && IsQuestMoveState())
        {
            HandleAvoidMovement();
        }
        else
        {
            HandleCurrentState();
        }

        base.FixedUpdate();
        UpdateAnimation();

        if (currentState == GuardQuestState.Patrol && lockPatrolY)
            LockYToPatrolLine();
    }

    private void SetupPatrolLine()
    {
        if (patrolLine == GuardPatrolLine.Guard1)
        {
            patrolLeftPoint = guard1LeftPoint;
            patrolRightPoint = guard1RightPoint;
        }
        else
        {
            patrolLeftPoint = guard2LeftPoint;
            patrolRightPoint = guard2RightPoint;
        }

        if (patrolLeftPoint != null)
            patrolY = patrolLeftPoint.position.y;
        else
            patrolY = transform.position.y;

        currentPatrolTarget = startMoveToRight ? patrolRightPoint : patrolLeftPoint;
    }

    private void HandleCurrentState()
    {
        switch (currentState)
        {
            case GuardQuestState.Patrol:
                HandlePatrol();
                break;

            case GuardQuestState.FollowPlayer:
                HandleFollowPlayer();
                break;

            case GuardQuestState.GoToMedicinePoint:
                HandleMoveToMedicinePoint();
                break;

            case GuardQuestState.WaitForMedicine:
                moveInput = Vector2.zero;
                break;

            case GuardQuestState.Leave:
                HandleLeave();
                break;
        }
    }

    private void HandlePatrol()
    {
        if (patrolLeftPoint == null || patrolRightPoint == null)
        {
            moveInput = Vector2.zero;
            return;
        }

        if (currentPatrolTarget == null)
            currentPatrolTarget = startMoveToRight ? patrolRightPoint : patrolLeftPoint;

        if (patrolWaitTimer > 0f)
        {
            patrolWaitTimer -= Time.fixedDeltaTime;
            moveInput = Vector2.zero;
            return;
        }

        Vector2 currentPosition = rb2d != null ? rb2d.position : (Vector2)transform.position;
        Vector2 targetPosition = currentPatrolTarget.position;

        if (lockPatrolY)
            targetPosition.y = patrolY;

        Vector2 direction = targetPosition - currentPosition;
        float distance = direction.magnitude;

        if (distance <= patrolArriveDistance)
        {
            SwitchPatrolTarget();
            patrolWaitTimer = waitAtPatrolPoint;
            moveInput = Vector2.zero;
            return;
        }

        moveInput = direction.normalized;
    }

    private void HandleFollowPlayer()
    {
        if (playerTarget == null)
        {
            TryFindPlayer();
            moveInput = Vector2.zero;
            return;
        }

        Vector2 direction = playerTarget.position - transform.position;
        float distance = direction.magnitude;

        if (distance <= followStopDistance)
        {
            moveInput = Vector2.zero;
            return;
        }

        moveInput = direction.normalized;
    }

    private void HandleMoveToMedicinePoint()
    {
        if (medicineReceivePoint == null)
        {
            moveInput = Vector2.zero;
            return;
        }

        Vector2 direction = medicineReceivePoint.position - transform.position;
        float distance = direction.magnitude;

        if (distance <= arriveDistance)
        {
            moveInput = Vector2.zero;
            currentState = GuardQuestState.WaitForMedicine;
            return;
        }

        moveInput = direction.normalized;
    }

    private void HandleLeave()
    {
        if (leavePoint == null)
        {
            moveInput = Vector2.zero;
            gameObject.SetActive(false);
            return;
        }

        Vector2 direction = leavePoint.position - transform.position;
        float distance = direction.magnitude;

        if (distance <= arriveDistance)
        {
            moveInput = Vector2.zero;
            gameObject.SetActive(false);
            return;
        }

        moveInput = direction.normalized;
    }

    private void HandleAvoidMovement()
    {
        avoidTimer -= Time.fixedDeltaTime;

        if (avoidTimer <= 0f)
        {
            avoidTimer = 0f;
            moveInput = Vector2.zero;
            return;
        }

        moveInput = avoidDirection;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleQuestCollision(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        HandleQuestCollision(collision);
    }

    private void HandleQuestCollision(Collision2D collision)
    {
        if (!IsQuestMoveState())
            return;

        if (collision == null)
            return;

        if (Time.time < nextAvoidTime)
            return;

        if (ignoreNpcCollision && collision.gameObject.CompareTag("NPC"))
            return;

        Vector2 awayDirection = transform.position - collision.transform.position;

        if (awayDirection.sqrMagnitude < 0.01f)
            awayDirection = GetSideAvoidDirection();

        awayDirection.Normalize();

        avoidDirection = awayDirection;
        avoidTimer = avoidMoveTime;
        nextAvoidTime = Time.time + collisionAvoidCooldown;
    }

    private Vector2 GetSideAvoidDirection()
    {
        if (playerTarget == null)
            return Vector2.down;

        Vector2 toPlayer = playerTarget.position - transform.position;

        if (Mathf.Abs(toPlayer.x) > Mathf.Abs(toPlayer.y))
            return Random.value < 0.5f ? Vector2.up : Vector2.down;

        return Random.value < 0.5f ? Vector2.left : Vector2.right;
    }

    private bool IsQuestMoveState()
    {
        return currentState == GuardQuestState.FollowPlayer
            || currentState == GuardQuestState.GoToMedicinePoint
            || currentState == GuardQuestState.Leave;
    }

    private void SwitchPatrolTarget()
    {
        if (currentPatrolTarget == patrolLeftPoint)
            currentPatrolTarget = patrolRightPoint;
        else
            currentPatrolTarget = patrolLeftPoint;
    }

    private void LockYToPatrolLine()
    {
        if (rb2d == null)
            return;

        Vector2 position = rb2d.position;

        if (Mathf.Abs(position.y - patrolY) <= 0.001f)
            return;

        rb2d.position = new Vector2(position.x, patrolY);
    }

    private void TryFindPlayer()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
            playerTarget = playerObject.transform;
    }

    public void StartFollowPlayer(Transform player)
    {
        playerTarget = player;
        currentState = GuardQuestState.FollowPlayer;
        ClearTemporaryMovement();
    }

    public void StartFollowPlayer()
    {
        TryFindPlayer();
        currentState = GuardQuestState.FollowPlayer;
        ClearTemporaryMovement();
    }

    public void GoToMedicineReceivePoint(Transform receivePoint)
    {
        medicineReceivePoint = receivePoint;
        currentState = GuardQuestState.GoToMedicinePoint;
        ClearTemporaryMovement();
    }

    public void WaitForMedicine()
    {
        currentState = GuardQuestState.WaitForMedicine;
        ClearTemporaryMovement();
    }

    public void ReceiveMedicineAndLeave(Transform exitPoint)
    {
        leavePoint = exitPoint;
        currentState = GuardQuestState.Leave;
        ClearTemporaryMovement();
    }

    public void ReturnToPatrol()
    {
        currentState = GuardQuestState.Patrol;
        patrolWaitTimer = 0f;
        ClearTemporaryMovement();
        SetupPatrolLine();
    }

    private void ClearTemporaryMovement()
    {
        moveInput = Vector2.zero;
        avoidTimer = 0f;

        if (rb2d != null)
        {
            rb2d.linearVelocity = Vector2.zero;
            rb2d.angularVelocity = 0f;
        }
    }

    [ContextMenu("DEBUG - Start Follow Player")]
    private void DebugStartFollowPlayer()
    {
        StartFollowPlayer();
    }

    [ContextMenu("DEBUG - Return To Patrol")]
    private void DebugReturnToPatrol()
    {
        ReturnToPatrol();
    }

    protected override void OnDisable()
    {
        ClearTemporaryMovement();
        base.OnDisable();
    }

    private void OnDrawGizmosSelected()
    {
        if (guard1LeftPoint != null && guard1RightPoint != null)
        {
            Gizmos.DrawLine(guard1LeftPoint.position, guard1RightPoint.position);
            Gizmos.DrawSphere(guard1LeftPoint.position, 0.15f);
            Gizmos.DrawSphere(guard1RightPoint.position, 0.15f);
        }

        if (guard2LeftPoint != null && guard2RightPoint != null)
        {
            Gizmos.DrawLine(guard2LeftPoint.position, guard2RightPoint.position);
            Gizmos.DrawSphere(guard2LeftPoint.position, 0.15f);
            Gizmos.DrawSphere(guard2RightPoint.position, 0.15f);
        }
    }
}