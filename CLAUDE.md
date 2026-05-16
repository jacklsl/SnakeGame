# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Unity 6000.4.5f1 2D Snake game using URP. The game runs on a single scene (`SampleScene.unity`) with all objects created at runtime by `SnakeGameBootstrap` — there are no prefabs placed in the scene.

## Build & Edit

- Open the project in Unity 6000.4.5f1+ (2D project with URP)
- Open `Assets/Scenes/SampleScene.unity` and press Play
- Generate sprites: Unity menu **Tools > 贪吃蛇 > 生成游戏素材** (outputs to `Assets/Sprites/Generated/`)
- There are no CLI build/test commands — everything runs through the Unity Editor

## Architecture

### Bootstrap & DI

[SnakeGameBootstrap.cs](Assets/Scripts/Runtime/SnakeGameBootstrap.cs) is the entry point. Via `[RuntimeInitializeOnLoadMethod]`, it creates a runtime GameObject `"Snake Game Runtime"` and attaches all manager components to it in dependency order. Each manager is registered into a static service locator.

[GameServices.cs](Assets/Scripts/Core/GameServices.cs) is a lightweight static service locator (`Register<T>` / `Get<T>` / `Unregister<T>`). Components register themselves in `Awake()` and unregister in `OnDestroy()`. All inter-manager communication uses this instead of `FindAnyObjectByType` or singletons.

[EventBus.cs](Assets/Scripts/Core/EventBus.cs) is a static pub/sub typed by struct events (zero GC). Events are defined in [EventTypes.cs](Assets/Scripts/Core/EventTypes.cs): `FoodEatenEvent`, `SnakeDiedEvent`, `ScoreChangedEvent`, `GameStateChangedEvent`.

[GameConfig.cs](Assets/Scripts/Core/GameConfig.cs) is a `ScriptableObject` holding all tunable parameters (grid size, movement speed, sprite paths, colors). The bootstrap loads it via `Resources.Load("Config/GameConfig")` and injects it into components that have a `config` field via reflection.

### Game State Machine

[GameManager.cs](Assets/Scripts/Managers/GameManager.cs) holds `GameState` (Ready → Playing → Paused → GameOver) and publishes `GameStateChangedEvent` on transitions. `GameManager.StartGame()` resets score/snake/food and begins play. `Time.timeScale` is used to pause (set to 0) and resume (set to 1).

### Snake System

- [SnakeController.cs](Assets/Scripts/Snake/SnakeController.cs) — manages the list of `SnakeSegment` GameObjects, initialization, movement loop, growth, collision, and death. Calls `SnakeMovement.Tick()` each frame.
- [SnakeMovement.cs](Assets/Scripts/Snake/SnakeMovement.cs) — **pure C# class (not MonoBehaviour)**. Handles move timing, speed-up on food eaten, direction buffering (prevents 180° reversal), and next-position calculation. Designed to be unit-testable without Unity.
- [SnakeSegment.cs](Assets/Scripts/Snake/SnakeSegment.cs) — component attached to head/body/food GameObjects. Holds grid position, sprite rendering, auto-scaling to cell size. Used by both snake parts and food.

### Grid System

[GridManager.cs](Assets/Scripts/Grid/GridManager.cs) owns the grid data (`bool[,] occupiedCells`), coordinate conversion (`GridToWorldPosition`/`WorldToGridPosition`), bounds checking, and empty-cell queries (used by food spawner). Two renderer components on the same GameObject handle visuals:
- [GridBackgroundRenderer.cs](Assets/Scripts/Grid/GridBackgroundRenderer.cs) — generates a checkerboard `Texture2D` from `GameConfig.BackgroundColorLight/Dark` and renders it as a single sprite.
- [GridWallRenderer.cs](Assets/Scripts/Grid/GridWallRenderer.cs) — spawns wall sprites around the grid perimeter.

### Food

[FoodSpawner.cs](Assets/Scripts/Food/FoodSpawner.cs) spawns food at a random empty cell, reuses `SnakeSegment` component, and checks head-vs-food collision each frame. On eat, calls `SnakeController.Grow()` (which publishes `FoodEatenEvent`), then respawns food.

### Input

[InputManager.cs](Assets/Scripts/Managers/InputManager.cs) handles WASD/arrow keys via the new Input System (`Keyboard.current`) and touch swipe gestures with a minimum swipe distance. Esc toggles pause.

### UI

[UIManager.cs](Assets/Scripts/UI/UIManager.cs) subscribes to `GameStateChangedEvent` and shows/hides the four view panels accordingly. Each view (`MainMenuView`, `GameHudView`, `PauseView`, `GameOverView`) can accept serialized prefab references; if none are assigned, they **code-generate their own fallback UI** using `CreateFallbackUI()` — defining buttons, text (TextMeshPro), and layout in C#. Views communicate back to `GameManager` via `GameServices.Get<GameManager>()`.

### Sprite Utilities

[SnakeSpriteLoader.cs](Assets/Scripts/Runtime/SnakeSpriteLoader.cs) loads sprites with an internal cache. In Editor, uses `AssetDatabase.LoadAssetAtPath`; in builds, uses `Resources.Load` with a fallback to direct `File.ReadAllBytes` → `Texture2D.LoadImage`.

[SpriteGeneratorEditor.cs](Assets/Scripts/Editor/SpriteGeneratorEditor.cs) is an `EditorWindow` that procedurally generates all game sprites (snake head with expressions, body with patterns, multiple food types, brick walls, grass background tiles) and writes them as PNGs to `Assets/Sprites/Generated/`.

### Directory Layout

```
Assets/
  Scripts/
    Core/         EventBus, EventTypes, GameConfig, GameServices
    Runtime/      SnakeGameBootstrap, SnakeSpriteLoader
    Managers/     GameManager, InputManager, ScoreManager
    Snake/        SnakeController, SnakeMovement, SnakeSegment
    Grid/         GridManager, GridBackgroundRenderer, GridWallRenderer
    Food/         FoodSpawner
    UI/           UIManager, MainMenuView, GameHudView, PauseView, GameOverView
    Editor/       SpriteGeneratorEditor
  Sprites/Generated/   Procedurally generated PNG sprites
  Config/GameConfig.asset   ScriptableObject configuration
  Scenes/SampleScene.unity  Single scene
```

### Key Conventions

- Components register in `Awake()`, resolve dependencies in `Start()`, unregister in `OnDestroy()`.
- Direct inter-component calls use `GameServices.Get<T>()`; cross-cutting notifications use `EventBus.Publish/Subscribe`.
- All event types are structs for zero-GC allocation.
- `SnakeMovement` is a plain C# class — the only non-MonoBehaviour logic class. It can be unit-tested without the Unity runtime.
- `ScoreManager` persists high score via `PlayerPrefs`.
- Prefabs and sprites are created at runtime if not assigned via the Inspector (`EnsureDefaults()` / `CreateFallbackUI()` patterns), making the game self-contained without requiring pre-built prefabs.
