using UnityEngine;

/// <summary>
/// 食物生成器 — 管理食物的生成和碰撞检测
/// </summary>
public class FoodSpawner : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject foodPrefab;

    [Header("Config")]
    [SerializeField] private GameConfig config;

    [Header("Sprite")]
    [SerializeField] private Sprite foodSprite;

    private GridManager gridManager;
    private SnakeController snakeController;
    private GameObject currentFood;
    private SnakeSegment foodSegment;

    private void Awake()
    {
        GameServices.Register(this);
    }

    private void Start()
    {
        EnsureDefaults();
        gridManager = GameServices.Get<GridManager>();
        snakeController = GameServices.Get<SnakeController>();
        if (GameServices.Get<GameManager>() == null)
            Invoke(nameof(SpawnFood), 0f);
    }

    private void Update()
    {
        if (currentFood == null || foodSegment == null) return;
        if (snakeController == null || snakeController.IsDead) return;

        if (snakeController.HeadGridPosition == foodSegment.GridPosition)
            EatFood();
    }

    public void SpawnFood()
    {
        if (foodPrefab == null) return;

        if (currentFood != null)
        {
            ClearFoodOccupied();
            Destroy(currentFood);
            currentFood = null;
            foodSegment = null;
        }

        var emptyCells = gridManager.GetEmptyCells();
        if (emptyCells.Count == 0) return;

        Vector2Int randomPos = emptyCells[Random.Range(0, emptyCells.Count)];
        Vector3 worldPos = gridManager.GridToWorldPosition(randomPos.x, randomPos.y);
        currentFood = Instantiate(foodPrefab, worldPos, Quaternion.identity);
        currentFood.name = "Food";
        currentFood.SetActive(true);

        foodSegment = currentFood.GetComponent<SnakeSegment>();
        if (foodSegment != null)
        {
            foodSegment.GridPosition = randomPos;
            foodSegment.SetSprite(foodSprite);
            foodSegment.SetSortingOrder(5);
            foodSegment.FitToCell(gridManager.CellSize, 0.82f);
        }

        gridManager.SetCellOccupied(randomPos.x, randomPos.y, true);
    }

    private void ClearFoodOccupied()
    {
        if (foodSegment != null && gridManager != null)
            gridManager.SetCellOccupied(foodSegment.GridPosition.x, foodSegment.GridPosition.y, false);
    }

    public bool IsFoodAt(Vector2Int gridPosition)
    {
        return currentFood != null && foodSegment != null && foodSegment.GridPosition == gridPosition;
    }

    public void EatFood()
    {
        if (currentFood != null)
        {
            Destroy(currentFood);
            currentFood = null;
            foodSegment = null;
        }

        snakeController.Grow();
        SpawnFood();
    }

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
            foodSprite = SnakeSpriteLoader.LoadSprite(config != null ? config.FoodSpritePath : "Assets/snakesprites/png/apple_red_64.png");

        if (foodPrefab == null)
        {
            foodPrefab = new GameObject("Food Prefab");
            foodPrefab.SetActive(false);
            SpriteRenderer renderer = foodPrefab.AddComponent<SpriteRenderer>();
            renderer.sprite = foodSprite;
            foodPrefab.AddComponent<SnakeSegment>();
        }
    }

    private void OnDestroy()
    {
        GameServices.Unregister<FoodSpawner>();
    }
}
