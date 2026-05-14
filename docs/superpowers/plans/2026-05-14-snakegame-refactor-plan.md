# SnakeGame 代码质量重构 — 实施计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 重构 SnakeGame 代码架构，引入 GameServices + EventBus + GameConfig，拆分大类，解耦模块

**Architecture:** 轻量 Service Locator 替代 FindAnyObjectByType，结构化 EventBus 替代分散的 System.Action，ScriptableObject 集中配置。按单一职责拆分 SnakeController、GridManager、UIManager

**Tech Stack:** Unity 6000.4.5f1, C#, URP

---

### Task 1: 创建 Core 基础设施 — 事件类型

**Files:**
- Create: `Assets/Scripts/Core/EventTypes.cs`

- [ ] **Step 1: 创建事件结构体文件**

```bash
mkdir -p Assets/Scripts/Core
```

- [ ] **Step 2: 写入 EventTypes.cs**

```csharp
// Assets/Scripts/Core/EventTypes.cs

/// <summary>
/// 事件类型定义 — 所有事件均为 struct，零 GC 分配
/// </summary>

public struct FoodEatenEvent { }

public struct SnakeDiedEvent { }

public struct ScoreChangedEvent
{
    public int Score;
    public int HighScore;
    public bool IsNewRecord;
}

public struct GameStateChangedEvent
{
    public GameState State;
}
```

- [ ] **Step 3: 验证编译**

在 Unity Editor 中确认 Console 无编译错误。

- [ ] **Step 4: 提交**

```bash
git add Assets/Scripts/Core/EventTypes.cs Assets/Scripts/Core.meta
git commit -m "feat: add event types (FoodEaten, SnakeDied, ScoreChanged, GameStateChanged)"
```

---

### Task 2: 创建 Core 基础设施 — EventBus

**Files:**
- Create: `Assets/Scripts/Core/EventBus.cs`

- [ ] **Step 1: 写入 EventBus.cs**

```csharp
// Assets/Scripts/Core/EventBus.cs
using System;
using System.Collections.Generic;

/// <summary>
/// 全局事件总线 — 订阅/发布 struct 事件，零 GC 分配
/// </summary>
public static class EventBus
{
    private static readonly Dictionary<Type, Delegate> _handlers = new();

    public static void Subscribe<T>(Action<T> handler) where T : struct
    {
        Type type = typeof(T);
        if (_handlers.ContainsKey(type))
            _handlers[type] = Delegate.Combine(_handlers[type], handler);
        else
            _handlers[type] = handler;
    }

    public static void Unsubscribe<T>(Action<T> handler) where T : struct
    {
        Type type = typeof(T);
        if (!_handlers.ContainsKey(type))
            return;

        Delegate combined = Delegate.Remove(_handlers[type], handler);
        if (combined == null)
            _handlers.Remove(type);
        else
            _handlers[type] = combined;
    }

    public static void Publish<T>(T evt) where T : struct
    {
        Type type = typeof(T);
        if (_handlers.TryGetValue(type, out Delegate del) && del is Action<T> action)
            action.Invoke(evt);
    }

    /// <summary>
    /// 清除所有订阅（场景切换时调用）
    /// </summary>
    public static void Clear()
    {
        _handlers.Clear();
    }
}
```

- [ ] **Step 2: 验证编译**

在 Unity Editor 中确认 Console 无编译错误。

- [ ] **Step 3: 提交**

```bash
git add Assets/Scripts/Core/EventBus.cs Assets/Scripts/Core.meta
git commit -m "feat: add EventBus for struct-based event publishing"
```

---

### Task 3: 创建 Core 基础设施 — GameServices

**Files:**
- Create: `Assets/Scripts/Core/GameServices.cs`

- [ ] **Step 1: 写入 GameServices.cs**

```csharp
// Assets/Scripts/Core/GameServices.cs
using System;
using System.Collections.Generic;

/// <summary>
/// 轻量服务定位器 — 替代 FindAnyObjectByType
/// 组件在 Awake 注册，OnDestroy 注销
/// </summary>
public static class GameServices
{
    private static readonly Dictionary<Type, object> _services = new();

    public static void Register<T>(T service) where T : class
    {
        _services[typeof(T)] = service;
    }

    public static void Unregister<T>() where T : class
    {
        _services.Remove(typeof(T));
    }

    public static T Get<T>() where T : class
    {
        if (_services.TryGetValue(typeof(T), out object service))
            return service as T;
        return null;
    }

    /// <summary>
    /// 清除所有注册（场景切换时调用）
    /// </summary>
    public static void Clear()
    {
        _services.Clear();
    }
}
```

- [ ] **Step 2: 验证编译**

在 Unity Editor 中确认 Console 无编译错误。

- [ ] **Step 3: 提交**

```bash
git add Assets/Scripts/Core/GameServices.cs Assets/Scripts/Core.meta
git commit -m "feat: add GameServices service locator"
```

---

### Task 4: 创建 Core 基础设施 — GameConfig

**Files:**
- Create: `Assets/Scripts/Core/GameConfig.cs`

- [ ] **Step 1: 创建 Config 目录**

```bash
mkdir -p Assets/Config
```

- [ ] **Step 2: 写入 GameConfig.cs**

