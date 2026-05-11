using UnityEngine;

/// <summary>
/// 食物生成器 - 管理食物的生成逻辑
/// </summary>
public class FoodSpawner : MonoBehaviour
{
    [Header("预制体")]
    [SerializeField] private GameObject foodPrefab;

    [Header("精灵")]
    [SerializeField] private Sprite foodSprite;

    private GridManager gridManager;
    private SnakeController snakeController;
    private GameObject currentFood;
    private SnakeSegment foodSegment;

    private void Awake()
    {
        gridManager = FindObjectOfType<GridManager>();
        snakeController = FindObjectOfType<SnakeController>();
        EnsureDefaults();
    }

    private void Start()
    {
        // 如果没有 GameManager（例如在测试场景中），延迟一帧生成食物
        // 确保 SnakeController 已经初始化
        if (FindObjectOfType<GameManager>() == null)
            Invoke(nameof(SpawnFood), 0f);
    }

    /// <summary>
    /// 在随机空白位置生成食物
    /// </summary>
    public void SpawnFood()
    {
        if (foodPrefab == null) return;

        // 如果已有食物，先销毁并清除占用标记
        if (currentFood != null)
        {
            ClearFoodOccupied();
            Destroy(currentFood);
            currentFood = null;
            foodSegment = null;
        }

        // 获取所有空白位置（此时蛇身占用已更新，旧食物占用已清除）
        Vector2Int[] emptyCells = gridManager.GetEmptyCells();

        if (emptyCells.Length == 0)
        {
            Debug.LogWarning("没有空白位置可以生成食物！");
            return;
        }

        // 随机选择一个空白位置
        Vector2Int randomPos = emptyCells[Random.Range(0, emptyCells.Length)];

        // 生成新食物
        Vector3 worldPos = gridManager.GridToWorldPosition(randomPos.x, randomPos.y);
        currentFood = Instantiate(foodPrefab, worldPos, Quaternion.identity);
        currentFood.name = "Food";
        currentFood.SetActive(true);

        // 设置精灵
        foodSegment = currentFood.GetComponent<SnakeSegment>();
        if (foodSegment != null)
        {
            foodSegment.GridPosition = randomPos;
            foodSegment.SetSprite(foodSprite);
            foodSegment.SetSortingOrder(5);
            foodSegment.FitToCell(gridManager.CellSize, 0.82f);
        }

        // 标记食物位置为占用
        gridManager.SetCellOccupied(randomPos.x, randomPos.y, true);
    }

    /// <summary>
    /// 清除食物占用的网格标记
    /// </summary>
    private void ClearFoodOccupied()
    {
        if (foodSegment != null && gridManager != null)
        {
            gridManager.SetCellOccupied(foodSegment.GridPosition.x, foodSegment.GridPosition.y, false);
        }
    }

    /// <summary>
    /// 检查蛇头是否吃到食物
    /// </summary>
    public bool CheckFoodCollision()
    {
        if (currentFood == null || snakeController == null)
            return false;

        Vector2Int headPos = snakeController.HeadGridPosition;
        Vector2Int foodPos = foodSegment.GridPosition;

        return headPos == foodPos;
    }

    public bool IsFoodAt(Vector2Int gridPosition)
    {
        return currentFood != null && foodSegment != null && foodSegment.GridPosition == gridPosition;
    }

    /// <summary>
    /// 处理吃食物逻辑
    /// </summary>
    public void EatFood()
    {
        // 先让蛇生长（更新蛇身占用标记），再清除食物
        // 这样食物位置在生长期间仍被标记为占用，避免新食物生成在相同位置
        snakeController.Grow();

        if (currentFood != null)
        {
            // 清除食物占用的格子
            ClearFoodOccupied();
            Destroy(currentFood);
            currentFood = null;
            foodSegment = null;
        }

        // 生成新食物（此时蛇身占用已更新，确保食物不会生成在蛇身上）
        SpawnFood();
    }

    /// <summary>
    /// 重置食物
    /// </summary>
    public void ResetFood()
    {
        if (currentFood != null)
        {
            ClearFoodOccupied();
            Destroy(currentFood);
            currentFood = null;
            foodSegment = null;
        }
        SpawnFood();
    }

    private void EnsureDefaults()
    {
        if (foodSprite == null)
            foodSprite = SnakeSpriteLoader.LoadSprite("Assets/snakesprites/png/apple_red_64.png");

        if (foodPrefab == null)
        {
            foodPrefab = new GameObject("Food Prefab");
            foodPrefab.SetActive(false);
            SpriteRenderer renderer = foodPrefab.AddComponent<SpriteRenderer>();
            renderer.sprite = foodSprite;
            foodPrefab.AddComponent<SnakeSegment>();
        }
    }
}
