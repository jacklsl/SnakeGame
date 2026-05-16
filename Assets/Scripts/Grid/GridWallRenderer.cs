using UnityEngine;

/// <summary>
/// 墙壁渲染器 — 负责墙壁对象的生成和缩放
/// </summary>
public class GridWallRenderer : MonoBehaviour
{
    private int gridWidth;
    private int gridHeight;
    private float cellSize;
    private GameObject wallPrefab;

    public void Generate(int width, int height, float cell)
    {
        gridWidth = width;
        gridHeight = height;
        cellSize = cell;
        EnsurePrefab();

        for (int x = -1; x <= gridWidth; x++)
        {
            SpawnWall(x, -1);
            SpawnWall(x, gridHeight);
        }

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
        if (renderer == null || renderer.sprite == null) return;

        Vector2 size = renderer.sprite.bounds.size;
        float largestSide = Mathf.Max(size.x, size.y);
        if (largestSide <= 0f) return;

        float scale = cellSize * 1f / largestSide;
        obj.transform.localScale = new Vector3(scale, scale, 1f);
    }

    private void EnsurePrefab()
    {
        if (wallPrefab != null) return;

        wallPrefab = new GameObject("Wall Prefab");
        wallPrefab.SetActive(false);
        SpriteRenderer renderer = wallPrefab.AddComponent<SpriteRenderer>();
        GameConfig config = GameServices.Get<GameConfig>();
        string wallSpritePath = config != null ? config.WallSpritePath : "Assets/snakesprites/png/wall_block_64_0.png";
        renderer.sprite = SnakeSpriteLoader.LoadSprite(wallSpritePath);
        renderer.sortingOrder = 2;
    }
}