```csharp
// Assets/Scripts/Core/GameConfig.cs
using UnityEngine;

/// <summary>
/// 游戏配置 ScriptableObject — 集中管理所有魔法数字和资源路径
/// 在 Unity Editor 中通过 Create > Game Config 创建资产文件
/// </summary>
[CreateAssetMenu(fileName = "GameConfig", menuName = "Create/Game Config", order = 1)]
public class GameConfig : ScriptableObject
{
    [Header("Grid")]
    public int GridWidth = 20;
    public int GridHeight = 20;
    public float CellSize = 1f;

    [Header("Movement")]
    public float BaseMoveInterval = 0.2f;
    public float MinMoveInterval = 0.05f;
    public int SpeedUpInterval = 5;
    public float SpeedUpAmount = 0.02f;

    [Header("Sprite Paths")]
    public string HeadSpritePath = "Assets/snakesprites/png/snake_yellow_head_64.png";
    public string BodySpritePath = "Assets/snakesprites/png/snake_yellow_blob_64.png";
    public string FoodSpritePath = "Assets/snakesprites/png/apple_red_64.png";
    public string WallSpritePath = "Assets/snakesprites/png/wall_block_64_0.png";

    [Header("Background Colors")]
    public Color BackgroundColorLight = new Color(0.45f, 0.78f, 0.30f);
    public Color BackgroundColorDark = new Color(0.22f, 0.55f, 0.15f);
}
```

- [ ] **Step 3: 在 Unity Editor 中创建 GameConfig.asset**

在 Unity Editor 中：右键 Project 窗口 > Create > Game Config，将生成的 `GameConfig.asset` 拖入 `Assets/Config/` 目录。

- [ ] **Step 4: 验证编译**

在 Unity Editor 中确认 Console 无编译错误。

- [ ] **Step 5: 提交**

```bash
git add Assets/Scripts/Core/GameConfig.cs Assets/Scripts/Core.meta Assets/Config/ Assets/Config.meta
git commit -m "feat: add GameConfig ScriptableObject for centralized configuration"
```

---

### Task 5: 拆分 GridManager — 创建 GridBackgroundRenderer

**Files:**
- Create: `Assets/Scripts/Grid/GridBackgroundRenderer.cs`
- Modify: `Assets/Scripts/Grid/GridManager.cs`

- [ ] **Step 1: 创建 GridBackgroundRenderer.cs**

```csharp
// Assets/Scripts/Grid/GridBackgroundRenderer.cs
using UnityEngine;

/// <summary>
/// 棋盘格背景渲染器 — 负责生成和渲染网格背景
/// </summary>
public class GridBackgroundRenderer : MonoBehaviour
{
    [SerializeField] private GameConfig config;
    [SerializeField] private SpriteRenderer backgroundRenderer;

    public void Generate(int gridWidth, int gridHeight, float cellSize)
    {
        int texWidth = gridWidth;
        int texHeight = gridHeight;
        Texture2D bgTexture = new Texture2D(texWidth, texHeight, TextureFormat.RGBA32, false);
        bgTexture.filterMode = FilterMode.Point;
        bgTexture.wrapMode = TextureWrapMode.Clamp;

        Color light = config != null ? config.BackgroundColorLight : new Color(0.45f, 0.78f, 0.30f);
        Color dark = config != null ? config.BackgroundColorDark : new Color(0.22f, 0.55f, 0.15f);

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                bgTexture.SetPixel(x, y, (x + y) % 2 == 0 ? light : dark);
            }
        }
        bgTexture.Apply();

        Vector2 pivot = new Vector2(0.5f, 0.5f);
        Sprite bgSprite = Sprite.Create(bgTexture, new Rect(0, 0, texWidth, texHeight), pivot, 1f / cellSize);

        if (backgroundRenderer == null)
        {
            GameObject bgObject = new GameObject("Background");
            bgObject.transform.SetParent(transform);
            bgObject.transform.position = Vector3.zero;
            backgroundRenderer = bgObject.AddComponent<SpriteRenderer>();
        }

        backgroundRenderer.sprite = bgSprite;
        backgroundRenderer.sortingOrder = -5;
    }
}
```

- [ ] **Step 2: 从 GridManager.cs 中移除背景生成逻辑**

修改 `GridManager.cs`：
- 删除 `GenerateBackground()` 方法体，替换为委托调用
- 删除 `backgroundTilePrefab` 字段
- 简化 `EnsureDefaults()` 移除背景相关代码
- 在 `Start()` 中将背景生成委托给 `GridBackgroundRenderer`

具体修改如下：

**删除字段 `backgroundTilePrefab`：**
```csharp
// 删除这行
[SerializeField] private GameObject backgroundTilePrefab;
```

**修改 `Start()` 方法：**
```csharp
private void Start()
{
    GridBackgroundRenderer bgRenderer = GetComponent<GridBackgroundRenderer>();
    if (bgRenderer == null)
        bgRenderer = gameObject.AddComponent<GridBackgroundRenderer>();
    bgRenderer.Generate(gridWidth, gridHeight, cellSize);

    GenerateWalls();
}
```

**删除 `GenerateBackground()` 方法**（整个方法 40-78 行）。

**简化 `EnsureDefaults()` — 移除 backgroundTilePrefab 相关代码：**
```csharp
private void EnsureDefaults()
{
    if (wallPrefab == null)
    {
        wallPrefab = CreateSpritePrefab(
            "Wall Prefab",
            SnakeSpriteLoader.LoadSprite("Assets/snakesprites/png/wall_block_64_0.png"),
            2);
    }
}
```

- [ ] **Step 3: 验证编译并运行**

在 Unity Editor 中 Play，确认背景仍然正确渲染。

- [ ] **Step 4: 提交**

```bash
git add Assets/Scripts/Grid/GridBackgroundRenderer.cs Assets/Scripts/Grid/GridBackgroundRenderer.cs.meta Assets/Scripts/Grid/GridManager.cs
git commit -m "refactor: extract GridBackgroundRenderer from GridManager"
```

---

### Task 6: 拆分 GridManager — 创建 GridWallRenderer

**Files:**
- Create: `Assets/Scripts/Grid/GridWallRenderer.cs`
- Modify: `Assets/Scripts/Grid/GridManager.cs`

- [ ] **Step 1: 创建 GridWallRenderer.cs**

