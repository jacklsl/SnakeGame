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
