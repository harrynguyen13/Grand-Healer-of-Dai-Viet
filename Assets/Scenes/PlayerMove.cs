using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : BaseMove
{
    [Header("Run")]
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float runSpeed = 4f;

    private bool isRunning;

    private void Update()
    {
        if (Keyboard.current != null)
        {
            isRunning = Keyboard.current.leftShiftKey.isPressed;
        }

        moveSpeed = isRunning ? runSpeed : walkSpeed;

        UpdateAnimation();
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();

        if (Mathf.Abs(moveInput.x) < 0.1f)
            moveInput.x = 0f;

        if (Mathf.Abs(moveInput.y) < 0.1f)
            moveInput.y = 0f;

        if (moveInput.sqrMagnitude > 1f)
            moveInput.Normalize();

        if (moveInput.sqrMagnitude <= 0.01f)
            moveInput = Vector2.zero;
    }

    public void OnRun(InputValue value)
    {
        isRunning = value.isPressed;
    }
}