```csharp
// Assets/Scripts/Grid/GridWallRenderer.cs
using UnityEngine;

/// <summary>
/// 墙壁渲染器 — 负责墙壁对象的生成和缩放
/// </summary>
public class GridWallRenderer : MonoBehaviour
{
    [SerializeField] private GameObject wallPrefab;

    public void Generate(int gridWidth, int gridHeight, float cellSize)
    {
        EnsurePrefab();

        // 上下边界
        for (int x = -1; x <= gridWidth; x++)
        {
            SpawnWall(x, -1, cellSize);
            SpawnWall(x, gridHeight, cellSize);
        }

        // 左右边界
        for (int y = 0; y < gridHeight; y++)
        {
            SpawnWall(-1, y, cellSize);
            SpawnWall(gridWidth, y, cellSize);
        }
    }

    private void SpawnWall(int gridX, int gridY, float cellSize)
    {
        // 计算世界坐标：与 GridManager.GridToWorldPosition 一致
        float originX = -gridWidth() * cellSize / 2f;
        float originY = -gridHeight() * cellSize / 2f;
        float worldX = originX + (gridX + 0.5f) * cellSize;
        float worldY = originY + (gridY + 0.5f) * cellSize;
        Vector3 pos = new Vector3(worldX, worldY, 0);

        GameObject wall = Instantiate(wallPrefab, pos, Quaternion.identity, transform);
        wall.name = $"Wall_{gridX}_{gridY}";
        FitSpriteToCell(wall, cellSize, 1f);
    }

    private void EnsurePrefab()
    {
        if (wallPrefab == null)
        {
            wallPrefab = CreateSpritePrefab(
                "Wall Prefab",
                SnakeSpriteLoader.LoadSprite("Assets/snakesprites/png/wall_block_64_0.png"),
                2);
        }
    }

    private static GameObject CreateSpritePrefab(string prefabName, Sprite sprite, int sortingOrder)
    {
        GameObject prefab = new GameObject(prefabName);
        prefab.SetActive(false);
        SpriteRenderer renderer = prefab.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = sortingOrder;
        return prefab;
    }

    private static void FitSpriteToCell(GameObject obj, float cellSize, float padding)
    {
        obj.SetActive(true);
        SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
        if (renderer == null || renderer.sprite == null)
            return;

        Vector2 size = renderer.sprite.bounds.size;
        float largestSide = Mathf.Max(size.x, size.y);
        if (largestSide <= 0f)
            return;

        float scale = cellSize * padding / largestSide;
        obj.transform.localScale = new Vector3(scale, scale, 1f);
    }
}
```

Wait — `SpawnWall` 需要 `gridWidth` 和 `gridHeight` 来计算原点，但它从参数传入。让我修正设计：`GridWallRenderer` 需要知道 `gridWidth` 和 `gridHeight`。

修正后的 `GridWallRenderer.cs`：

```csharp
// Assets/Scripts/Grid/GridWallRenderer.cs
using UnityEngine;

/// <summary>
/// 墙壁渲染器 — 负责墙壁对象的生成和缩放
/// </summary>
public class GridWallRenderer : MonoBehaviour
{
    [SerializeField] private GameObject wallPrefab;

    private int gridWidth;
    private int gridHeight;
    private float cellSize;

    public void Generate(int width, int height, float cellSize)
    {
        this.gridWidth = width;
        this.gridHeight = height;
        this.cellSize = cellSize;

        EnsurePrefab();

        // 上下边界
        for (int x = -1; x <= gridWidth; x++)
        {
            SpawnWall(x, -1);
            SpawnWall(x, gridHeight);
        }

        // 左右边界
        for (int y = 0; y < gridHeight; y++)
        {
            SpawnWall(-1, y);
            SpawnWall(gridWidth, y);
        }
    }

    private void SpawnWall(int gridX, int gridY)
    {
        float originX = -gridWidth * cellSize / 2f;
        float originY = -gridHeight * cellSize / 2f;
        float worldX = originX + (gridX + 0.5f) * cellSize;
        float worldY = originY + (gridY + 0.5f) * cellSize;

        GameObject wall = Instantiate(wallPrefab, new Vector3(worldX, worldY, 0), Quaternion.identity, transform);
        wall.name = $"Wall_{gridX}_{gridY}";
        FitToCell(wall);
    }

    private void FitToCell(GameObject obj)
    {
        obj.SetActive(true);
        SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
        if (renderer == null || renderer.sprite == null)
            return;

        Vector2 size = renderer.sprite.bounds.size;
        float largestSide = Mathf.Max(size.x, size.y);
        if (largestSide <= 0f)
            return;

        float scale = cellSize * 1f / largestSide;
        obj.transform.localScale = new Vector3(scale, scale, 1f);
    }

    private void EnsurePrefab()
    {
        if (wallPrefab != null) return;

        wallPrefab = new GameObject("Wall Prefab");
        wallPrefab.SetActive(false);
        SpriteRenderer renderer = wallPrefab.AddComponent<SpriteRenderer>();
        renderer.sprite = SnakeSpriteLoader.LoadSprite("Assets/snakesprites/png/wall_block_64_0.png");
        renderer.sortingOrder = 2;
    }
}
```

- [ ] **Step 2: 修改 GridManager.cs — 移除墙壁生成逻辑**

修改 `GridManager.cs` 的 `Start()`：

```csharp
private void Start()
{
    GridBackgroundRenderer bgRenderer = GetComponent<GridBackgroundRenderer>();
    if (bgRenderer == null)
        bgRenderer = gameObject.AddComponent<GridBackgroundRenderer>();
    bgRenderer.Generate(gridWidth, gridHeight, cellSize);

    GridWallRenderer wallRenderer = GetComponent<GridWallRenderer>();
    if (wallRenderer == null)
        wallRenderer = gameObject.AddComponent<GridWallRenderer>();
    wallRenderer.Generate(gridWidth, gridHeight, cellSize);
}
```

