using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 蛇控制器 - 管理蛇的初始化、生长、重置
/// 移动逻辑委托给 SnakeMovement
/// </summary>
public class SnakeController : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private GameConfig config;

    [Header("Prefabs")]
    [SerializeField] private GameObject headPrefab;
    [SerializeField] private GameObject bodyPrefab;

    [Header("Sprites")]
    [SerializeField] private Sprite headSprite;
    [SerializeField] private Sprite bodyStraightSprite;
    [SerializeField] private Sprite bodyCornerSprite;
    [SerializeField] private Sprite tailSprite;

    private GridManager gridManager;
    private SnakeMovement movement = new SnakeMovement();
    private List<SnakeSegment> segments = new List<SnakeSegment>();
    private bool isDead;
    private Vector2Int lastTailPos;

    public IReadOnlyList<SnakeSegment> Segments => segments;
    public Vector2Int HeadGridPosition => segments.Count > 0 ? segments[0].GridPosition : Vector2Int.zero;
    public int FoodEaten { get; private set; }
    public bool IsDead => isDead;

    private void Awake()
    {
        GameServices.Register(this);
    }

    private void Start()
    {
        gridManager = GameServices.Get<GridManager>();
        ApplyConfig();
        EnsureDefaults();
        InitializeSnake();
    }

    private void Update()
    {
        if (isDead) return;

        if (movement.Tick(Time.deltaTime))
        {
            MoveSnake();
        }
    }

    private void ApplyConfig()
    {
        if (config == null) return;
        movement.BaseMoveInterval = config.BaseMoveInterval;
        movement.MinMoveInterval = config.MinMoveInterval;
        movement.SpeedUpInterval = config.SpeedUpInterval;
        movement.SpeedUpAmount = config.SpeedUpAmount;
    }

    public void InitializeSnake()
    {
        foreach (var seg in segments)
        {
            if (seg != null) Destroy(seg.gameObject);
        }
        segments.Clear();

        isDead = false;
        FoodEaten = 0;
        movement.Initialize();
        movement.IsMoving = false;

        int startX = gridManager.GridWidth / 2;
        int startY = gridManager.GridHeight / 2;

        GameObject headObj = Instantiate(headPrefab, Vector3.zero, Quaternion.identity);
        headObj.name = "Snake Head";
        headObj.SetActive(true);
        SnakeSegment headSegment = headObj.GetComponent<SnakeSegment>();
        headSegment.GridPosition = new Vector2Int(startX, startY);
        headSegment.SetSortingOrder(10);
        headSegment.FitToCell(gridManager.CellSize);
        segments.Add(headSegment);

        for (int i = 1; i <= 2; i++)
        {
            GameObject bodyObj = Instantiate(bodyPrefab, Vector3.zero, Quaternion.identity);
            bodyObj.name = $"Snake Body {i}";
            bodyObj.SetActive(true);
            SnakeSegment bodySegment = bodyObj.GetComponent<SnakeSegment>();
            bodySegment.GridPosition = new Vector2Int(startX - i, startY);
            bodySegment.SetSortingOrder(Mathf.Max(1, 10 - i));
            bodySegment.FitToCell(gridManager.CellSize);
            segments.Add(bodySegment);
        }

        UpdateSegmentPositions();
        UpdateSegmentVisuals();
        RefreshAllOccupiedCells();
        lastTailPos = segments[segments.Count - 1].GridPosition;
    }

    public void StartMoving()
    {
        movement.IsMoving = true;
    }

    public void SetDirection(Vector2Int direction)
    {
        movement.SetDirection(direction);
    }

    private void MoveSnake()
    {
        Vector2Int newHeadPos = movement.GetNextHeadPosition(segments[0].GridPosition);

        if (!gridManager.IsInBounds(newHeadPos.x, newHeadPos.y))
        {
            Die();
            return;
        }

        for (int i = 0; i < segments.Count - 1; i++)
        {
            if (segments[i].GridPosition == newHeadPos)
            {
                Die();
                return;
            }
        }

        for (int i = segments.Count - 1; i > 0; i--)
        {
            segments[i].GridPosition = segments[i - 1].GridPosition;
        }

        segments[0].GridPosition = newHeadPos;

        UpdateSegmentPositions();
        UpdateSegmentVisuals();
        UpdateOccupiedCells();
    }

    private void UpdateSegmentVisuals()
    {
        if (segments.Count == 0) return;

        // Head: faces the movement direction
        segments[0].SetSprite(headSprite);
        segments[0].SetRotation(DirectionToAngle(movement.CurrentDirection));

        // Tail (if more than 1 segment): faces toward body[count-2]
        if (segments.Count >= 2)
        {
            int tailIdx = segments.Count - 1;
            Vector2Int tailDir = segments[tailIdx - 1].GridPosition - segments[tailIdx].GridPosition;
            segments[tailIdx].SetSprite(tailSprite);
            segments[tailIdx].SetRotation(DirectionToAngle(tailDir));
        }

        // Body segments (indices 1..count-2): determine straight or corner
        for (int i = 1; i < segments.Count - 1; i++)
        {
            Vector2Int dirToHead = segments[i - 1].GridPosition - segments[i].GridPosition;
            Vector2Int dirToTail = segments[i + 1].GridPosition - segments[i].GridPosition;

            if (dirToHead == -dirToTail)
            {
                segments[i].SetSprite(bodyStraightSprite);
                segments[i].SetRotation(GetStraightAngle(dirToHead));
            }
            else
            {
                segments[i].SetSprite(bodyCornerSprite);
                segments[i].SetRotation(GetCornerAngle(dirToHead, dirToTail));
            }
        }
    }

    private float DirectionToAngle(Vector2Int dir)
    {
        if (dir == Vector2Int.right)  return 0f;
        if (dir == Vector2Int.up)     return 90f;
        if (dir == Vector2Int.left)   return 180f;
        if (dir == Vector2Int.down)   return 270f;
        return 0f;
    }

    private float GetStraightAngle(Vector2Int dir)
    {
        return (dir.x != 0) ? 0f : 90f;
    }

    private float GetCornerAngle(Vector2Int d1, Vector2Int d2)
    {
        Vector2Int sum = d1 + d2;
        if (sum.x > 0 && sum.y > 0)  return 0f;
        if (sum.x < 0 && sum.y > 0)  return 90f;
        if (sum.x < 0 && sum.y < 0)  return 180f;
        if (sum.x > 0 && sum.y < 0)  return 270f;
        return 0f;
    }

    public void Grow()
    {
        Vector2Int tailPos = segments[segments.Count - 1].GridPosition;
        GameObject bodyObj = Instantiate(bodyPrefab, Vector3.zero, Quaternion.identity);
        bodyObj.name = $"Snake Body {segments.Count}";
        bodyObj.SetActive(true);
        SnakeSegment bodySegment = bodyObj.GetComponent<SnakeSegment>();
        bodySegment.GridPosition = tailPos;
        bodySegment.SetSortingOrder(Mathf.Max(1, 10 - segments.Count));
        bodySegment.FitToCell(gridManager.CellSize);
        segments.Add(bodySegment);

        UpdateSegmentPositions();
        UpdateSegmentVisuals();
        RefreshAllOccupiedCells();
        lastTailPos = tailPos;

        FoodEaten++;
        movement.OnFoodEaten();

        EventBus.Publish(new FoodEatenEvent());
    }

    public void ResetSnake()
    {
        InitializeSnake();
    }

    private void UpdateSegmentPositions()
    {
        for (int i = 0; i < segments.Count; i++)
        {
            Vector2Int gridPos = segments[i].GridPosition;
            segments[i].transform.position = gridManager.GridToWorldPosition(gridPos.x, gridPos.y);
        }
    }

    private void UpdateOccupiedCells()
    {
        if (segments.Count == 0) return;
        Vector2Int headPos = segments[0].GridPosition;
        gridManager.SetCellOccupied(headPos.x, headPos.y, true);
        gridManager.SetCellOccupied(lastTailPos.x, lastTailPos.y, false);
        lastTailPos = segments[segments.Count - 1].GridPosition;
    }

    private void RefreshAllOccupiedCells()
    {
        gridManager.ClearOccupiedCells();
        foreach (var segment in segments)
            gridManager.SetCellOccupied(segment.GridPosition.x, segment.GridPosition.y, true);
    }

    private void Die()
    {
        isDead = true;
        movement.IsMoving = false;
        EventBus.Publish(new SnakeDiedEvent());
    }

    private void EnsureDefaults()
    {
        if (headSprite == null)
            headSprite = SnakeSpriteLoader.LoadSprite("Assets/snakesprites/png/snake_yellow_head_64.png");
        if (bodyStraightSprite == null)
            bodyStraightSprite = SnakeSpriteLoader.LoadSprite("Assets/Sprites/Generated/SnakeBodyStraight.png");
        if (bodyCornerSprite == null)
            bodyCornerSprite = SnakeSpriteLoader.LoadSprite("Assets/Sprites/Generated/SnakeBodyCorner.png");
        if (tailSprite == null)
            tailSprite = SnakeSpriteLoader.LoadSprite("Assets/Sprites/Generated/SnakeTail.png");

        if (headPrefab == null)
            headPrefab = CreateSegmentPrefab("Snake Head Prefab", headSprite);
        if (bodyPrefab == null)
            bodyPrefab = CreateSegmentPrefab("Snake Body Prefab", bodyStraightSprite);
    }

    private void OnDestroy()
    {
        GameServices.Unregister<SnakeController>();
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
