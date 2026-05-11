using System;
using System.IO;

/// <summary>
/// 独立运行的贪吃蛇游戏素材生成器（方案B：卡通风格）
/// 无需Unity编辑器，直接运行生成PNG素材
/// </summary>
public class SpriteGenerator
{
    // 方案B：卡通风格配色
    private static Color snakeHeadColor = new Color(0.15f, 0.75f, 0.25f);
    private static Color snakeBodyColor = new Color(0.20f, 0.85f, 0.30f);
    private static Color snakeBodyPatternColor = new Color(0.10f, 0.60f, 0.20f);
    private static Color foodAppleColor = new Color(1.0f, 0.15f, 0.10f);
    private static Color foodLeafColor = new Color(0.10f, 0.80f, 0.10f);
    private static Color wallColor = new Color(0.70f, 0.55f, 0.35f);
    private static Color wallMortarColor = new Color(0.55f, 0.40f, 0.25f);
    private static Color bgGrassColor1 = new Color(0.35f, 0.70f, 0.25f);
    private static Color bgGrassColor2 = new Color(0.30f, 0.65f, 0.20f);
    private static int pixelSize = 64;

    public static void Main(string[] args)
    {
        string outputPath = "Assets/Sprites/Generated/";
        Directory.CreateDirectory(outputPath);

        Console.WriteLine("开始生成贪吃蛇游戏素材（方案B：卡通风格）...");

        GenerateSnakeHead(outputPath);
        Console.WriteLine("  ✓ SnakeHead.png 生成完成");

        GenerateSnakeBody(outputPath);
        Console.WriteLine("  ✓ SnakeBody.png 生成完成");

        GenerateFood(outputPath);
        Console.WriteLine("  ✓ Food.png 生成完成");

        GenerateWall(outputPath);
        Console.WriteLine("  ✓ Wall.png 生成完成");

        GenerateBackground(outputPath);
        Console.WriteLine("  ✓ Background.png 生成完成");

        Console.WriteLine("\n所有素材生成完成！");
    }

    private static Texture2D CreateTexture(int width, int height)
    {
        Texture2D tex = new Texture2D(width, height);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;
        return tex;
    }