删除 `GridManager.cs` 中的：
- `wallPrefab` 字段
- `GenerateWalls()` 方法
- `SpawnWall()` 方法
- `FitSpriteToCell()` 方法
- `CreateSpritePrefab()` 方法
- `EnsureDefaults()` 方法中 wallPrefab 相关代码（整个 EnsureDefaults 方法可删除）

- [ ] **Step 3: 验证编译并运行**

在 Unity Editor 中 Play，确认墙壁和背景正常渲染。

- [ ] **Step 4: 提交**

```bash
git add Assets/Scripts/Grid/GridWallRenderer.cs Assets/Scripts/Grid/GridWallRenderer.cs.meta Assets/Scripts/Grid/GridManager.cs
git commit -m "refactor: extract GridWallRenderer from GridManager"
```

---

### Task 7: 拆分 SnakeController — 创建 SnakeMovement

**Files:**
- Create: `Assets/Scripts/Snake/SnakeMovement.cs`
- Modify: `Assets/Scripts/Snake/SnakeController.cs`

- [ ] **Step 1: 创建 SnakeMovement.cs**

```csharp
// Assets/Scripts/Snake/SnakeMovement.cs
using UnityEngine;

/// <summary>
/// 蛇移动逻辑 — 移动计时、方向缓冲、碰撞检测
/// 从 SnakeController 中拆分出来，纯数据+逻辑（非 MonoBehaviour）
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
        if (!IsMoving)
            return false;

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
```

- [ ] **Step 2: 修改 SnakeController.cs — 引入 SnakeMovement**

修改 `SnakeController.cs`：

1. 添加 `SnakeMovement` 字段，替换原有移动相关字段
2. 删除字段：`currentDirection`, `nextDirection`, `moveTimer`, `currentMoveInterval`, `foodEaten`, `isMoving`
3. 删除字段：`baseMoveInterval`, `minMoveInterval`, `speedUpInterval`, `speedUpAmount`（迁移到 GameConfig）
4. `SetDirection()` 委托给 `movement.SetDirection()`
5. `Update()` 中 `moveTimer` 逻辑替换为 `movement.Tick()`
6. `Grow()` 中加速逻辑替换为 `movement.OnFoodEaten()`

修改后的关键方法：

```csharp
// 新增字段
private SnakeMovement movement = new SnakeMovement();

// 修改 InitializeSnake() 中的初始化
movement.Initialize();
movement.IsMoving = false;

// 修改 Update()
private void Update()
{
    if (isDead) return;

    if (movement.Tick(Time.deltaTime))
    {
        MoveSnake();
    }
}

// 修改 SetDirection()
public void SetDirection(Vector2Int direction)
{
    movement.SetDirection(direction);
}

// 修改 StartMoving()
public void StartMoving()
{
    movement.IsMoving = true;
}

// 修改 MoveSnake() 中的方向引用
private void MoveSnake()
{
    Vector2Int newHeadPos = movement.GetNextHeadPosition(segments[0].GridPosition);
    // ... 其余不变
}

// 修改 Grow() 中的加速逻辑
movement.OnFoodEaten();

// 删除 FoodEaten 属性，替换为
public int FoodEaten { get; private set; }
// 在 Grow() 中 increment: FoodEaten++;
```

完整修改后的 `SnakeController.cs`：

```csharp
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

    // 事件（保留用于 GameManager 旧版兼容，逐步迁移到 EventBus）
    public System.Action<Vector2Int> OnSnakeMoved;

    public IReadOnlyList<SnakeSegment> Segments => segments;
    public Vector2Int HeadGridPosition => segments.Count > 0 ? segments[0].GridPosition : Vector2Int.zero;
    public int FoodEaten { get; private set; }
    public bool IsDead => isDead;

    private void Awake()
    {
        gridManager = GameServices.Get<GridManager>();
        ApplyConfig();
        EnsureDefaults();
    }

    private void Start()
    {
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

        OnSnakeMoved?.Invoke(newHeadPos);
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

    private Vector2Int lastTailPos;

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
        {
            gridManager.SetCellOccupied(segment.GridPosition.x, segment.GridPosition.y, true);
        }
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
        if (bodySprite == null)
            bodySprite = SnakeSpriteLoader.LoadSprite("Assets/snakesprites/png/snake_yellow_blob_64.png");

        if (headPrefab == null)
            headPrefab = CreateSegmentPrefab("Snake Head Prefab", headSprite);
        if (bodyPrefab == null)
            bodyPrefab = CreateSegmentPrefab("Snake Body Prefab", bodySprite);
    }

    private void OnDestroy()
    {
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
```

- [ ] **Step 3: 创建 GameConfig.asset 引用**

SnakeController 现在需要 `[SerializeField] private GameConfig config;`。在 Unity Editor 中将 `GameConfig.asset` 拖入 SnakeController 的 Config 字段。

- [ ] **Step 4: 验证编译并运行**

在 Unity Editor 中 Play，确认蛇移动正常（速度、方向、碰撞检测）。

- [ ] **Step 5: 提交**

```bash
git add Assets/Scripts/Snake/SnakeMovement.cs Assets/Scripts/Snake/SnakeMovement.cs.meta Assets/Scripts/Snake/SnakeController.cs
git commit -m "refactor: extract SnakeMovement from SnakeController, use GameConfig"
```

---

### Task 8: 拆分 UIManager — 创建 View 类（MainMenuView + GameHudView）

**Files:**
- Create: `Assets/Scripts/UI/MainMenuView.cs`
- Create: `Assets/Scripts/UI/GameHudView.cs`

- [ ] **Step 1: 创建 MainMenuView.cs**

