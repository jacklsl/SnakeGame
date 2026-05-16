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
    [SerializeField] private Sprite bodySprite;

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
        headSegment.SetSprite(headSprite);
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
            bodySegment.SetSprite(bodySprite);
            bodySegment.SetSortingOrder(Mathf.Max(1, 10 - i));
            bodySegment.FitToCell(gridManager.CellSize);
            segments.Add(bodySegment);
        }

        UpdateSegmentPositions();
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
        UpdateOccupiedCells();
    }

    public void Grow()
    {
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
            headSprite = SnakeSpriteLoader.LoadSprite(config != null ? config.HeadSpritePath : "Assets/snakesprites/png/snake_yellow_head_64.png");
        if (bodySprite == null)
            bodySprite = SnakeSpriteLoader.LoadSprite(config != null ? config.BodySpritePath : "Assets/snakesprites/png/snake_yellow_blob_64.png");

        if (headPrefab == null)
            headPrefab = CreateSegmentPrefab("Snake Head Prefab", headSprite);
        if (bodyPrefab == null)
            bodyPrefab = CreateSegmentPrefab("Snake Body Prefab", bodySprite);
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