    private static void GenerateSnakeHead(string path)
    {
        Texture2D tex = CreateTexture(pixelSize, pixelSize);
        Color[] pixels = new Color[pixelSize * pixelSize];

        int half = pixelSize / 2;

        for (int y = 0; y < pixelSize; y++)
        {
            for (int x = 0; x < pixelSize; x++)
            {
                float cx = x - half;
                float cy = y - half;
                float rx = pixelSize * 0.38f;
                float ry = pixelSize * 0.35f;
                float dist = (cx * cx) / (rx * rx) + (cy * cy) / (ry * ry);

                if (dist <= 1.0f)
                {
                    float t = dist;
                    Color headColor = LerpColor(snakeHeadColor, LerpColor(snakeHeadColor, new Color(1, 1, 1), 0.15f), 1 - t);
                    pixels[y * pixelSize + x] = headColor;

                    // 眼睛
                    int eyeY = (int)(-half * 0.15f);
                    int eyeSpacing = (int)(pixelSize * 0.18f);
                    int eyeSize = (int)(pixelSize * 0.08f);
                    int pupilSize = (int)(pixelSize * 0.04f);

                    int leftEyeX = half - eyeSpacing;
                    int rightEyeX = half + eyeSpacing;

                    float eyeRx = eyeSize * 1.2f;
                    float eyeRy = eyeSize * 1.0f;
                    float leftEyeDist = ((x - leftEyeX) * (x - leftEyeX)) / (eyeRx * eyeRx) + ((y - (half + eyeY)) * (y - (half + eyeY))) / (eyeRy * eyeRy);
                    float rightEyeDist = ((x - rightEyeX) * (x - rightEyeX)) / (eyeRx * eyeRx) + ((y - (half + eyeY)) * (y - (half + eyeY))) / (eyeRy * eyeRy);

                    if (leftEyeDist <= 1.0f || rightEyeDist <= 1.0f)
                    {
                        pixels[y * pixelSize + x] = new Color(1, 1, 1);

                        float pupilR = pupilSize;
                        float leftPupilDist = MathF.Sqrt((x - leftEyeX) * (x - leftEyeX) + (y - (half + eyeY)) * (y - (half + eyeY)));
                        float rightPupilDist = MathF.Sqrt((x - rightEyeX) * (x - rightEyeX) + (y - (half + eyeY)) * (y - (half + eyeY)));

                        if (leftPupilDist <= pupilR || rightPupilDist <= pupilR)
                        {
                            pixels[y * pixelSize + x] = new Color(0, 0, 0);
                            if (leftPupilDist <= pupilR * 0.4f || rightPupilDist <= pupilR * 0.4f)
                            {
                                pixels[y * pixelSize + x] = new Color(1, 1, 1);
                            }
                        }
                    }

                    // 微笑表情
                    int mouthY = half + (int)(pixelSize * 0.20f);
                    int mouthX = half;
                    int mouthWidth = (int)(pixelSize * 0.20f);
                    int dx = x - mouthX;
                    int dy = y - mouthY;
                    int curveY = (int)(-dx * dx / (float)(mouthWidth * mouthWidth / 4) * (pixelSize * 0.04f));
                    if (MathF.Abs(dx) <= mouthWidth && MathF.Abs(dy - curveY) <= 1)
                    {
                        pixels[y * pixelSize + x] = new Color(0.05f, 0.05f, 0.05f);
                    }
                }
                else
                {
                    pixels[y * pixelSize + x] = new Color(0, 0, 0, 0);
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        File.WriteAllBytes(path + "SnakeHead.png", tex.EncodeToPNG());
    }

    private static void GenerateSnakeBody(string path)
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
                    pixels[y * pixelSize + x] = snakeBodyColor;

                    // 花纹
                    float patternScale = 0.15f;
                    int px = (int)(cx * patternScale);
                    int py = (int)(cy * patternScale);
                    bool hasPattern = ((px + py) % 3 == 0) && MathF.Abs(cx) < rx * 0.6f && MathF.Abs(cy) < ry * 0.6f;

                    if (hasPattern)
                    {
                        float pd = MathF.Abs(cx * 0.08f) + MathF.Abs(cy * 0.10f);
                        if (pd < 0.5f)
                        {
                            pixels[y * pixelSize + x] = snakeBodyPatternColor;
                        }
                    }

                    // 边缘高光
                    float edgeGlow = 1.0f - MathF.Sqrt(dist);
                    if (edgeGlow < 0.3f && edgeGlow > 0.1f)
                    {
                        pixels[y * pixelSize + x] = LerpColor(snakeBodyColor, new Color(1, 1, 1), 0.2f);
                    }
                }
                else
                {
                    pixels[y * pixelSize + x] = new Color(0, 0, 0, 0);
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        File.WriteAllBytes(path + "SnakeBody.png", tex.EncodeToPNG());
    }

    private static void GenerateFood(string path)
    {
        Texture2D tex = CreateTexture(pixelSize, pixelSize);
        Color[] pixels = new Color[pixelSize * pixelSize];

        int half = pixelSize / 2;
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
                    float t = dist;
                    Color appleColor = LerpColor(foodAppleColor, LerpColor(foodAppleColor, new Color(1, 1, 1), 0.2f), 1 - t);
                    pixels[y * size + x] = appleColor;

                    float highlightDist = MathF.Sqrt((cx + rx * 0.3f) * (cx + rx * 0.3f) + (cy + ry * 0.3f) * (cy + ry * 0.3f));
                    if (highlightDist < rx * 0.25f)
                    {
                        pixels[y * size + x] = LerpColor(appleColor, new Color(1, 1, 1), 0.4f);
                    }
                }
                else
                {
                    pixels[y * size + x] = new Color(0, 0, 0, 0);
                }
            }
        }

        // 苹果梗
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