```csharp
// Assets/Scripts/UI/MainMenuView.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 主菜单视图 — 标题、最高分、开始/退出按钮
/// </summary>
public class MainMenuView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private Button startButton;
    [SerializeField] private Button quitButton;

    private void Start()
    {
        if (startButton != null)
            startButton.onClick.AddListener(OnStartClicked);
        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);

        EventBus.Subscribe<ScoreChangedEvent>(OnScoreChanged);
    }

    public void Show()
    {
        gameObject.SetActive(true);
        ScoreManager sm = GameServices.Get<ScoreManager>();
        if (highScoreText != null && sm != null)
            highScoreText.text = $"Best: {sm.HighScore}";
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnStartClicked()
    {
        GameManager gm = GameServices.Get<GameManager>();
        gm?.StartGame();
    }

    private void OnQuitClicked()
    {
        GameManager gm = GameServices.Get<GameManager>();
        gm?.QuitGame();
    }

    private void OnScoreChanged(ScoreChangedEvent evt)
    {
        if (highScoreText != null)
            highScoreText.text = $"Best: {evt.HighScore}";
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<ScoreChangedEvent>(OnScoreChanged);
    }
}
```

- [ ] **Step 2: 创建 GameHudView.cs**

```csharp
// Assets/Scripts/UI/GameHudView.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 游戏 HUD 视图 — 得分、最高分、暂停按钮
/// </summary>
public class GameHudView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private Button pauseButton;

    private void Start()
    {
        if (pauseButton != null)
            pauseButton.onClick.AddListener(OnPauseClicked);

        EventBus.Subscribe<ScoreChangedEvent>(OnScoreChanged);
    }

    public void Show()
    {
        gameObject.SetActive(true);
        ScoreManager sm = GameServices.Get<ScoreManager>();
        if (scoreText != null && sm != null)
            scoreText.text = $"Score: {sm.CurrentScore}";
        if (highScoreText != null && sm != null)
            highScoreText.text = $"Best: {sm.HighScore}";
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnScoreChanged(ScoreChangedEvent evt)
    {
        if (scoreText != null)
            scoreText.text = $"Score: {evt.Score}";
        if (highScoreText != null)
            highScoreText.text = $"Best: {evt.HighScore}";
    }

    private void OnPauseClicked()
    {
        GameManager gm = GameServices.Get<GameManager>();
        gm?.PauseGame();
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<ScoreChangedEvent>(OnScoreChanged);
    }
}
```

- [ ] **Step 3: 验证编译**

在 Unity Editor 中确认 Console 无编译错误。

- [ ] **Step 4: 提交**

```bash
git add Assets/Scripts/UI/MainMenuView.cs Assets/Scripts/UI/MainMenuView.cs.meta Assets/Scripts/UI/GameHudView.cs Assets/Scripts/UI/GameHudView.cs.meta
git commit -m "feat: add MainMenuView and GameHudView"
```

---

### Task 9: 拆分 UIManager — 创建 View 类（PauseView + GameOverView）

**Files:**
- Create: `Assets/Scripts/UI/PauseView.cs`
- Create: `Assets/Scripts/UI/GameOverView.cs`

- [ ] **Step 1: 创建 PauseView.cs**

```csharp
// Assets/Scripts/UI/PauseView.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 暂停视图 — 继续/重启/主菜单/退出按钮
/// </summary>
public class PauseView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;

    private void Start()
    {
        if (resumeButton != null) resumeButton.onClick.AddListener(OnResumeClicked);
        if (restartButton != null) restartButton.onClick.AddListener(OnRestartClicked);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuitClicked);
    }

    public void Show() { gameObject.SetActive(true); }
    public void Hide() { gameObject.SetActive(false); }

    private void OnResumeClicked()
    {
        GameServices.Get<GameManager>()?.ResumeGame();
    }

    private void OnRestartClicked()
    {
        GameServices.Get<GameManager>()?.RestartGame();
    }

    private void OnMainMenuClicked()
    {
        GameServices.Get<GameManager>()?.GoToMainMenu();
    }

    private void OnQuitClicked()
    {
        GameServices.Get<GameManager>()?.QuitGame();
    }
}
```

- [ ] **Step 2: 创建 GameOverView.cs**

```csharp
// Assets/Scripts/UI/GameOverView.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 游戏结束视图 — 得分、最高分、新纪录提示、按钮
/// </summary>
public class GameOverView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private TextMeshProUGUI newRecordText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;

    private void Start()
    {
        if (restartButton != null) restartButton.onClick.AddListener(OnRestartClicked);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuitClicked);

        EventBus.Subscribe<ScoreChangedEvent>(OnScoreChanged);
    }

    public void Show()
    {
        gameObject.SetActive(true);
        ScoreManager sm = GameServices.Get<ScoreManager>();
        if (scoreText != null && sm != null)
            scoreText.text = $"Score: {sm.CurrentScore}";
        if (highScoreText != null && sm != null)
            highScoreText.text = $"Best: {sm.HighScore}";
        if (newRecordText != null && sm != null)
            newRecordText.gameObject.SetActive(sm.IsNewRecord);
    }

    public void Hide() { gameObject.SetActive(false); }

    private void OnScoreChanged(ScoreChangedEvent evt)
    {
        if (scoreText != null)
            scoreText.text = $"Score: {evt.Score}";
        if (highScoreText != null)
            highScoreText.text = $"Best: {evt.HighScore}";
        if (newRecordText != null)
            newRecordText.gameObject.SetActive(evt.IsNewRecord);
    }

    private void OnRestartClicked()
    {
        GameServices.Get<GameManager>()?.RestartGame();
    }

    private void OnMainMenuClicked()
    {
        GameServices.Get<GameManager>()?.GoToMainMenu();
    }

    private void OnQuitClicked()
    {
        GameServices.Get<GameManager>()?.QuitGame();
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<ScoreChangedEvent>(OnScoreChanged);
    }
}
```

- [ ] **Step 3: 验证编译**

在 Unity Editor 中确认 Console 无编译错误。

- [ ] **Step 4: 提交**

