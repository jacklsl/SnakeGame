using UnityEngine;

/// <summary>
/// 棋盘格背景渲染器 — 负责生成和渲染网格背景
/// </summary>
public class GridBackgroundRenderer : MonoBehaviour
{
    public void Generate(int gridWidth, int gridHeight, float cellSize)
    {
        GameConfig config = GameServices.Get<GameConfig>();

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

        GameObject bgObject = new GameObject("Background");
        bgObject.transform.SetParent(transform);
        bgObject.transform.position = Vector3.zero;

        SpriteRenderer renderer = bgObject.AddComponent<SpriteRenderer>();
        renderer.sprite = bgSprite;
        renderer.sortingOrder = -5;
    }
}
