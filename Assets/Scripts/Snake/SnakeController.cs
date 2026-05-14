using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 蛇控制器 - 管理蛇的移动、生长和碰撞
/// </summary>
public class SnakeController : MonoBehaviour
{
    [Header("移动设置")]
    [SerializeField] private float baseMoveInterval = 0.2f;
    [SerializeField] private float minMoveInterval = 0.05f;
    [SerializeField] private int speedUpInterval = 5; // 每吃5个食物加速
    [SerializeField] private float speedUpAmount = 0.02f;

    [Header("预制体")]
    [SerializeField] private GameObject headPrefab;
    [SerializeField] private GameObject bodyPrefab;

    [Header("精灵")]
    [SerializeField] private Sprite headSprite;
    [SerializeField] private Sprite bodySprite;

    private GridManager gridManager;
    private List<SnakeSegment> segments = new List<SnakeSegment>();
    private Vector2Int currentDirection = Vector2Int.right;
    private Vector2Int nextDirection = Vector2Int.right;
    private float moveTimer;
    private float currentMoveInterval;
    private int foodEaten;
    private bool isMoving;
    private bool isDead;

    // 事件
    public System.Action OnFoodEaten;
    public System.Action OnDeath;
    public System.Action<Vector2Int> OnSnakeMoved;

    public IReadOnlyList<SnakeSegment> Segments => segments;
    public Vector2Int HeadGridPosition => segments.Count > 0 ? segments[0].GridPosition : Vector2Int.zero;
    public int FoodEaten => foodEaten;
    public bool IsDead => isDead;

    private void Awake()
    {
        gridManager = FindAnyObjectByType<GridManager>();
        currentMoveInterval = baseMoveInterval;
        EnsureDefaults();
    }

    private void Start()
    {
        InitializeSnake();
    }

    private void Update()
    {
        if (!isMoving || isDead)
            return;

        moveTimer += Time.deltaTime;
        if (moveTimer >= currentMoveInterval)
        {
            moveTimer = 0;
            MoveSnake();
        }
    }

    /// <summary>
    /// 初始化蛇（生成初始长度3的蛇）
    /// </summary>
    public void InitializeSnake()
    {
        // 清除旧蛇
        foreach (var seg in segments)
        {
            if (seg != null)
                Destroy(seg.gameObject);
        }
        segments.Clear();

        isDead = false;
        isMoving = false;
        foodEaten = 0;
        currentDirection = Vector2Int.right;
        nextDirection = Vector2Int.right;
        currentMoveInterval = baseMoveInterval;
        moveTimer = 0;

        // 蛇初始位置在网格中央
        int startX = gridManager.GridWidth / 2;
        int startY = gridManager.GridHeight / 2;

        // 创建蛇头
        GameObject headObj = Instantiate(headPrefab, Vector3.zero, Quaternion.identity);
        headObj.name = "Snake Head";
        headObj.SetActive(true);
        SnakeSegment headSegment = headObj.GetComponent<SnakeSegment>();
        headSegment.GridPosition = new Vector2Int(startX, startY);
        headSegment.SetSprite(headSprite);
        headSegment.SetSortingOrder(10);
        headSegment.FitToCell(gridManager.CellSize);
        segments.Add(headSegment);

        // 创建蛇身（初始2节）
        for (int i = 1; i <= 2; i++)
        {
            GameObject bodyObj = Instantiate(bodyPrefab, Vector3.zero, Quaternion.identity);
            bodyObj.name = $"Snake Body {i}";
            bodyObj.SetActive(true);
            SnakeSegment bodySegment = bodyObj.GetComponent<SnakeSegment>();
            bodySegment.GridPosition = new Vector2Int(startX - i, startY);
            bodySegment.SetSprite(bodySprite);
            bodySegment.SetSortingOrder(Mathf.Max(1, 10 - i));
            bodySegment.FitToCell(gridManager.CellSize);
            segments.Add(bodySegment);
        }

        // 更新位置
        UpdateSegmentPositions();
        RefreshAllOccupiedCells();
        lastTailPos = segments[segments.Count - 1].GridPosition;
    }

    /// <summary>
    /// 开始移动
    /// </summary>
    public void StartMoving()
    {
        isMoving = true;
    }

    /// <summary>
    /// 设置移动方向
    /// </summary>
    public void SetDirection(Vector2Int direction)
    {
        // 不能反向移动
        if (direction + currentDirection == Vector2Int.zero)
            return;

        nextDirection = direction;
    }

