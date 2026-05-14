# SnakeGame 代码质量重构 — 设计文档

## 1. 目标

- 解耦：用 Service Locator + EventBus 替代 `FindAnyObjectByType` 和散落 `System.Action`
- 单一职责：拆分大类和职责混杂的文件
- 可测试性：接口注入、逻辑与 Unity API 分离
- 配置集中：引入 ScriptableObject 管理所有魔法数字和资源路径

## 2. 核心架构变更

### 2.1 GameServices — 轻量服务定位器

```csharp
public static class GameServices
{
    private static Dictionary<Type, object> services = new();
    public static void Register<T>(T service) where T : class;
    public static void Unregister<T>() where T : class;
    public static T Get<T>() where T : class;
}
```

- Bootstrap 在 Awake 中注册，OnDestroy 中注销
- 替代所有 `FindAnyObjectByType<T>()`

### 2.2 EventBus — 集中事件系统

```csharp
public static class EventBus
{
    public static void Subscribe<T>(Action<T> handler) where T : struct;
    public static void Unsubscribe<T>(Action<T> handler) where T : struct;
    public static void Publish<T>(T evt) where T : struct;
}
```

- 使用 struct 事件，零 GC 分配
- 事件类型：`FoodEatenEvent`, `SnakeDiedEvent`, `ScoreChangedEvent`, `GameStateChangedEvent`

### 2.3 GameConfig — ScriptableObject 配置

```csharp
[CreateAssetMenu]
public class GameConfig : ScriptableObject
{
    // 网格
    public int GridWidth = 20;
    public int GridHeight = 20;
    public float CellSize = 1f;

    // 移动
    public float BaseMoveInterval = 0.2f;
    public float MinMoveInterval = 0.05f;
    public int SpeedUpInterval = 5;
    public float SpeedUpAmount = 0.02f;

    // 精灵路径
    public string HeadSpritePath;
    public string BodySpritePath;
    public string FoodSpritePath;
    public string WallSpritePath;

    // 背景颜色
    public Color BackgroundColorLight;
    public Color BackgroundColorDark;
}
```

## 3. 文件拆分详案

### 3.1 SnakeController → SnakeController + SnakeMovement

| 文件 | 职责 |
|------|------|
| `SnakeMovement.cs` | 移动计时、方向缓冲、位置计算、边界/自身碰撞检测 |
| `SnakeController.cs` | 蛇的初始化、生长、重置、精灵设置 |

SnakeController 持有 SnakeMovement，移动逻辑与蛇数据分离。

### 3.2 GridManager → GridManager + GridBackgroundRenderer + GridWallRenderer

| 文件 | 职责 |
|------|------|
| `GridManager.cs` | 网格数据：occupiedCells、坐标转换、空白格子查询 |
| `GridBackgroundRenderer.cs` | 棋盘格背景 Texture2D 生成和 Sprite 渲染 |
| `GridWallRenderer.cs` | 墙壁 Sprite 生成和位置计算 |

### 3.3 UIManager → UIManager + 4 个 View 类

| 文件 | 职责 |
|------|------|
| `UIManager.cs` | 订阅 GameStateChangedEvent，路由面板切换 |
| `MainMenuView.cs` | 主菜单面板：标题、最高分、开始按钮 |
| `GameHudView.cs` | 游戏 HUD：得分、最高分、暂停按钮 |
| `PauseView.cs` | 暂停面板：继续/重启/主菜单/退出 |
| `GameOverView.cs` | 结算面板：得分、最高分、新纪录、按钮 |

所有 View 通过 `[SerializeField]` 引用 Prefab，保留代码创建 fallback。

### 3.4 Bootstrap 简化

`SnakeGameBootstrap.cs` 职责收窄：
- 确保 Camera 和 Runtime 根节点存在
- EnsureComponent 注册所有实例
- 加载 GameConfig 并注入到需要的组件

### 3.5 GameManager 瘦身

- 状态机逻辑保留，但状态切换通过 EventBus 广播
- 移除对 FoodSpawner/SnakeController/ScoreManager 的直接引用
- 通过 GameServices 获取依赖

## 4. 目录结构

```
Assets/
├── Config/
│   └── GameConfig.asset
├── Prefabs/UI/
│   ├── MainMenuPanel.prefab
│   ├── GameHudPanel.prefab
│   ├── PausePanel.prefab
│   └── GameOverPanel.prefab
├── Scripts/
│   ├── Core/
│   │   ├── GameServices.cs
│   │   ├── EventBus.cs
│   │   └── GameConfig.cs
│   ├── Managers/
│   │   ├── GameManager.cs
│   │   ├── InputManager.cs
│   │   └── ScoreManager.cs
│   ├── Snake/
│   │   ├── SnakeController.cs
│   │   ├── SnakeMovement.cs
│   │   └── SnakeSegment.cs
│   ├── Grid/
│   │   ├── GridManager.cs
│   │   ├── GridBackgroundRenderer.cs
│   │   └── GridWallRenderer.cs
│   ├── Food/
│   │   └── FoodSpawner.cs
│   ├── UI/
│   │   ├── UIManager.cs
│   │   ├── MainMenuView.cs
│   │   ├── GameHudView.cs
│   │   ├── PauseView.cs
│   │   └── GameOverView.cs
│   └── Runtime/
│       ├── SnakeGameBootstrap.cs
│       └── SnakeSpriteLoader.cs
```

## 5. 实施顺序

1. 创建 `GameServices` + `EventBus` + `GameConfig` 基础设施
2. 拆分 `GridManager` → GridBackgroundRenderer + GridWallRenderer
3. 拆分 `SnakeController` → SnakeMovement
4. 拆分 `UIManager` → View 类 + Prefab
5. 重构 `GameManager` 使用 EventBus + GameServices
6. 迁移 `InputManager`、`ScoreManager`、`FoodSpawner` 到新依赖方式
7. 更新 `Bootstrap`：注册服务 + 注入配置
8. 清理废弃代码，确认运行正常

每步完成后在 Editor 中验证路径。

## 6. 不变的部分

- `SnakeSegment` — 已经是单一职责的轻量组件
- `SnakeSpriteLoader` — 静态工具类，独立于架构
- `ScoreManager` — 逻辑清晰，只需将事件改为 EventBus 发布
- `InputManager` — 只需引入 GameServices
