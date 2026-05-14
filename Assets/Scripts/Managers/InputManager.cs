using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 输入管理器 — 处理键盘和触控输入
/// </summary>
public class InputManager : MonoBehaviour
{
    private SnakeController snakeController;
    private GameManager gameManager;

    private Vector2 touchStartPos;
    private bool isTouchInput;
    private readonly float minSwipeDistance = 50f;

    private void Start()
    {
        snakeController = GameServices.Get<SnakeController>();
        gameManager = GameServices.Get<GameManager>();
    }

    private void Update()
    {
        if (gameManager == null) return;

        HandlePauseToggle();

        if (gameManager.CurrentState == GameState.Playing)
        {
            HandleKeyboardInput();
            HandleTouchInput();
        }
    }

    private void HandlePauseToggle()
    {
        if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame) return;

        if (gameManager.CurrentState == GameState.Playing)
            gameManager.PauseGame();
        else if (gameManager.CurrentState == GameState.Paused)
            gameManager.ResumeGame();
    }

    private void HandleKeyboardInput()
    {
        if (Keyboard.current == null || snakeController == null) return;

        if (Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame)
            snakeController.SetDirection(Vector2Int.up);
        else if (Keyboard.current.sKey.wasPressedThisFrame || Keyboard.current.downArrowKey.wasPressedThisFrame)
            snakeController.SetDirection(Vector2Int.down);
        else if (Keyboard.current.aKey.wasPressedThisFrame || Keyboard.current.leftArrowKey.wasPressedThisFrame)
            snakeController.SetDirection(Vector2Int.left);
        else if (Keyboard.current.dKey.wasPressedThisFrame || Keyboard.current.rightArrowKey.wasPressedThisFrame)
            snakeController.SetDirection(Vector2Int.right);
    }

    private void HandleTouchInput()
    {
        if (Touchscreen.current == null || snakeController == null) return;

        if (Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            touchStartPos = Touchscreen.current.primaryTouch.position.ReadValue();
            isTouchInput = true;
        }

        if (isTouchInput && Touchscreen.current.primaryTouch.press.wasReleasedThisFrame)
        {
            Vector2 touchEndPos = Touchscreen.current.primaryTouch.position.ReadValue();
            Vector2 swipeDelta = touchEndPos - touchStartPos;

            if (swipeDelta.magnitude < minSwipeDistance) return;

            if (Mathf.Abs(swipeDelta.x) > Mathf.Abs(swipeDelta.y))
                snakeController.SetDirection(swipeDelta.x > 0 ? Vector2Int.right : Vector2Int.left);
            else
                snakeController.SetDirection(swipeDelta.y > 0 ? Vector2Int.up : Vector2Int.down);

            isTouchInput = false;
        }
    }
}
