/// <summary>
/// 事件类型定义 — 所有事件均为 struct，零 GC 分配
/// </summary>

public struct FoodEatenEvent { }

public struct SnakeDiedEvent { }

public struct ScoreChangedEvent
{
    public int Score;
    public int HighScore;
    public bool IsNewRecord;
}

public struct GameStateChangedEvent
{
    public GameState State;
}
