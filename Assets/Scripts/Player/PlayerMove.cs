using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : BaseMove
{
    [Header("Run")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float runSpeed = 8f;

    private bool isRunning;

    private void Update()
    {
        if (PlayerControlLock.IsLocked)
        {
            ForceStopMovement();
            UpdateAnimation();
            return;
        }

        ReadMoveInput();
        ReadRunInput();

        moveSpeed = isRunning ? runSpeed : walkSpeed;

        UpdateAnimation();
    }

    private void ReadMoveInput()
    {
        if (Keyboard.current == null)
        {
            moveInput = Vector2.zero;
            return;
        }

        Vector2 input = Vector2.zero;

        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
        {
            input.y += 1f;
        }

        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
        {
            input.y -= 1f;
        }

        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
        {
            input.x -= 1f;
        }

        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
        {
            input.x += 1f;
        }

        if (input.sqrMagnitude > 1f)
        {
            input.Normalize();
        }

        moveInput = input;
    }

    private void ReadRunInput()
    {
        if (Keyboard.current == null)
        {
            isRunning = false;
            return;
        }

        isRunning =
            Keyboard.current.leftShiftKey.isPressed ||
            Keyboard.current.rightShiftKey.isPressed;
    }

    public void ForceStopMovement()
    {
        moveInput = Vector2.zero;
        isRunning = false;
        moveSpeed = walkSpeed;

        Rigidbody2D rb2d = GetComponent<Rigidbody2D>();

        if (rb2d != null)
        {
            rb2d.linearVelocity = Vector2.zero;
            rb2d.angularVelocity = 0f;
        }
    }
}