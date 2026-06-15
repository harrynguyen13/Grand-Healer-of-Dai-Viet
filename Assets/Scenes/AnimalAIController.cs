using UnityEngine;

public class AnimalAIController : BaseMove
{
    [Header("Cấu hình Động vật Tự di chuyển")]
    [SerializeField] private float directionChangeTime = 3.5f;
    [SerializeField] private float idleChance = 0.35f;

    [Header("Cải tiến Tự nhiên")]
    [Range(0f, 100f)]
    [SerializeField] private float midWayTurnChance = 0.5f;

    private float timer;

    private void Start()
    {
        ChooseNewAction();
        timer = directionChangeTime;
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            ChooseNewAction();
            timer = directionChangeTime;
        }

        if (moveInput.sqrMagnitude > 0.01f)
        {
            if (Random.value * 100f < midWayTurnChance)
            {
                moveInput = GetRandomDirection();
                timer = directionChangeTime;
            }
        }

        UpdateAnimation();
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
        HandleCollision();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        HandleCollision();
    }

    private void HandleCollision()
    {
        StopMoving();

        Vector2 currentDir = moveInput;
        Vector2 newDir = GetRandomDirection();

        int safetyNet = 0;

        while (newDir == currentDir && safetyNet < 10)
        {
            newDir = GetRandomDirection();
            safetyNet++;
        }

        moveInput = newDir;
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