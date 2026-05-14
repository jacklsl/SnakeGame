using UnityEngine;

/// <summary>
/// 蛇移动逻辑 — 移动计时、方向缓冲、速度计算
/// 纯数据+逻辑（非 MonoBehaviour），可独立测试
/// </summary>
public class SnakeMovement
{
    private float moveTimer;
    private float currentMoveInterval;
    private Vector2Int currentDirection = Vector2Int.right;
    private Vector2Int nextDirection = Vector2Int.right;
    private int foodEaten;

    public float BaseMoveInterval { get; set; } = 0.2f;
    public float MinMoveInterval { get; set; } = 0.05f;
    public int SpeedUpInterval { get; set; } = 5;
    public float SpeedUpAmount { get; set; } = 0.02f;

    public Vector2Int CurrentDirection => currentDirection;
    public float CurrentMoveInterval => currentMoveInterval;
    public bool IsMoving { get; set; }

    public void Initialize()
    {
        currentMoveInterval = BaseMoveInterval;
        currentDirection = Vector2Int.right;
        nextDirection = Vector2Int.right;
        foodEaten = 0;
        moveTimer = 0;
    }

    /// <summary>
    /// 设置移动方向（不能反向）
    /// </summary>
    public void SetDirection(Vector2Int direction)
    {
        if (direction + currentDirection == Vector2Int.zero)
            return;
        nextDirection = direction;
    }

    /// <summary>
    /// 每帧调用，返回 true 表示应该移动一步
    /// </summary>
    public bool Tick(float deltaTime)
    {
        if (!IsMoving) return false;

        moveTimer += deltaTime;
        if (moveTimer >= currentMoveInterval)
        {
            moveTimer -= currentMoveInterval;
            currentDirection = nextDirection;
            return true;
        }
        return false;
    }

    /// <summary>
    /// 吃到食物时调用，处理加速
    /// </summary>
    public void OnFoodEaten()
    {
        foodEaten++;
        if (foodEaten % SpeedUpInterval == 0)
        {
            currentMoveInterval = Mathf.Max(MinMoveInterval, currentMoveInterval - SpeedUpAmount);
        }
    }

    /// <summary>
    /// 计算蛇头下一步位置
    /// </summary>
    public Vector2Int GetNextHeadPosition(Vector2Int currentHeadPos)
    {
        return currentHeadPos + currentDirection;
    }
}
