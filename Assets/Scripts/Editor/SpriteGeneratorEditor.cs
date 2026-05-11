using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// 编辑器工具：为贪吃蛇游戏生成卡通风格精灵素材（方案B）
/// </summary>
public class SpriteGeneratorEditor : EditorWindow
{
    [MenuItem("Tools/贪吃蛇/生成游戏素材")]
    public static void ShowWindow()
    {
        GetWindow<SpriteGeneratorEditor>("贪吃蛇素材生成器");
    }

    // 方案B：卡通风格配色
    private Color snakeHeadColor = new Color(1.0f, 0.75f, 0.10f); // 金黄色蛇头
    private Color snakeBodyColor = new Color(1.0f, 0.85f, 0.20f); // 亮黄色蛇身
    private Color snakeBodyPatternColor = new Color(0.90f, 0.60f, 0.05f); // 深橙色花纹
    private Color foodAppleColor = new Color(1.0f, 0.15f, 0.10f);  // 苹果红
    private Color foodLeafColor = new Color(0.10f, 0.80f, 0.10f);  // 叶子绿
    private Color wallColor = new Color(0.70f, 0.55f, 0.35f);      // 暖棕色砖块
    private Color wallMortarColor = new Color(0.55f, 0.40f, 0.25f); // 灰泥色
    private Color bgGrassColor1 = new Color(0.45f, 0.78f, 0.30f);  // 草地色1（浅亮绿）
    private Color bgGrassColor2 = new Color(0.22f, 0.55f, 0.15f);  // 草地色2（深暗绿）
    private int pixelSize = 64;

    // 表情选择
    private enum SnakeExpression
    {
        Happy,      // 开心（默认）
        Eating,     // 吃食物时张嘴
        Tongue      // 吐舌头
    }
    private SnakeExpression currentExpression = SnakeExpression.Happy;

    // 食物类型选择
    private enum FoodType
    {
        Apple,      // 苹果
        Strawberry, // 草莓
        Cherry      // 樱桃
    }
    private FoodType currentFood = FoodType.Apple;

