using UnityEngine;

/// <summary>
/// 游戏管理器 — 游戏状态机，通过 EventBus 广播状态变化
/// 通过 GameServices 获取依赖
/// </summary>
public class GameManager : MonoBehaviour
{
    private GameState currentState = GameState.Ready;

    public GameState CurrentState => currentState;

    private void Awake()
    {
        GameServices.Register(this);
    }

    private void Start()
    {
        EventBus.Subscribe<FoodEatenEvent>(OnFoodEaten);
        EventBus.Subscribe<SnakeDiedEvent>(OnSnakeDied);
        SetState(GameState.Ready);
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<FoodEatenEvent>(OnFoodEaten);
        EventBus.Unsubscribe<SnakeDiedEvent>(OnSnakeDied);
        GameServices.Unregister<GameManager>();
    }

    public void SetState(GameState newState)
    {
        currentState = newState;
        EventBus.Publish(new GameStateChangedEvent { State = currentState });

        Time.timeScale = (currentState == GameState.Paused || currentState == GameState.GameOver) ? 0f : 1f;
    }

    public void StartGame()
    {
        GameServices.Get<ScoreManager>()?.ResetScore();
        GameServices.Get<SnakeController>()?.ResetSnake();
        GameServices.Get<SnakeController>()?.StartMoving();
        GameServices.Get<FoodSpawner>()?.ResetFood();
        SetState(GameState.Playing);
    }

    public void PauseGame()
    {
        if (currentState == GameState.Playing)
            SetState(GameState.Paused);
    }

    public void ResumeGame()
    {
        if (currentState == GameState.Paused)
            SetState(GameState.Playing);
    }

    public void RestartGame()
    {
        StartGame();
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        GameServices.Get<SnakeController>()?.ResetSnake();
        SetState(GameState.Ready);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnFoodEaten(FoodEatenEvent evt)
    {
        GameServices.Get<ScoreManager>()?.AddScore(1);
    }

    private void OnSnakeDied(SnakeDiedEvent evt)
    {
        SetState(GameState.GameOver);
    }
}
