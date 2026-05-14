# Directional Snake Body — Design Spec

**Date**: 2026-05-14
**Status**: Approved

## Goal

Replace the disconnected blob-style snake segments with a direction-aware, visually continuous snake using 4 base sprites + rotation.

## Sprite Assets Required (4 base sprites)

All sprites default-facing right (0°), rotated at runtime:

| # | Sprite | Orientation | Rotations |
|---|--------|-------------|-----------|
| 1 | Head — cartoon snake head with eyes | Facing right → | 0°/90°/180°/270° |
| 2 | Body straight — rounded-rect connecting two ends | Horizontal ↔ | 0°/90° |
| 3 | Body corner — 90° curved bend (connects Right+Up) | Right & Up (└) | 0°/90°/180°/270° |
| 4 | Tail — tapering tail tip | Facing right → | 0°/90°/180°/270° |

## Direction Calculation

- **Head**: uses `SnakeMovement.CurrentDirection` for facing
- **Body segment i**: computes two connection vectors:
  - `dirToHead` = `segments[i-1].position - segments[i].position`
  - `dirToTail` = `segments[i+1].position - segments[i].position`
  - Straight if `dirToHead == -dirToTail`; Corner if perpendicular
- **Tail**: direction = from tail position to `segments[count-2].position`

## Code Changes

### `SnakeSegment.cs`
- Add `SetRotation(float angleZ)` method

### `SnakeController.cs`
- Add 4 sprite fields: `headSprite`, `bodyStraightSprite`, `bodyCornerSprite`, `tailSprite`
- Add `UpdateSegmentVisuals()` — called after every move/grow/reset
- Removes old `bodySprite` field (replaced by straight + corner)

### `SpriteGeneratorEditor.cs`
- Add `GenerateBodyStraight()` — rounded rectangle
- Add `GenerateBodyCorner()` — 90° curved bend
- Add `GenerateSnakeTail()` — tapering tail shape
- Update `GenerateAllSprites()` to include new sprites

## Data Flow

```
MoveSnake() / Grow() / InitializeSnake()
    │
    ▼
UpdateSegmentPositions()  (existing)
    │
    ▼
UpdateSegmentVisuals()    (new)
  ├─ Head: headSprite + rotation from movement direction
  ├─ Body[1..n-2]: detect straight vs corner, set sprite + rotation
  └─ Tail: tailSprite + direction toward body[count-2]
```

## Existing Assets (for reference)

- `Assets/snakesprites/png/snake_yellow_head_64.png` — existing head (can reuse)
- `Assets/snakesprites/png/snake_yellow_blob_64.png` — existing blob (will be replaced by straight + corner)
- `Assets/Sprites/Generated/` — runtime-generated sprites from SpriteGeneratorEditor
