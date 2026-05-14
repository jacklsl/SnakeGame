using UnityEngine;

/// <summary>
/// 网格地图管理器 - 管理游戏网格系统
/// </summary>
public class GridManager : MonoBehaviour
{
    [Header("网格设置")]
    [SerializeField] private int gridWidth = 20;
    [SerializeField] private int gridHeight = 20;
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject backgroundTilePrefab;

    private Vector2 originPosition;
    private bool[,] occupiedCells;
    private System.Collections.Generic.List<Vector2Int> emptyCellsBuffer = new System.Collections.Generic.List<Vector2Int>();

    public int GridWidth => gridWidth;
    public int GridHeight => gridHeight;
    public float CellSize => cellSize;

    private void Awake()
    {
        occupiedCells = new bool[gridWidth, gridHeight];
        // 计算原点位置使网格居中
        originPosition = new Vector2(-gridWidth * cellSize / 2f, -gridHeight * cellSize / 2f);
        EnsureDefaults();
    }

    private void Start()
    {
        GenerateBackground();
        GenerateWalls();
    }

    /// <summary>
    /// 生成背景草地（棋盘格风格，使用单个 SpriteRenderer 减少 Draw Call）
    /// </summary>
    private void GenerateBackground()
    {
        // 计算纹理大小：每个格子 cellSize 像素
        int texWidth = gridWidth;
        int texHeight = gridHeight;
        Texture2D bgTexture = new Texture2D(texWidth, texHeight, TextureFormat.RGBA32, false);
        bgTexture.filterMode = FilterMode.Point;
        bgTexture.wrapMode = TextureWrapMode.Clamp;

        // 绘制棋盘格
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                bool isLight = (x + y) % 2 == 0;
                Color color = isLight
                    ? new Color(0.45f, 0.78f, 0.30f)
                    : new Color(0.22f, 0.55f, 0.15f);
                bgTexture.SetPixel(x, y, color);
            }
        }
        bgTexture.Apply();

        // 创建单个 Sprite 覆盖整个网格
        // 纹理每个像素代表一个格子，pixelsPerUnit 设为 1/cellSize
        // 这样纹理的 1 像素 = cellSize 世界单位，整个纹理正好覆盖网格区域
        Vector2 pivot = new Vector2(0.5f, 0.5f);
        Sprite bgSprite = Sprite.Create(bgTexture, new Rect(0, 0, texWidth, texHeight), pivot, 1f / cellSize);

        GameObject bgObject = new GameObject("Background");
        bgObject.transform.SetParent(transform);
        // 背景居中于网格原点
        bgObject.transform.position = new Vector3(0, 0, 0);
        bgObject.transform.localScale = Vector3.one;

        SpriteRenderer renderer = bgObject.AddComponent<SpriteRenderer>();
        renderer.sprite = bgSprite;
        renderer.sortingOrder = -5;
    }

    /// <summary>
    /// 生成墙壁
    /// </summary>
    private void GenerateWalls()
    {
        if (wallPrefab == null) return;

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
        Vector3 pos = GridToWorldPosition(gridX, gridY);
        GameObject wall = Instantiate(wallPrefab, pos, Quaternion.identity, transform);
        wall.name = $"Wall_{gridX}_{gridY}";
        FitSpriteToCell(wall, 1f);
    }

    /// <summary>
    /// 网格坐标转世界坐标
    /// </summary>
    public Vector3 GridToWorldPosition(int gridX, int gridY)
    {
        float worldX = originPosition.x + (gridX + 0.5f) * cellSize;
        float worldY = originPosition.y + (gridY + 0.5f) * cellSize;
        return new Vector3(worldX, worldY, 0);
    }

    /// <summary>
    /// 世界坐标转网格坐标
    /// </summary>
    public Vector2Int WorldToGridPosition(Vector3 worldPos)
    {
        int gridX = Mathf.FloorToInt((worldPos.x - originPosition.x) / cellSize);
        int gridY = Mathf.FloorToInt((worldPos.y - originPosition.y) / cellSize);
        return new Vector2Int(gridX, gridY);
    }

    /// <summary>
    /// 检查网格坐标是否在地图范围内
    /// </summary>
    public bool IsInBounds(int gridX, int gridY)
    {
        return gridX >= 0 && gridX < gridWidth && gridY >= 0 && gridY < gridHeight;
    }

    /// <summary>
    /// 设置单元格占用状态
    /// </summary>
    public void SetCellOccupied(int gridX, int gridY, bool occupied)
    {
        if (IsInBounds(gridX, gridY))
        {
            occupiedCells[gridX, gridY] = occupied;
        }
    }

    /// <summary>
    /// 检查单元格是否被占用
    /// </summary>
    public bool IsCellOccupied(int gridX, int gridY)
    {
        if (!IsInBounds(gridX, gridY))
            return true; // 边界外视为占用
        return occupiedCells[gridX, gridY];
    }

    /// <summary>
    /// 获取所有空白（未占用）的网格位置
    /// </summary>
    public System.Collections.Generic.List<Vector2Int> GetEmptyCells()
    {
        emptyCellsBuffer.Clear();
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (!occupiedCells[x, y])
                    emptyCellsBuffer.Add(new Vector2Int(x, y));
            }
        }
        return emptyCellsBuffer;
    }

    /// <summary>
    /// 清除所有占用标记
    /// </summary>
    public void ClearOccupiedCells()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                occupiedCells[x, y] = false;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // 在编辑器中绘制网格边界
        Gizmos.color = Color.green;
        Vector3 center = new Vector3(0, 0, 0);
        Vector3 size = new Vector3(gridWidth * cellSize, gridHeight * cellSize, 0);
        Gizmos.DrawWireCube(center, size);
    }

    private void EnsureDefaults()
    {
        if (backgroundTilePrefab == null)
        {
            // 优先使用已生成的草地背景图片，否则使用纯色背景
            Sprite bgSprite = SnakeSpriteLoader.LoadSprite("Assets/Sprites/Generated/Background.png");
            if (bgSprite == null)
            {
                bgSprite = SnakeSpriteLoader.CreateSolidSprite(new Color(0.12f, 0.19f, 0.15f));
            }
            backgroundTilePrefab = CreateSpritePrefab("Background Tile Prefab", bgSprite, -5);
        }

        if (wallPrefab == null)
        {
            wallPrefab = CreateSpritePrefab(
                "Wall Prefab",
                SnakeSpriteLoader.LoadSprite("Assets/snakesprites/png/wall_block_64_0.png"),
                2);
        }
    }

    private GameObject CreateSpritePrefab(string prefabName, Sprite sprite, int sortingOrder)
    {
        GameObject prefab = new GameObject(prefabName);
        prefab.SetActive(false);
        SpriteRenderer renderer = prefab.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = sortingOrder;
        return prefab;
    }

    private void FitSpriteToCell(GameObject obj, float padding)
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