```bash
git add Assets/Scripts/UI/PauseView.cs Assets/Scripts/UI/PauseView.cs.meta Assets/Scripts/UI/GameOverView.cs Assets/Scripts/UI/GameOverView.cs.meta
git commit -m "feat: add PauseView and GameOverView"
```

---

### Task 10: 重构 UIManager — 路由面板切换

**Files:**
- Modify: `Assets/Scripts/UI/UIManager.cs`

- [ ] **Step 1: 重写 UIManager.cs**

用 View 路由替代原有的面板创建和管理逻辑：

```csharp
// Assets/Scripts/UI/UIManager.cs
using UnityEngine;

/// <summary>
/// UI 管理器 — 订阅 GameStateChangedEvent，路由面板切换
/// View 类的创建和 UI 布局由各自的 View 脚本和 Prefab 负责
/// </summary>
public class UIManager : MonoBehaviour
{
    [SerializeField] private MainMenuView mainMenuView;
    [SerializeField] private GameHudView gameHudView;
    [SerializeField] private PauseView pauseView;
    [SerializeField] private GameOverView gameOverView;

    private void Awake()
    {
        // 如果 View 未通过 Prefab 赋值，回退到代码创建
        if (mainMenuView == null || gameHudView == null || pauseView == null || gameOverView == null)
        {
            CreateFallbackUI();
        }

        EventBus.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
    }

    private void Start()
    {
        HideAll();
        mainMenuView?.Show();
    }

    private void OnGameStateChanged(GameStateChangedEvent evt)
    {
        HideAll();
        switch (evt.State)
        {
            case GameState.Ready:
                mainMenuView?.Show();
                break;
            case GameState.Playing:
                gameHudView?.Show();
                break;
            case GameState.Paused:
                pauseView?.Show();
                break;
            case GameState.GameOver:
                gameOverView?.Show();
                break;
        }
    }

    private void HideAll()
    {
        mainMenuView?.Hide();
        gameHudView?.Hide();
        pauseView?.Hide();
        gameOverView?.Hide();
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
    }

    /// <summary>
    /// 回退方案：当 View 未通过 Prefab 赋值时，代码创建 UI 面板
    /// </summary>
    private void CreateFallbackUI()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("Snake UI Canvas");
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObject.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        if (mainMenuView == null) mainMenuView = CreateViewOnCanvas<MainMenuView>(canvas.transform, "MainMenuPanel");
        if (gameHudView == null) gameHudView = CreateViewOnCanvas<GameHudView>(canvas.transform, "GameHudPanel");
        if (pauseView == null) pauseView = CreateViewOnCanvas<PauseView>(canvas.transform, "PausePanel");
        if (gameOverView == null) gameOverView = CreateViewOnCanvas<GameOverView>(canvas.transform, "GameOverPanel");
    }

    private T CreateViewOnCanvas<T>(Transform parent, string name) where T : MonoBehaviour
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rect = go.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        Image image = go.AddComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0f);
        return go.AddComponent<T>();
    }
}
```

- [ ] **Step 2: 删除 UIManager.cs 中原有的代码**

删除原有的：
- 所有 `[Header]` `[SerializeField]` UI 引用字段（它们现在在各自的 View 中）
- `RegisterButtonEvents()` — 按钮事件在 View 类中绑定
- `ShowMainMenu()`, `ShowGamePanel()`, `ShowPausePanel()`, `ShowGameOverPanel()`
- `SetAllPanelsInactive()`
- `EnsureDefaultUI()`, `CreateMainMenu()`, `CreateGameHud()`, `CreatePauseMenu()`, `CreateGameOverMenu()`
- `CreatePanel()`, `CreateButton()`, `CreateText()`, `SetRect()`
- `OnScoreChanged()`, `OnHighScoreChanged()` — 由 View 类自己处理
- 对 `scoreManager` 的直接引用

- [ ] **Step 3: 在 Unity Editor 中配置 View 引用**

在 UIManager 组件的 Inspector 中：
1. 将场景中已有的面板 GameObject 拖入对应字段
2. 或者删除场景中旧的面板，让 fallback 方案创建新的（运行后自动创建）

- [ ] **Step 4: 验证编译并运行**

在 Unity Editor 中 Play，验证所有 UI 面板正常切换（主菜单 → 游戏 → 暂停 → 游戏结束 → 重新开始）。

- [ ] **Step 5: 提交**

```bash
git add Assets/Scripts/UI/UIManager.cs
git commit -m "refactor: slim UIManager to view routing, delegate to View classes"
```

---

### Task 11: 重构 GameManager — 使用 EventBus + GameServices

**Files:**
- Modify: `Assets/Scripts/Managers/GameManager.cs`

- [ ] **Step 1: 重写 GameManager.cs**