        // 叶子
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
                    float ld = MathF.Sqrt(dx * dx * 0.5f + dy * dy);
                    if (ld <= 4)
                    {
                        pixels[ly * size + lx] = foodLeafColor;
                    }
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        File.WriteAllBytes(path + "Food.png", tex.EncodeToPNG());
    }

    private static void GenerateWall(string path)
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
                    float variation = ((x * 7 + y * 13) % 10) / 20f - 0.25f;
                    Color brickColor = new Color(
                        MathF.Clamp(wallColor.r + variation, 0, 1),
                        MathF.Clamp(wallColor.g + variation * 0.8f, 0, 1),
                        MathF.Clamp(wallColor.b + variation * 0.5f, 0, 1)
                    );
                    pixels[y * pixelSize + x] = brickColor;

                    if ((x * 3 + y * 7) % 5 == 0)
                    {
                        pixels[y * pixelSize + x] = LerpColor(brickColor, new Color(1, 1, 1), 0.1f);
                    }
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        File.WriteAllBytes(path + "Wall.png", tex.EncodeToPNG());
    }

    private static void GenerateBackground(string path)
    {
        Texture2D tex = CreateTexture(pixelSize, pixelSize);
        Color[] pixels = new Color[pixelSize * pixelSize];

        int grassSize = 8;

        for (int y = 0; y < pixelSize; y++)
        {
            for (int x = 0; x < pixelSize; x++)
            {
                bool isLight = ((x / grassSize) + (y / grassSize)) % 2 == 0;
                Color baseColor = isLight ? bgGrassColor1 : bgGrassColor2;

                float noise = ((x * 17 + y * 31) % 20) / 100f;
                Color grassColor = LerpColor(baseColor, LerpColor(baseColor, new Color(1, 1, 1), 0.1f), noise);

                if ((x * 13 + y * 7) % 47 == 0 && (x % grassSize == 0 || y % grassSize == 0))
                {
                    grassColor = LerpColor(grassColor, new Color(1, 1, 1), 0.3f);
                }

                pixels[y * pixelSize + x] = grassColor;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        File.WriteAllBytes(path + "Background.png", tex.EncodeToPNG());
    }

    // 简单的颜色结构体
    private struct Color
    {
        public float r, g, b, a;

        public Color(float r, float g, float b, float a = 1f)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public static Color Lerp(Color a, Color b, float t)
        {
            t = MathF.Clamp(t, 0, 1);
            return new Color(
                a.r + (b.r - a.r) * t,
                a.g + (b.g - a.g) * t,
                a.b + (b.b - a.b) * t,
                a.a + (b.a - a.a) * t
            );
        }
    }

    private static Color LerpColor(Color a, Color b, float t)
    {
        return Color.Lerp(a, b, t);
    }

    // 简单的纹理类
    private class Texture2D
    {
        private int width;
        private int height;
        private Color[] pixels;
        public FilterMode filterMode;
        public WrapMode wrapMode;

        public Texture2D(int width, int height)
        {
            this.width = width;
            this.height = height;
            this.pixels = new Color[width * height];
        }

        public void SetPixels(Color[] colors)
        {
            pixels = colors;
        }

        public void Apply() { }

        public byte[] EncodeToPNG()
        {
            // 手动生成PNG文件
            return GeneratePNG(pixels, width, height);
        }

        private byte[] GeneratePNG(Color[] pixels, int w, int h)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // PNG签名
                byte[] signature = { 137, 80, 78, 71, 13, 10, 26, 10 };
                ms.Write(signature, 0, 8);

                // IHDR块
                WriteIHDR(ms, w, h);

                // IDAT块 - 使用原始像素数据（无压缩，简单实现）
                byte[] rawData = new byte[w * h * 4];
                for (int y = 0; y < h; y++)
                {
                    // 每行以0（无过滤）开头
                    rawData[y * w * 4] = 0;
                    for (int x = 0; x < w; x++)
                    {
                        int srcIdx = y * w + x;
                        int dstIdx = y * w * 4 + 1 + x * 4;
                        rawData[dstIdx] = (byte)(pixels[srcIdx].r * 255);
                        rawData[dstIdx + 1] = (byte)(pixels[srcIdx].g * 255);
                        rawData[dstIdx + 2] = (byte)(pixels[srcIdx].b * 255);
                        rawData[dstIdx + 3] = (byte)(pixels[srcIdx].a * 255);
                    }
                }

                // 使用zlib压缩（简化：存储为未压缩的deflate块）
                byte[] compressed = DeflateRaw(rawData);
                WriteChunk(ms, "IDAT", compressed);

                // IEND块
                WriteChunk(ms, "IEND", new byte[0]);

                return ms.ToArray();
            }
        }

        private void WriteIHDR(MemoryStream ms, int w, int h)
        {
            byte[] data = new byte[13];
            WriteInt32BE(data, 0, w);
            WriteInt32BE(data, 4, h);
            data[8] = 8; // 位深度
            data[9] = 6; // 颜色类型：RGBA
            data[10] = 0; // 压缩
            data[11] = 0; // 过滤
            data[12] = 0; // 交错
            WriteChunk(ms, "IHDR", data);
        }

        private void WriteChunk(MemoryStream ms, string type, byte[] data)
        {
            byte[] typeBytes = System.Text.Encoding.ASCII.GetBytes(type);
            byte[] lengthBytes = new byte[4];
            WriteInt32BE(lengthBytes, 0, data.Length);

            ms.Write(lengthBytes, 0, 4);
            ms.Write(typeBytes, 0, 4);
            ms.Write(data, 0, data.Length);

            // CRC
            byte[] crcData = new byte[typeBytes.Length + data.Length];
            Array.Copy(typeBytes, 0, crcData, 0, typeBytes.Length);
            Array.Copy(data, 0, crcData, typeBytes.Length, data.Length);
            uint crc = CRC32(crcData);
            byte[] crcBytes = new byte[4];
            WriteInt32BE(crcBytes, 0, (int)crc);
            ms.Write(crcBytes, 0, 4);
        }

        private void WriteInt32BE(byte[] buffer, int offset, int value)
        {
            buffer[offset] = (byte)((value >> 24) & 0xFF);
            buffer[offset + 1] = (byte)((value >> 16) & 0xFF);
            buffer[offset + 2] = (byte)((value >> 8) & 0xFF);
            buffer[offset + 3] = (byte)(value & 0xFF);
        }

        private byte[] DeflateRaw(byte[] data)
        {
            // 简化的deflate：存储块（无压缩）
            using (MemoryStream ms = new MemoryStream())
            {
                // Zlib头
                ms.WriteByte(0x78);
                ms.WriteByte(0x01);

                // Deflate存储块
                int pos = 0;
                while (pos < data.Length)
                {
                    int blockSize = Math.Min(65535, data.Length - pos);
                    bool isFinal = (pos + blockSize >= data.Length);

                    // 块头
                    ms.WriteByte((byte)(isFinal ? 0x01 : 0x00));

                    // 块大小（小端）
                    ms.WriteByte((byte)(blockSize & 0xFF));
                    ms.WriteByte((byte)((blockSize >> 8) & 0xFF));
                    ms.WriteByte((byte)(~(blockSize & 0xFF) & 0xFF));
                    ms.WriteByte((byte)(~((blockSize >> 8) & 0xFF) & 0xFF));

                    // 块数据
                    ms.Write(data, pos, blockSize);
                    pos += blockSize;
                }

                // Adler32校验和
                uint a1 = 1, a2 = 0;
                foreach (byte b in data)
                {
                    a1 = (a1 + b) % 65521;
                    a2 = (a2 + a1) % 65521;
                }
                uint adler = (a2 << 16) | a1;
                ms.WriteByte((byte)((adler >> 24) & 0xFF));
                ms.WriteByte((byte)((adler >> 16) & 0xFF));
                ms.WriteByte((byte)((adler >> 8) & 0xFF));
                ms.WriteByte((byte)(adler & 0xFF));

                return ms.ToArray();
            }
        }

        private static uint CRC32(byte[] data)
        {
            uint[] table = new uint[256];
            for (uint i = 0; i < 256; i++)
            {
                uint crc = i;
                for (int j = 0; j < 8; j++)
                {
                    if ((crc & 1) != 0)
                        crc = 0xEDB88320 ^ (crc >> 1);
                    else
                        crc = crc >> 1;
                }
                table[i] = crc;
            }

            uint result = 0xFFFFFFFF;
            foreach (byte b in data)
            {
                result = table[(result ^ b) & 0xFF] ^ (result >> 8);
            }
            return result ^ 0xFFFFFFFF;
        }
    }

    private enum FilterMode { Point }
    private enum WrapMode { Clamp }
}