    /// <summary>
    /// 蛇移动逻辑
    /// </summary>
    private void MoveSnake()
    {
        currentDirection = nextDirection;
        Vector2Int newHeadPos = segments[0].GridPosition + currentDirection;

        // 检查墙壁碰撞
        if (!gridManager.IsInBounds(newHeadPos.x, newHeadPos.y))
        {
            Die();
            return;
        }

        // 检查自身碰撞
        // 使用网格占用标记判断：如果目标位置已被占用（蛇身或食物），则碰撞
        // 但需要排除尾部（索引 segments.Count - 1），因为尾部会移走
        // 如果目标位置是食物位置，尾部不会移走，需要检查尾部
        // 简化处理：直接检查目标位置是否被蛇身节段占据（排除尾部）
        for (int i = 0; i < segments.Count - 1; i++)
        {
            if (segments[i].GridPosition == newHeadPos)
            {
                Die();
                return;
            }
        }

        // 移动蛇身：从尾部开始，每个节段移动到前一个节段的位置
        for (int i = segments.Count - 1; i > 0; i--)
        {
            segments[i].GridPosition = segments[i - 1].GridPosition;
        }

        // 移动蛇头
        segments[0].GridPosition = newHeadPos;

        // 更新位置
        UpdateSegmentPositions();
        UpdateOccupiedCells();

        // 触发食物检测事件（由 GameManager 处理）
        OnSnakeMoved?.Invoke(newHeadPos);
    }

    /// <summary>
    /// 蛇生长（吃到食物时调用）
    /// </summary>
    public void Grow()
    {
        // 在尾部添加新节段
        Vector2Int tailPos = segments[segments.Count - 1].GridPosition;
        GameObject bodyObj = Instantiate(bodyPrefab, Vector3.zero, Quaternion.identity);
        bodyObj.name = $"Snake Body {segments.Count}";
        bodyObj.SetActive(true);
        SnakeSegment bodySegment = bodyObj.GetComponent<SnakeSegment>();
        bodySegment.GridPosition = tailPos;
        bodySegment.SetSprite(bodySprite);
        bodySegment.SetSortingOrder(Mathf.Max(1, 10 - segments.Count));
        bodySegment.FitToCell(gridManager.CellSize);
        segments.Add(bodySegment);

        UpdateSegmentPositions();
        RefreshAllOccupiedCells();
        lastTailPos = tailPos;

        foodEaten++;

        // 检查是否需要加速
        if (foodEaten % speedUpInterval == 0)
        {
            currentMoveInterval = Mathf.Max(minMoveInterval, currentMoveInterval - speedUpAmount);
        }

        OnFoodEaten?.Invoke();
    }

    /// <summary>
    /// 更新所有节段的世界位置
    /// </summary>
    private void UpdateSegmentPositions()
    {
        for (int i = 0; i < segments.Count; i++)
        {
            Vector2Int gridPos = segments[i].GridPosition;
            segments[i].transform.position = gridManager.GridToWorldPosition(gridPos.x, gridPos.y);
        }
    }

    private Vector2Int lastTailPos;

    /// <summary>
    /// 更新网格占用状态 — 仅更新变化的格子（蛇头新位置 + 蛇尾旧位置）
    /// </summary>
    private void UpdateOccupiedCells()
    {
        if (segments.Count == 0)
            return;

        // 标记蛇头新位置为占用
        Vector2Int headPos = segments[0].GridPosition;
        gridManager.SetCellOccupied(headPos.x, headPos.y, true);

        // 标记旧蛇尾位置为空（蛇尾已移走）
        gridManager.SetCellOccupied(lastTailPos.x, lastTailPos.y, false);

        // 记录当前蛇尾位置供下次使用
        lastTailPos = segments[segments.Count - 1].GridPosition;
    }

    /// <summary>
    /// 全量刷新占用状态 — 仅在初始化/重置/生长时调用
    /// </summary>
    private void RefreshAllOccupiedCells()
    {
        gridManager.ClearOccupiedCells();
        foreach (var segment in segments)
        {
            gridManager.SetCellOccupied(segment.GridPosition.x, segment.GridPosition.y, true);
        }
    }

    /// <summary>
    /// 蛇死亡
    /// </summary>
    private void Die()
    {
        isDead = true;
        isMoving = false;
        OnDeath?.Invoke();
    }

    /// <summary>
    /// 重置蛇
    /// </summary>
    public void ResetSnake()
    {
        InitializeSnake();
    }

    private void EnsureDefaults()
    {
        if (headSprite == null)
            headSprite = SnakeSpriteLoader.LoadSprite("Assets/snakesprites/png/snake_yellow_head_64.png");
        if (bodySprite == null)
            bodySprite = SnakeSpriteLoader.LoadSprite("Assets/snakesprites/png/snake_yellow_blob_64.png");

        if (headPrefab == null)
            headPrefab = CreateSegmentPrefab("Snake Head Prefab", headSprite);
        if (bodyPrefab == null)
            bodyPrefab = CreateSegmentPrefab("Snake Body Prefab", bodySprite);
    }

    /// <summary>
    /// 销毁时清理资源
    /// </summary>
    private void OnDestroy()
    {
        // 清理蛇身节段
        foreach (var seg in segments)
        {
            if (seg != null && seg.gameObject != null)
                Destroy(seg.gameObject);
        }
        segments.Clear();
    }

    private GameObject CreateSegmentPrefab(string prefabName, Sprite sprite)
    {
        GameObject prefab = new GameObject(prefabName);
        prefab.hideFlags = HideFlags.HideAndDontSave;
        prefab.SetActive(false);
        SpriteRenderer renderer = prefab.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        prefab.AddComponent<SnakeSegment>();
        return prefab;
    }
}