```csharp
// Assets/Scripts/Managers/GameManager.cs
using UnityEngine;

/// <summary>
/// 游戏管理器 — 游戏状态机，通过 EventBus 广播状态变化
/// 通过 GameServices 获取依赖，不再使用 FindAnyObjectByType
/// </summary>
public class GameManager : MonoBehaviour
{
    private GameState currentState = GameState.Ready;

    public GameState CurrentState => currentState;

    private void Awake()
    {
        GameServices.Register(this);
    }

    private void Start()
    {
        EventBus.Subscribe<FoodEatenEvent>(OnFoodEaten);
        EventBus.Subscribe<SnakeDiedEvent>(OnSnakeDied);

        // 初始状态
        SetState(GameState.Ready);
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<FoodEatenEvent>(OnFoodEaten);
        EventBus.Unsubscribe<SnakeDiedEvent>(OnSnakeDied);
        GameServices.Unregister<GameManager>();
    }

    public void SetState(GameState newState)
    {
        currentState = newState;
        EventBus.Publish(new GameStateChangedEvent { State = currentState });

        switch (currentState)
        {
            case GameState.Ready:
            case GameState.Playing:
                Time.timeScale = 1f;
                break;
            case GameState.Paused:
            case GameState.GameOver:
                Time.timeScale = 0f;
                break;
        }
    }

    public void StartGame()
    {
        SnakeController snake = GameServices.Get<SnakeController>();
        FoodSpawner food = GameServices.Get<FoodSpawner>();
        ScoreManager score = GameServices.Get<ScoreManager>();

        score?.ResetScore();
        snake?.ResetSnake();
        snake?.StartMoving();
        food?.ResetFood();

        SetState(GameState.Playing);
    }

    public void PauseGame()
    {
        if (currentState == GameState.Playing)
            SetState(GameState.Paused);
    }

    public void ResumeGame()
    {
        if (currentState == GameState.Paused)
            SetState(GameState.Playing);
    }

    public void RestartGame()
    {
        StartGame();
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SnakeController snake = GameServices.Get<SnakeController>();
        snake?.ResetSnake();
        SetState(GameState.Ready);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnFoodEaten(FoodEatenEvent evt)
    {
        ScoreManager score = GameServices.Get<ScoreManager>();
        score?.AddScore(1);
    }

    private void OnSnakeDied(SnakeDiedEvent evt)
    {
        SetState(GameState.GameOver);
    }
}
```

- [ ] **Step 2: 删除 GameManager.cs 中原有的代码**

删除：
- `mainMenuSceneName`, `gameSceneName` 字段
- 所有 `FindAnyObjectByType` 获取的引用字段
- `OnSnakeMoved()` 方法（食物检测逻辑移到 FoodSpawner 或通过事件处理）
- `SceneExists()` 和 `sceneNameSet` 
- `LoadGameScene()` 方法
- 所有 Debug.Log 调试输出

**重要**：由于删除了 `OnSnakeMoved` 回调订阅，需要在 SnakeController 中调整食物检测逻辑。现在食物检测通过以下方式工作：`FoodSpawner` 在 `Update` 中自行检测蛇头位置是否与食物位置重叠（或用另一种方式）。

替代方案：让 `FoodSpawner` 在 `Update` 中自己检查蛇头位置：

在 `FoodSpawner.cs` 的 `Update()` 中添加：
```csharp
private void Update()
{
    if (currentFood == null || foodSegment == null) return;
    SnakeController snake = GameServices.Get<SnakeController>();
    if (snake == null) return;
    
    if (snake.HeadGridPosition == foodSegment.GridPosition)
    {
        EatFood();
    }
}
```

并移除 GameManager 中的 `OnSnakeMoved` 订阅。

- [ ] **Step 3: 验证编译并运行**

在 Unity Editor 中 Play，验证完整游戏流程可用。

- [ ] **Step 4: 提交**

```bash
git add Assets/Scripts/Managers/GameManager.cs Assets/Scripts/Food/FoodSpawner.cs
git commit -m "refactor: GameManager uses EventBus+GameServices, food detection moved to FoodSpawner"
```

---

### Task 12: 迁移 InputManager 和 ScoreManager

**Files:**
- Modify: `Assets/Scripts/Managers/InputManager.cs`
- Modify: `Assets/Scripts/Managers/ScoreManager.cs`

- [ ] **Step 1: 修改 InputManager.cs — 使用 GameServices**

```csharp
// Assets/Scripts/Managers/InputManager.cs
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

    private void Awake()
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
        if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame)
            return;

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
```

改动：`FindAnyObjectByType` → `GameServices.Get<T>()`，移除 `hasTouchscreen` 变量（Unity Input System 在无设备时返回 null）。

- [ ] **Step 2: 修改 ScoreManager.cs — 通过 EventBus 发布事件**

```csharp
// Assets/Scripts/Managers/ScoreManager.cs
using UnityEngine;

/// <summary>
/// 计分管理器 — 管理得分和最高分，通过 EventBus 发布分数变化
/// </summary>
public class ScoreManager : MonoBehaviour
{
    private const string HIGH_SCORE_KEY = "SnakeGame_HighScore";

    private int currentScore;
    private int highScore;

    public int CurrentScore => currentScore;
    public int HighScore => highScore;
    public bool IsNewRecord { get; private set; }

    private void Awake()
    {
        LoadHighScore();
        GameServices.Register(this);
    }

    public void AddScore(int points = 1)
    {
        currentScore += points;
        IsNewRecord = false;

        if (currentScore > highScore)
        {
            highScore = currentScore;
            IsNewRecord = true;
            SaveHighScore();
        }

        EventBus.Publish(new ScoreChangedEvent
        {
            Score = currentScore,
            HighScore = highScore,
            IsNewRecord = IsNewRecord
        });
    }

    public void ResetScore()
    {
        currentScore = 0;
        IsNewRecord = false;
        LoadHighScore();
        EventBus.Publish(new ScoreChangedEvent
        {
            Score = currentScore,
            HighScore = highScore,
            IsNewRecord = false
        });
    }

    private void LoadHighScore()
    {
        highScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
    }

    private void SaveHighScore()
    {
        PlayerPrefs.SetInt(HIGH_SCORE_KEY, highScore);
        PlayerPrefs.Save();
    }

    private void OnDestroy()
    {
        GameServices.Unregister<ScoreManager>();
    }
}
```

改动：删除 `OnScoreChanged` 和 `OnHighScoreChanged` 的 `System.Action` 事件，改用 `EventBus.Publish<ScoreChangedEvent>()`，在 Awake/OnDestroy 中注册/注销 GameServices。

- [ ] **Step 3: 验证编译并运行**

在 Unity Editor 中 Play，验证键盘/触控输入正常，分数显示正常。

- [ ] **Step 4: 提交**

```bash
git add Assets/Scripts/Managers/InputManager.cs Assets/Scripts/Managers/ScoreManager.cs
git commit -m "refactor: InputManager and ScoreManager use GameServices+EventBus"
```

