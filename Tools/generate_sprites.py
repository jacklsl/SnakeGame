#!/usr/bin/env python3
"""
贪吃蛇游戏素材生成器（方案B：卡通风格）
生成卡通风格的蛇头、蛇身、食物、墙壁和背景素材
"""

import struct
import zlib
import os

# 方案B：卡通风格配色
SNAKE_HEAD_COLOR = (1.0, 0.75, 0.10)  # 金黄色蛇头
SNAKE_BODY_COLOR = (1.0, 0.85, 0.20)  # 亮黄色蛇身
SNAKE_BODY_PATTERN_COLOR = (0.90, 0.60, 0.05)  # 深橙色花纹
FOOD_APPLE_COLOR = (1.0, 0.15, 0.10)  # 苹果红
FOOD_LEAF_COLOR = (0.10, 0.80, 0.10)  # 叶子绿
WALL_COLOR = (0.70, 0.55, 0.35)  # 暖棕色砖块
WALL_MORTAR_COLOR = (0.55, 0.40, 0.25)  # 灰泥色
BG_GRASS_COLOR1 = (0.45, 0.78, 0.30)  # 草地色1（浅亮绿）
BG_GRASS_COLOR2 = (0.22, 0.55, 0.15)  # 草地色2（深暗绿）
PIXEL_SIZE = 64


def lerp_color(c1, c2, t):
    """颜色插值"""
    return tuple(c1[i] + (c2[i] - c1[i]) * t for i in range(3))


def create_png(pixels, width, height):
    """创建PNG图像数据"""
    # 构建原始像素数据（RGBA）
    raw_data = bytearray()
    for y in range(height):
        raw_data.append(0)  # 过滤类型：None
        for x in range(width):
            r, g, b, a = pixels[y * width + x]
            raw_data.extend([int(r * 255), int(g * 255), int(b * 255), int(a * 255)])

    # 压缩
    compressed = zlib.compress(raw_data)

    # 构建PNG文件
    def write_chunk(chunk_type, data):
        chunk = chunk_type + data
        crc = struct.pack('>I', zlib.crc32(chunk) & 0xFFFFFFFF)
        return struct.pack('>I', len(data)) + chunk + crc

    # PNG签名
    signature = b'\x89PNG\r\n\x1a\n'

    # IHDR块
    ihdr_data = struct.pack('>IIBBBBB', width, height, 8, 6, 0, 0, 0)
    ihdr = write_chunk(b'IHDR', ihdr_data)

    # IDAT块
    idat = write_chunk(b'IDAT', compressed)

    # IEND块
    iend = write_chunk(b'IEND', b'')

    return signature + ihdr + idat + iend


def save_png(filename, pixels, width, height):
    """保存PNG文件"""
    png_data = create_png(pixels, width, height)
    with open(filename, 'wb') as f:
        f.write(png_data)
    print(f"  ✓ {os.path.basename(filename)} 生成完成")


def generate_snake_head():
    """生成卡通蛇头（椭圆造型，开心表情）"""
    pixels = []
    half = PIXEL_SIZE // 2
    rx = PIXEL_SIZE * 0.38
    ry = PIXEL_SIZE * 0.35

    for y in range(PIXEL_SIZE):
        for x in range(PIXEL_SIZE):
            cx = x - half
            cy = y - half
            dist = (cx * cx) / (rx * rx) + (cy * cy) / (ry * ry)

            if dist <= 1.0:
                # 蛇头主体 - 渐变色
                t = dist
                head_color = lerp_color(SNAKE_HEAD_COLOR, lerp_color(SNAKE_HEAD_COLOR, (1, 1, 1), 0.15), 1 - t)
                r, g, b = head_color
                a = 1.0

                # 眼睛
                eye_y = int(-half * 0.15)
                eye_spacing = int(PIXEL_SIZE * 0.18)
                eye_size = int(PIXEL_SIZE * 0.08)
                pupil_size = int(PIXEL_SIZE * 0.04)

                left_eye_x = half - eye_spacing
                right_eye_x = half + eye_spacing

                eye_rx = eye_size * 1.2
                eye_ry = eye_size * 1.0

                left_eye_dist = ((x - left_eye_x) ** 2) / (eye_rx ** 2) + ((y - (half + eye_y)) ** 2) / (eye_ry ** 2)
                right_eye_dist = ((x - right_eye_x) ** 2) / (eye_rx ** 2) + ((y - (half + eye_y)) ** 2) / (eye_ry ** 2)

                if left_eye_dist <= 1.0 or right_eye_dist <= 1.0:
                    r, g, b = 1.0, 1.0, 1.0  # 白色眼白

                    left_pupil_dist = ((x - left_eye_x) ** 2 + (y - (half + eye_y)) ** 2) ** 0.5
                    right_pupil_dist = ((x - right_eye_x) ** 2 + (y - (half + eye_y)) ** 2) ** 0.5

                    if left_pupil_dist <= pupil_size or right_pupil_dist <= pupil_size:
                        r, g, b = 0.0, 0.0, 0.0  # 黑色瞳孔
                        if left_pupil_dist <= pupil_size * 0.4 or right_pupil_dist <= pupil_size * 0.4:
                            r, g, b = 1.0, 1.0, 1.0  # 瞳孔高光

                # 微笑表情
                mouth_y = half + int(PIXEL_SIZE * 0.20)
                mouth_x = half
                mouth_width = int(PIXEL_SIZE * 0.20)
                dx = x - mouth_x
                dy = y - mouth_y
                curve_y = int(-dx * dx / (mouth_width * mouth_width / 4) * (PIXEL_SIZE * 0.04))
                if abs(dx) <= mouth_width and abs(dy - curve_y) <= 1:
                    r, g, b = 0.05, 0.05, 0.05

                pixels.append((r, g, b, a))
            else:
                pixels.append((0, 0, 0, 0))

    return pixels, PIXEL_SIZE, PIXEL_SIZE


