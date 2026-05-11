using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 输入管理器 - 处理键盘和触控输入
/// 使用 Unity 新输入系统（Input System）
/// </summary>
public class InputManager : MonoBehaviour
{
    private SnakeController snakeController;
    private GameManager gameManager;

    private Vector2 touchStartPos;
    private bool isTouchInput;
    private float minSwipeDistance = 50f;
    private bool hasTouchscreen;

    private void Awake()
    {
        snakeController = FindObjectOfType<SnakeController>();
        gameManager = FindObjectOfType<GameManager>();
        // 缓存触控设备状态，避免每帧检查
        hasTouchscreen = Touchscreen.current != null;
    }

    private void Update()
    {
        if (gameManager == null)
        {
            Debug.LogError("[InputManager] gameManager is NULL!");
            return;
        }

        Debug.Log($"[InputManager] Update - CurrentState: {gameManager.CurrentState}, snakeController: {(snakeController != null ? "OK" : "NULL")}");

        // 暂停切换在任何状态下都可用
        HandlePauseToggle();

        // 只有 Playing 状态下才处理方向输入
        if (gameManager.CurrentState == GameState.Playing)
        {
            HandleKeyboardInput();
            HandleTouchInput();
        }
    }

    private void HandlePauseToggle()
    {
        if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame)
            return;

        if (gameManager.CurrentState == GameState.Playing)
            gameManager.PauseGame();
        else if (gameManager.CurrentState == GameState.Paused)
            gameManager.ResumeGame();
    }

    /// <summary>
    /// 处理键盘输入（WASD / 方向键）
    /// </summary>
    private void HandleKeyboardInput()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            snakeController?.SetDirection(Vector2Int.up);
        }
        else if (Keyboard.current.sKey.wasPressedThisFrame || Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            snakeController?.SetDirection(Vector2Int.down);
        }
        else if (Keyboard.current.aKey.wasPressedThisFrame || Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            snakeController?.SetDirection(Vector2Int.left);
        }
        else if (Keyboard.current.dKey.wasPressedThisFrame || Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            snakeController?.SetDirection(Vector2Int.right);
        }
    }

    /// <summary>
    /// 处理触控输入（滑动屏幕控制方向）
    /// </summary>
    private void HandleTouchInput()
    {
        if (!hasTouchscreen || Touchscreen.current == null) return;

        if (Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            touchStartPos = Touchscreen.current.primaryTouch.position.ReadValue();
            isTouchInput = true;
        }

        if (isTouchInput && Touchscreen.current.primaryTouch.press.wasReleasedThisFrame)
        {
            Vector2 touchEndPos = Touchscreen.current.primaryTouch.position.ReadValue();
            Vector2 swipeDelta = touchEndPos - touchStartPos;

            if (swipeDelta.magnitude < minSwipeDistance)
                return;

            // 判断滑动方向
            if (Mathf.Abs(swipeDelta.x) > Mathf.Abs(swipeDelta.y))
            {
                // 水平滑动
                if (swipeDelta.x > 0)
                    snakeController?.SetDirection(Vector2Int.right);
                else
                    snakeController?.SetDirection(Vector2Int.left);
            }
            else
            {
                // 垂直滑动
                if (swipeDelta.y > 0)
                    snakeController?.SetDirection(Vector2Int.up);
                else
                    snakeController?.SetDirection(Vector2Int.down);
            }

            isTouchInput = false;
        }
    }
}
