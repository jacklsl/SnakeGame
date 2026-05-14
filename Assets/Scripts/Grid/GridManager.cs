using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 网格地图管理器 - 管理网格数据和坐标转换
/// 渲染逻辑委托给 GridBackgroundRenderer 和 GridWallRenderer
/// </summary>
public class GridManager : MonoBehaviour
{
    [Header("网格设置")]
    [SerializeField] private int gridWidth = 20;
    [SerializeField] private int gridHeight = 20;
    [SerializeField] private float cellSize = 1f;

    private Vector2 originPosition;
    private bool[,] occupiedCells;
    private List<Vector2Int> emptyCellsBuffer = new List<Vector2Int>();

    public int GridWidth => gridWidth;
    public int GridHeight => gridHeight;
    public float CellSize => cellSize;

    private void Awake()
    {
        GameServices.Register(this);
        occupiedCells = new bool[gridWidth, gridHeight];
        originPosition = new Vector2(-gridWidth * cellSize / 2f, -gridHeight * cellSize / 2f);
    }

    private void OnDestroy()
    {
        GameServices.Unregister<GridManager>();
    }

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

    public Vector3 GridToWorldPosition(int gridX, int gridY)
    {
        float worldX = originPosition.x + (gridX + 0.5f) * cellSize;
        float worldY = originPosition.y + (gridY + 0.5f) * cellSize;
        return new Vector3(worldX, worldY, 0);
    }

    public Vector2Int WorldToGridPosition(Vector3 worldPos)
    {
        int gridX = Mathf.FloorToInt((worldPos.x - originPosition.x) / cellSize);
        int gridY = Mathf.FloorToInt((worldPos.y - originPosition.y) / cellSize);
        return new Vector2Int(gridX, gridY);
    }

    public bool IsInBounds(int gridX, int gridY)
    {
        return gridX >= 0 && gridX < gridWidth && gridY >= 0 && gridY < gridHeight;
    }

    public void SetCellOccupied(int gridX, int gridY, bool occupied)
    {
        if (IsInBounds(gridX, gridY))
            occupiedCells[gridX, gridY] = occupied;
    }

    public bool IsCellOccupied(int gridX, int gridY)
    {
        if (!IsInBounds(gridX, gridY))
            return true;
        return occupiedCells[gridX, gridY];
    }

    public List<Vector2Int> GetEmptyCells()
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
        Gizmos.color = Color.green;
        Vector3 center = new Vector3(0, 0, 0);
        Vector3 size = new Vector3(gridWidth * cellSize, gridHeight * cellSize, 0);
        Gizmos.DrawWireCube(center, size);
    }
}