def generate_snake_body():
    """生成卡通蛇身（椭圆造型，带菱形花纹）"""
    pixels = []
    half = PIXEL_SIZE // 2
    rx = PIXEL_SIZE * 0.35
    ry = PIXEL_SIZE * 0.30

    for y in range(PIXEL_SIZE):
        for x in range(PIXEL_SIZE):
            cx = x - half
            cy = y - half
            dist = (cx * cx) / (rx * rx) + (cy * cy) / (ry * ry)

            if dist <= 1.0:
                r, g, b = SNAKE_BODY_COLOR
                a = 1.0

                # 花纹（菱形斑点图案）
                pattern_scale = 0.15
                px = int(cx * pattern_scale)
                py = int(cy * pattern_scale)
                has_pattern = ((px + py) % 3 == 0) and abs(cx) < rx * 0.6 and abs(cy) < ry * 0.6

                if has_pattern:
                    pd = abs(cx * 0.08) + abs(cy * 0.10)
                    if pd < 0.5:
                        r, g, b = SNAKE_BODY_PATTERN_COLOR

                # 边缘高光
                edge_glow = 1.0 - dist ** 0.5
                if edge_glow < 0.3 and edge_glow > 0.1:
                    r, g, b = lerp_color(SNAKE_BODY_COLOR, (1, 1, 1), 0.2)

                pixels.append((r, g, b, a))
            else:
                pixels.append((0, 0, 0, 0))

    return pixels, PIXEL_SIZE, PIXEL_SIZE


def generate_food():
    """生成卡通食物（苹果）"""
    pixels = []
    half = PIXEL_SIZE // 2
    rx = PIXEL_SIZE * 0.30
    ry = PIXEL_SIZE * 0.32
    stem_height = int(PIXEL_SIZE * 0.12)

    for y in range(PIXEL_SIZE):
        for x in range(PIXEL_SIZE):
            cx = x - half
            cy = y - half
            dist = (cx * cx) / (rx * rx) + (cy * cy) / (ry * ry)

            if dist <= 1.0:
                t = dist
                apple_color = lerp_color(FOOD_APPLE_COLOR, lerp_color(FOOD_APPLE_COLOR, (1, 1, 1), 0.2), 1 - t)
                r, g, b = apple_color
                a = 1.0

                # 高光
                highlight_dist = ((cx + rx * 0.3) ** 2 + (cy + ry * 0.3) ** 2) ** 0.5
                if highlight_dist < rx * 0.25:
                    r, g, b = lerp_color(apple_color, (1, 1, 1), 0.4)

                pixels.append((r, g, b, a))
            else:
                pixels.append((0, 0, 0, 0))

    # 苹果梗
    stem_x = half
    stem_base = half - int(ry * 0.85)
    for y in range(stem_base - stem_height, stem_base):
        if 0 <= y < PIXEL_SIZE:
            sx = stem_x + (y - stem_base) // 2
            if 0 <= sx < PIXEL_SIZE:
                pixels[y * PIXEL_SIZE + sx] = (0.4, 0.25, 0.1, 1.0)

    # 叶子
    leaf_x = stem_x + int(PIXEL_SIZE * 0.06)
    leaf_y = stem_base - stem_height // 2
    for dy in range(-3, 4):
        for dx in range(-2, 7):
            lx = leaf_x + dx
            ly = leaf_y + dy
            if 0 <= lx < PIXEL_SIZE and 0 <= ly < PIXEL_SIZE:
                ld = (dx * dx * 0.5 + dy * dy) ** 0.5
                if ld <= 4:
                    pixels[ly * PIXEL_SIZE + lx] = (*FOOD_LEAF_COLOR, 1.0)

    return pixels, PIXEL_SIZE, PIXEL_SIZE


