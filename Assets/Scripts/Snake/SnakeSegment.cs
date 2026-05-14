using UnityEngine;

/// <summary>
/// 蛇节段组件 - 挂在蛇头和蛇身的每个节段上
/// </summary>
public class SnakeSegment : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    private Vector2Int gridPosition;

    public Vector2Int GridPosition
    {
        get => gridPosition;
        set => gridPosition = value;
    }

    public void SetSprite(Sprite sprite)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = sprite;
        }
    }

    public void SetColor(Color color)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }
    }

    public void SetSortingOrder(int order)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = order;
        }
    }

    public void FitToCell(float cellSize, float padding = 0.92f)
    {
        if (spriteRenderer == null || spriteRenderer.sprite == null)
            return;

        Vector2 size = spriteRenderer.sprite.bounds.size;
        float largestSide = Mathf.Max(size.x, size.y);
        if (largestSide <= 0f)
            return;

        float scale = cellSize * padding / largestSide;
        transform.localScale = new Vector3(scale, scale, 1f);
    }

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }
}