---

### Task 13: 更新 Bootstrap — 注册服务 + 注入配置

**Files:**
- Modify: `Assets/Scripts/Runtime/SnakeGameBootstrap.cs`

- [ ] **Step 1: 重写 SnakeGameBootstrap.cs**

```csharp
// Assets/Scripts/Runtime/SnakeGameBootstrap.cs
using UnityEngine;

public static class SnakeGameBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        EnsureCamera();

        GameObject root = GameObject.Find("Snake Game Runtime");
        if (root == null)
            root = new GameObject("Snake Game Runtime");

        // 加载配置
        GameConfig config = Resources.Load<GameConfig>("Config/GameConfig");
        if (config == null)
            config = ScriptableObject.CreateInstance<GameConfig>();

        // 确保组件存在并按依赖顺序添加
        GridManager gridManager = EnsureComponent<GridManager>(root);
        InjectConfig(gridManager, config);

        SnakeController snakeController = EnsureComponent<SnakeController>(root);
        InjectConfig(snakeController, config);

        FoodSpawner foodSpawner = EnsureComponent<FoodSpawner>(root);
        InjectConfig(foodSpawner, config);

        ScoreManager scoreManager = EnsureComponent<ScoreManager>(root);
        GameManager gameManager = EnsureComponent<GameManager>(root);
        InputManager inputManager = EnsureComponent<InputManager>(root);
        UIManager uiManager = EnsureComponent<UIManager>(root);

        // 确保 GridManager 的渲染组件
        if (root.GetComponent<GridBackgroundRenderer>() == null)
            root.AddComponent<GridBackgroundRenderer>();
        if (root.GetComponent<GridWallRenderer>() == null)
            root.AddComponent<GridWallRenderer>();
    }

    private static void InjectConfig<T>(T component, GameConfig config) where T : Component
    {
        // 通过反射设置 config 字段（如果组件有此字段）
        var field = typeof(T).GetField("config",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Public);
        if (field != null && field.FieldType == typeof(GameConfig))
            field.SetValue(component, config);
    }

    private static void EnsureCamera()
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            camera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
        }

        camera.orthographic = true;
        camera.orthographicSize = 12.5f;
        camera.transform.position = new Vector3(0f, 0f, -10f);
        camera.backgroundColor = new Color(0.09f, 0.13f, 0.16f);
        camera.clearFlags = CameraClearFlags.SolidColor;
    }

    private static T EnsureComponent<T>(GameObject root) where T : Component
    {
        T existing = Object.FindAnyObjectByType<T>();
        if (existing != null)
            return existing;
        return root.AddComponent<T>();
    }
}
```

- [ ] **Step 2: 验证编译**

在 Unity Editor 中确认 Console 无编译错误。注意 `InjectConfig` 使用反射 — 确认 `SnakeController` 和 `GridManager` 都有 `[SerializeField] private GameConfig config;` 字段。

- [ ] **Step 3: 运行验证并提交**

在 Unity Editor 中 Play，确认游戏从头到尾正常运行。

```bash
git add Assets/Scripts/Runtime/SnakeGameBootstrap.cs
git commit -m "refactor: bootstrap registers services and injects GameConfig"
```

---

### Task 14: 最终清理与验证

**Files:**
- Modify: 所有改动文件

- [ ] **Step 1: 清理废弃代码**

确认以下内容已被移除：
- 所有 `FindAnyObjectByType` 调用（除 Bootstrap 和 UIManager 的 fallback）
- 所有未使用的 `using` 语句
- 所有 Debug.Log 调试输出
- 废弃的 `System.Action` 事件字段（`OnFoodEaten`, `OnDeath`, `OnGameStateChanged`, `OnScoreChanged`, `OnHighScoreChanged`）
- `SceneExists()` 方法

- [ ] **Step 2: 全流程回归测试**

在 Unity Editor Play 模式下验证：

1. **启动** → 显示主菜单，最高分正确
2. **开始游戏** → 游戏 HUD 显示，分数为 0
3. **方向控制** → WASD / 方向键控制蛇移动
4. **吃食物** → 蛇身增长，分数 +1，新食物生成
5. **撞墙** → 游戏结束面板，显示最终分数
6. **撞自身** → 同上
7. **暂停/继续** → 按 Esc 暂停，再按继续
8. **重新开始** → 从 GameOver 面板重新开始
9. **最高分** → 超过最高分时显示新纪录
10. **退出** → 在 Editor 中停止 Play Mode

- [ ] **Step 3: 提交**

```bash
git add -A
git commit -m "chore: final cleanup, remove dead code and debug logs"
```

---

## 实施总结

| 任务 | 内容 | 新建文件 | 修改文件 |
|------|------|----------|----------|
| 1 | EventTypes | EventTypes.cs | - |
| 2 | EventBus | EventBus.cs | - |
| 3 | GameServices | GameServices.cs | - |
| 4 | GameConfig | GameConfig.cs | - |
| 5 | GridBackgroundRenderer | GridBackgroundRenderer.cs | GridManager.cs |
| 6 | GridWallRenderer | GridWallRenderer.cs | GridManager.cs |
| 7 | SnakeMovement | SnakeMovement.cs | SnakeController.cs |
| 8 | MainMenuView + GameHudView | 2 files | - |
| 9 | PauseView + GameOverView | 2 files | - |
| 10 | UIManager slim | - | UIManager.cs |
| 11 | GameManager 重构 | - | GameManager.cs, FoodSpawner.cs |
| 12 | InputManager + ScoreManager | - | InputManager.cs, ScoreManager.cs |
| 13 | Bootstrap 更新 | - | SnakeGameBootstrap.cs |
| 14 | 清理验证 | - | 所有文件 |

**新建 10 个文件，修改 7 个已有文件，SnakeSegment.cs 和 SnakeSpriteLoader.cs 不变。**