def generate_wall():
    """生成卡通砖块墙壁"""
    pixels = []
    brick_w = PIXEL_SIZE // 4
    brick_h = PIXEL_SIZE // 5

    for y in range(PIXEL_SIZE):
        for x in range(PIXEL_SIZE):
            in_brick_x = x % brick_w
            in_brick_y = y % brick_h
            mortar_size = 2

            if in_brick_x < mortar_size or in_brick_x >= brick_w - mortar_size or \
               in_brick_y < mortar_size or in_brick_y >= brick_h - mortar_size:
                r, g, b = WALL_MORTAR_COLOR
            else:
                variation = ((x * 7 + y * 13) % 10) / 20.0 - 0.25
                r = max(0, min(1, WALL_COLOR[0] + variation))
                g = max(0, min(1, WALL_COLOR[1] + variation * 0.8))
                b = max(0, min(1, WALL_COLOR[2] + variation * 0.5))

                if (x * 3 + y * 7) % 5 == 0:
                    r, g, b = lerp_color((r, g, b), (1, 1, 1), 0.1)

            pixels.append((r, g, b, 1.0))

    return pixels, PIXEL_SIZE, PIXEL_SIZE


def generate_background_light():
    """生成浅色草地背景瓦片（纯色，与蛇头大小一致 64x64）"""
    pixels = []
    for y in range(PIXEL_SIZE):
        for x in range(PIXEL_SIZE):
            # 添加草地纹理细节（随机小草）
            noise = ((x * 17 + y * 31) % 20) / 100.0
            grass_color = lerp_color(BG_GRASS_COLOR1, lerp_color(BG_GRASS_COLOR1, (1, 1, 1), 0.1), noise)
            # 偶尔的小花点缀
            if (x * 13 + y * 7) % 47 == 0:
                grass_color = lerp_color(grass_color, (1, 1, 1), 0.3)
            pixels.append((*grass_color, 1.0))
    return pixels, PIXEL_SIZE, PIXEL_SIZE


def generate_background_dark():
    """生成深色草地背景瓦片（纯色，与蛇头大小一致 64x64）"""
    pixels = []
    for y in range(PIXEL_SIZE):
        for x in range(PIXEL_SIZE):
            # 添加草地纹理细节（随机小草）
            noise = ((x * 17 + y * 31) % 20) / 100.0
            grass_color = lerp_color(BG_GRASS_COLOR2, lerp_color(BG_GRASS_COLOR2, (1, 1, 1), 0.1), noise)
            # 偶尔的小花点缀
            if (x * 13 + y * 7) % 47 == 0:
                grass_color = lerp_color(grass_color, (1, 1, 1), 0.3)
            pixels.append((*grass_color, 1.0))
    return pixels, PIXEL_SIZE, PIXEL_SIZE


def main():
    output_dir = "Assets/Sprites/Generated"
    os.makedirs(output_dir, exist_ok=True)

    print("开始生成贪吃蛇游戏素材（方案B：卡通风格）...")
    print()

    # 生成蛇头
    pixels, w, h = generate_snake_head()
    save_png(os.path.join(output_dir, "SnakeHead.png"), pixels, w, h)

    # 生成蛇身
    pixels, w, h = generate_snake_body()
    save_png(os.path.join(output_dir, "SnakeBody.png"), pixels, w, h)

    # 生成食物
    pixels, w, h = generate_food()
    save_png(os.path.join(output_dir, "Food.png"), pixels, w, h)

    # 生成墙壁
    pixels, w, h = generate_wall()
    save_png(os.path.join(output_dir, "Wall.png"), pixels, w, h)

    # 生成背景（浅色和深色两张瓦片，每个与蛇头大小一致 64x64）
    pixels, w, h = generate_background_light()
    save_png(os.path.join(output_dir, "Background_Light.png"), pixels, w, h)
    pixels, w, h = generate_background_dark()
    save_png(os.path.join(output_dir, "Background_Dark.png"), pixels, w, h)

    print()
    print("所有素材生成完成！")
    print(f"素材已保存到: {output_dir}/")


if __name__ == "__main__":
    main()