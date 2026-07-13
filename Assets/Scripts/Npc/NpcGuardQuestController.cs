using UnityEngine;

public enum GuardPatrolLine
{
    Guard1,
    Guard2
}

public class NpcGuardQuestController : BaseMove
{
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

    private Transform patrolLeftPoint;
    private Transform patrolRightPoint;
    private Transform currentPatrolTarget;

    private float patrolWaitTimer;
    private float patrolY;

    protected override void Awake()
    {
        base.Awake();
        SetupPatrolLine();
    }

    protected override void FixedUpdate()
    {
        HandlePatrol();

        base.FixedUpdate();
        UpdateAnimation();

        if (lockPatrolY)
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

    public void ResetPatrol()
    {
        patrolWaitTimer = 0f;
        moveInput = Vector2.zero;

        SetupPatrolLine();

        if (rb2d != null)
        {
            rb2d.linearVelocity = Vector2.zero;
            rb2d.angularVelocity = 0f;
        }
    }

    [ContextMenu("DEBUG - Reset Patrol")]
    private void DebugResetPatrol()
    {
        ResetPatrol();
    }

    protected override void OnDisable()
    {
        moveInput = Vector2.zero;
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