    private void OnGUI()
    {
        GUILayout.Label("贪吃蛇游戏素材生成器 - 卡通风格", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        pixelSize = EditorGUILayout.IntField("精灵尺寸 (像素)", pixelSize);
        snakeHeadColor = EditorGUILayout.ColorField("蛇头颜色", snakeHeadColor);
        snakeBodyColor = EditorGUILayout.ColorField("蛇身颜色", snakeBodyColor);
        snakeBodyPatternColor = EditorGUILayout.ColorField("蛇身花纹颜色", snakeBodyPatternColor);
        wallColor = EditorGUILayout.ColorField("墙壁砖块颜色", wallColor);
        bgGrassColor1 = EditorGUILayout.ColorField("背景草地色1", bgGrassColor1);
        bgGrassColor2 = EditorGUILayout.ColorField("背景草地色2", bgGrassColor2);

        EditorGUILayout.Space();
        GUILayout.Label("蛇头表情", EditorStyles.boldLabel);
        currentExpression = (SnakeExpression)EditorGUILayout.EnumPopup("表情", currentExpression);

        EditorGUILayout.Space();
        GUILayout.Label("食物类型", EditorStyles.boldLabel);
        currentFood = (FoodType)EditorGUILayout.EnumPopup("食物", currentFood);

        EditorGUILayout.Space();

        if (GUILayout.Button("生成所有素材", GUILayout.Height(40)))
        {
            GenerateAllSprites();
        }
    }

    private void GenerateAllSprites()
    {
        string path = "Assets/Sprites/Generated/";
        Directory.CreateDirectory(path);

        GenerateSnakeHead(path);
        GenerateSnakeBody(path);
        GenerateFood(path);
        GenerateWall(path);
        GenerateBackground(path);

        AssetDatabase.Refresh();
        Debug.Log("所有卡通风格素材生成完成！");
    }

    private Texture2D CreateTexture(int width, int height)
    {
        Texture2D tex = new Texture2D(width, height);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;
        return tex;
    }

    private void GenerateSnakeHead(string path)
    {
        Texture2D tex = CreateTexture(pixelSize, pixelSize);
        Color[] pixels = new Color[pixelSize * pixelSize];

        int half = pixelSize / 2;

        for (int y = 0; y < pixelSize; y++)
        {
            for (int x = 0; x < pixelSize; x++)
            {
                // 圆润卡通蛇头（椭圆形主体）
                float cx = x - half;
                float cy = y - half;
                float rx = pixelSize * 0.38f;
                float ry = pixelSize * 0.35f;
                float dist = (cx * cx) / (rx * rx) + (cy * cy) / (ry * ry);

                if (dist <= 1.0f)
                {
                    // 蛇头主体 - 使用渐变色增加立体感
                    float t = dist;
                    Color headColor = Color.Lerp(snakeHeadColor, Color.Lerp(snakeHeadColor, Color.white, 0.15f), 1 - t);
                    pixels[y * pixelSize + x] = headColor;

                    // 绘制表情
                    int eyeY = (int)(-half * 0.15f);
                    int eyeSpacing = (int)(pixelSize * 0.18f);
                    int eyeSize = (int)(pixelSize * 0.08f);
                    int pupilSize = (int)(pixelSize * 0.04f);

                    // 左眼
                    int leftEyeX = half - eyeSpacing;
                    int rightEyeX = half + eyeSpacing;

                    // 眼睛（白色椭圆）
                    float eyeRx = eyeSize * 1.2f;
                    float eyeRy = eyeSize * 1.0f;
                    float leftEyeDist = ((x - leftEyeX) * (x - leftEyeX)) / (eyeRx * eyeRx) + ((y - (half + eyeY)) * (y - (half + eyeY))) / (eyeRy * eyeRy);
                    float rightEyeDist = ((x - rightEyeX) * (x - rightEyeX)) / (eyeRx * eyeRx) + ((y - (half + eyeY)) * (y - (half + eyeY))) / (eyeRy * eyeRy);

                    if (leftEyeDist <= 1.0f || rightEyeDist <= 1.0f)
                    {
                        pixels[y * pixelSize + x] = Color.white;

                        // 瞳孔（黑色圆形）
                        float pupilR = pupilSize;
                        float leftPupilDist = Mathf.Sqrt((x - leftEyeX) * (x - leftEyeX) + (y - (half + eyeY)) * (y - (half + eyeY)));
                        float rightPupilDist = Mathf.Sqrt((x - rightEyeX) * (x - rightEyeX) + (y - (half + eyeY)) * (y - (half + eyeY)));

                        if (leftPupilDist <= pupilR || rightPupilDist <= pupilR)
                        {
                            pixels[y * pixelSize + x] = Color.black;
                            // 瞳孔高光
                            if (leftPupilDist <= pupilR * 0.4f || rightPupilDist <= pupilR * 0.4f)
                            {
                                pixels[y * pixelSize + x] = Color.white;
                            }
                        }
                    }

                    // 根据表情绘制嘴巴
                    int mouthY = half + (int)(pixelSize * 0.20f);
                    int mouthX = half;

                    switch (currentExpression)
                    {
                        case SnakeExpression.Happy:
                            // 微笑弧线
                            {
                                int mouthWidth = (int)(pixelSize * 0.20f);
                                int dx = x - mouthX;
                                int dy = y - mouthY;
                                // 抛物线形状的微笑
                                int curveY = (int)(-dx * dx / (float)(mouthWidth * mouthWidth / 4) * (pixelSize * 0.04f));
                                if (Mathf.Abs(dx) <= mouthWidth && Mathf.Abs(dy - curveY) <= 1)
                                {
                                    pixels[y * pixelSize + x] = new Color(0.05f, 0.05f, 0.05f);
                                }
                            }
                            break;

                        case SnakeExpression.Eating:
                            // 张嘴（张开的大嘴）
                            {
                                int mouthOpen = (int)(pixelSize * 0.12f);
                                int mouthWidth = (int)(pixelSize * 0.22f);
                                if (Mathf.Abs(x - mouthX) <= mouthWidth && Mathf.Abs(y - mouthY) <= mouthOpen)
                                {
                                    // 嘴内部（深色）
                                    pixels[y * pixelSize + x] = new Color(0.1f, 0.05f, 0.05f);
                                    // 舌头（红色）
                                    if (Mathf.Abs(y - (mouthY + mouthOpen * 0.5f)) <= 2 && Mathf.Abs(x - mouthX) <= mouthWidth * 0.5f)
                                    {
                                        pixels[y * pixelSize + x] = new Color(1f, 0.2f, 0.2f);
                                    }
                                }
                            }
                            break;

                        case SnakeExpression.Tongue:
                            // 吐舌头
                            {
                                int mouthWidth = (int)(pixelSize * 0.15f);
                                int dx = x - mouthX;
                                int dy = y - mouthY;
                                int curveY = (int)(-dx * dx / (float)(mouthWidth * mouthWidth / 4) * (pixelSize * 0.03f));
                                if (Mathf.Abs(dx) <= mouthWidth && Mathf.Abs(dy - curveY) <= 1)
                                {
                                    pixels[y * pixelSize + x] = new Color(0.05f, 0.05f, 0.05f);
                                }
                                // 舌头从嘴巴伸出
                                int tongueTip = mouthY + (int)(pixelSize * 0.08f);
                                if (Mathf.Abs(x - mouthX) <= (int)(pixelSize * 0.04f) && y >= mouthY && y <= tongueTip)
                                {
                                    pixels[y * pixelSize + x] = new Color(1f, 0.2f, 0.2f);
                                }
                                // 舌尖分叉
                                if (y == tongueTip && (x == mouthX - 2 || x == mouthX + 2))
                                {
                                    pixels[y * pixelSize + x] = new Color(1f, 0.2f, 0.2f);
                                }
                            }
                            break;
                    }
                }
                else
                {
                    pixels[y * pixelSize + x] = Color.clear;
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        File.WriteAllBytes(path + "SnakeHead.png", tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
    }

    private void GenerateSnakeBody(string path)
    {
        Texture2D tex = CreateTexture(pixelSize, pixelSize);
        Color[] pixels = new Color[pixelSize * pixelSize];

        int half = pixelSize / 2;
        float rx = pixelSize * 0.35f;
        float ry = pixelSize * 0.30f;

        for (int y = 0; y < pixelSize; y++)
        {
            for (int x = 0; x < pixelSize; x++)
            {
                float cx = x - half;
                float cy = y - half;
                float dist = (cx * cx) / (rx * rx) + (cy * cy) / (ry * ry);

                if (dist <= 1.0f)
                {
                    // 蛇身主体
                    pixels[y * pixelSize + x] = snakeBodyColor;

                    // 绘制花纹（菱形斑点图案）
                    float patternScale = 0.15f;
                    int px = (int)(cx * patternScale);
                    int py = (int)(cy * patternScale);
                    bool hasPattern = ((px + py) % 3 == 0) && Mathf.Abs(cx) < rx * 0.6f && Mathf.Abs(cy) < ry * 0.6f;

                    if (hasPattern)
                    {
                        // 花纹菱形
                        float pd = Mathf.Abs(cx * 0.08f) + Mathf.Abs(cy * 0.10f);
                        if (pd < 0.5f)
                        {
                            pixels[y * pixelSize + x] = snakeBodyPatternColor;
                        }
                    }

                    // 边缘高光（增加立体感）
                    float edgeGlow = 1.0f - Mathf.Sqrt(dist);
                    if (edgeGlow < 0.3f && edgeGlow > 0.1f)
                    {
                        pixels[y * pixelSize + x] = Color.Lerp(snakeBodyColor, Color.white, 0.2f);
                    }
                }
                else
                {
                    pixels[y * pixelSize + x] = Color.clear;
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        File.WriteAllBytes(path + "SnakeBody.png", tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
    }

    private void GenerateFood(string path)
    {
        Texture2D tex = CreateTexture(pixelSize, pixelSize);
        Color[] pixels = new Color[pixelSize * pixelSize];

        int half = pixelSize / 2;

        switch (currentFood)
        {
            case FoodType.Apple:
                GenerateApple(pixels, half);
                break;
            case FoodType.Strawberry:
                GenerateStrawberry(pixels, half);
                break;
            case FoodType.Cherry:
                GenerateCherry(pixels, half);
                break;
        }

        tex.SetPixels(pixels);
        tex.Apply();
        File.WriteAllBytes(path + "Food.png", tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
    }

    private void GenerateApple(Color[] pixels, int half)
    {
        int size = pixelSize;
        float rx = size * 0.30f;
        float ry = size * 0.32f;
        int stemHeight = (int)(size * 0.12f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float cx = x - half;
                float cy = y - half;
                float dist = (cx * cx) / (rx * rx) + (cy * cy) / (ry * ry);

                if (dist <= 1.0f)
                {
                    // 苹果主体 - 渐变色
                    float t = dist;
                    Color appleColor = Color.Lerp(foodAppleColor, Color.Lerp(foodAppleColor, Color.white, 0.2f), 1 - t);
                    pixels[y * size + x] = appleColor;

                    // 高光
                    float highlightDist = Mathf.Sqrt((cx + rx * 0.3f) * (cx + rx * 0.3f) + (cy + ry * 0.3f) * (cy + ry * 0.3f));
                    if (highlightDist < rx * 0.25f)
                    {
                        pixels[y * size + x] = Color.Lerp(appleColor, Color.white, 0.4f);
                    }
                }
                else
                {
                    pixels[y * size + x] = Color.clear;
                }
            }
        }

        // 绘制苹果梗
        int stemX = half;
        int stemBase = half - (int)(ry * 0.85f);
        for (int y = stemBase - stemHeight; y < stemBase; y++)
        {
            if (y >= 0 && y < size)
            {
                int sx = stemX + (y - stemBase) / 2;
                if (sx >= 0 && sx < size)
                {
                    pixels[y * size + sx] = new Color(0.4f, 0.25f, 0.1f);
                }
            }
        }

        // 绘制叶子
        int leafX = stemX + (int)(size * 0.06f);
        int leafY = stemBase - stemHeight / 2;
        for (int dy = -3; dy <= 3; dy++)
        {
            for (int dx = -2; dx <= 6; dx++)
            {
                int lx = leafX + dx;
                int ly = leafY + dy;
                if (lx >= 0 && lx < size && ly >= 0 && ly < size)
                {
                    float ld = Mathf.Sqrt(dx * dx * 0.5f + dy * dy);
                    if (ld <= 4)
                    {
                        pixels[ly * size + lx] = foodLeafColor;
                    }
                }
            }
        }
    }

    private void GenerateStrawberry(Color[] pixels, int half)
    {
        int size = pixelSize;
        float rx = size * 0.28f;
        float ry = size * 0.32f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float cx = x - half;
                float cy = y - half + size * 0.03f;
                // 草莓形状（心形变体）
                float dist = (cx * cx) / (rx * rx) + (cy * cy) / (ry * ry) * (1 + cx * 0.1f / rx);

                if (dist <= 1.0f && cy > -ry * 0.3f)
                {
                    Color strawberryColor = new Color(1f, 0.1f, 0.15f);
                    pixels[y * size + x] = strawberryColor;

                    // 种子（小黄点）
                    int seedX = (int)(cx / 6);
                    int seedY = (int)(cy / 6);
                    if ((seedX + seedY) % 2 == 0 && Mathf.Abs(cx) < rx * 0.6f && cy > -ry * 0.3f)
                    {
                        if (Mathf.Abs(cx - seedX * 6) < 2 && Mathf.Abs(cy - seedY * 6) < 2)
                        {
                            pixels[y * size + x] = new Color(1f, 0.85f, 0.2f);
                        }
                    }

                    // 高光
                    float highlightDist = Mathf.Sqrt((cx + rx * 0.25f) * (cx + rx * 0.25f) + (cy + ry * 0.2f) * (cy + ry * 0.2f));
                    if (highlightDist < rx * 0.2f)
                    {
                        pixels[y * size + x] = Color.Lerp(strawberryColor, Color.white, 0.3f);
                    }
                }
                else
                {
                    pixels[y * size + x] = Color.clear;
                }
            }
        }

        // 草莓叶子
        int leafY = half - (int)(ry * 0.7f);
        for (int dy = -4; dy <= 2; dy++)
        {
            for (int dx = -6; dx <= 6; dx++)
            {
                int lx = half + dx;
                int ly = leafY + dy;
                if (lx >= 0 && lx < size && ly >= 0 && ly < size)
                {
                    float ld = Mathf.Sqrt(dx * dx * 0.4f + dy * dy * 1.5f);
                    if (ld <= 4)
                    {
                        pixels[ly * size + lx] = foodLeafColor;
                    }
                }
            }
        }
    }

    private void GenerateCherry(Color[] pixels, int half)
    {
        int size = pixelSize;
        float rx = size * 0.18f;
        float ry = size * 0.20f;

        // 两颗樱桃
        int[] cherryOffsets = { -6, 6 };
        foreach (int offsetX in cherryOffsets)
        {
            int cherryCX = half + offsetX;
            int cherryCY = half + (int)(size * 0.05f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float cx = x - cherryCX;
                    float cy = y - cherryCY;
                    float dist = (cx * cx) / (rx * rx) + (cy * cy) / (ry * ry);

                    if (dist <= 1.0f)
                    {
                        Color cherryColor = new Color(0.85f, 0.05f, 0.10f);
                        pixels[y * size + x] = cherryColor;

                        // 高光
                        float highlightDist = Mathf.Sqrt((cx + rx * 0.3f) * (cx + rx * 0.3f) + (cy + ry * 0.3f) * (cy + ry * 0.3f));
                        if (highlightDist < rx * 0.3f)
                        {
                            pixels[y * size + x] = Color.Lerp(cherryColor, Color.white, 0.5f);
                        }
                    }
                }
            }

            // 樱桃梗
            int stemTop = cherryCY - (int)(ry * 0.85f);
            for (int dy = -8; dy <= 0; dy++)
            {
                int sy = stemTop + dy;
                int sx = cherryCX + dy / 2;
                if (sy >= 0 && sy < size && sx >= 0 && sx < size)
                {
                    pixels[sy * size + sx] = new Color(0.3f, 0.6f, 0.1f);
                }
            }
        }

        // 连接两根梗的顶部
        int topY = half - (int)(ry * 0.85f) - 8;
        for (int x = half - 6; x <= half + 6; x++)
        {
            if (x >= 0 && x < size && topY >= 0 && topY < size)
            {
                pixels[topY * size + x] = new Color(0.3f, 0.6f, 0.1f);
            }
        }
    }

    private void GenerateWall(string path)
    {
        Texture2D tex = CreateTexture(pixelSize, pixelSize);
        Color[] pixels = new Color[pixelSize * pixelSize];

        int brickW = pixelSize / 4;
        int brickH = pixelSize / 5;

        for (int y = 0; y < pixelSize; y++)
        {
            for (int x = 0; x < pixelSize; x++)
            {
                int row = y / brickH;

                // 砖块边界（灰泥）
                int inBrickX = x % brickW;
                int inBrickY = y % brickH;
                int mortarSize = 2;

                if (inBrickX < mortarSize || inBrickX >= brickW - mortarSize ||
                    inBrickY < mortarSize || inBrickY >= brickH - mortarSize)
                {
                    pixels[y * pixelSize + x] = wallMortarColor;
                }
                else
                {
                    // 砖块内部 - 使用暖色并增加变化
                    float variation = ((x * 7 + y * 13) % 10) / 20f - 0.25f;
                    Color brickColor = new Color(
                        Mathf.Clamp01(wallColor.r + variation),
                        Mathf.Clamp01(wallColor.g + variation * 0.8f),
                        Mathf.Clamp01(wallColor.b + variation * 0.5f)
                    );
                    pixels[y * pixelSize + x] = brickColor;

                    // 砖块纹理（轻微噪点）
                    if ((x * 3 + y * 7) % 5 == 0)
                    {
                        pixels[y * pixelSize + x] = Color.Lerp(brickColor, Color.white, 0.1f);
                    }
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        File.WriteAllBytes(path + "Wall.png", tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
    }

    private void GenerateBackground(string path)
    {
        // 生成浅色背景瓦片
        GenerateSingleBackground(path, "Background_Light.png", bgGrassColor1);
        // 生成深色背景瓦片
        GenerateSingleBackground(path, "Background_Dark.png", bgGrassColor2);
    }

    private void GenerateSingleBackground(string path, string filename, Color baseColor)
    {
        Texture2D tex = CreateTexture(pixelSize, pixelSize);
        Color[] pixels = new Color[pixelSize * pixelSize];

        for (int y = 0; y < pixelSize; y++)
        {
            for (int x = 0; x < pixelSize; x++)
            {
                // 添加草地纹理细节（随机小草）
                float noise = ((x * 17 + y * 31) % 20) / 100f;
                Color grassColor = Color.Lerp(baseColor, Color.Lerp(baseColor, Color.white, 0.1f), noise);

                // 偶尔的小花点缀
                if ((x * 13 + y * 7) % 47 == 0)
                {
                    grassColor = Color.Lerp(grassColor, Color.white, 0.3f);
                }

                pixels[y * pixelSize + x] = grassColor;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        File.WriteAllBytes(path + filename, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
    }
}