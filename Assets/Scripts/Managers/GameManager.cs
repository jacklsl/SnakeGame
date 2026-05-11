using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 游戏状态枚举
/// </summary>
public enum GameState
{
    Ready,      // 待开始
    Playing,    // 进行中
    Paused,     // 暂停
    GameOver    // 结束
}

/// <summary>
/// 游戏管理器 - 管理游戏状态和整体流程
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("场景名称")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string gameSceneName = "Game";

    private GameState currentState = GameState.Ready;

    private SnakeController snakeController;
    private FoodSpawner foodSpawner;
    private ScoreManager scoreManager;
    private UIManager uiManager;

    public GameState CurrentState => currentState;

    // 事件
    public System.Action<GameState> OnGameStateChanged;

    private void Awake()
    {
        snakeController = FindObjectOfType<SnakeController>();
        foodSpawner = FindObjectOfType<FoodSpawner>();
        scoreManager = FindObjectOfType<ScoreManager>();
        uiManager = FindObjectOfType<UIManager>();
    }

    private void Start()
    {
        // 注册事件
        if (snakeController != null)
        {
            snakeController.OnFoodEaten += OnFoodEaten;
            snakeController.OnDeath += OnSnakeDeath;
            snakeController.OnSnakeMoved += OnSnakeMoved;
        }

        // 初始状态为Ready
        SetState(GameState.Ready);
    }

    private void OnDestroy()
    {
        if (snakeController != null)
        {
            snakeController.OnFoodEaten -= OnFoodEaten;
            snakeController.OnDeath -= OnSnakeDeath;
            snakeController.OnSnakeMoved -= OnSnakeMoved;
        }
    }

    /// <summary>
    /// 设置游戏状态
    /// </summary>
    public void SetState(GameState newState)
    {
        currentState = newState;
        OnGameStateChanged?.Invoke(currentState);

        switch (currentState)
        {
            case GameState.Ready:
                Time.timeScale = 1f;
                break;
            case GameState.Playing:
                Time.timeScale = 1f;
                break;
            case GameState.Paused:
                Time.timeScale = 0f;
                break;
            case GameState.GameOver:
                Time.timeScale = 0f;
                break;
        }
    }

    /// <summary>
    /// 开始游戏
    /// </summary>
    public void StartGame()
    {
        Debug.Log("[GameManager] StartGame() called - currentState: " + currentState);

        // 重置分数
        if (scoreManager != null)
            scoreManager.ResetScore();
        else
            Debug.LogError("[GameManager] scoreManager is NULL!");

        // 重置蛇
        if (snakeController != null)
        {
            Debug.Log("[GameManager] Calling snakeController.ResetSnake() and StartMoving()");
            snakeController.ResetSnake();
            snakeController.StartMoving();
        }
        else
        {
            Debug.LogError("[GameManager] snakeController is NULL!");
        }

        // 重置食物
        if (foodSpawner != null)
            foodSpawner.ResetFood();
        else
            Debug.LogError("[GameManager] foodSpawner is NULL!");

        SetState(GameState.Playing);
        Debug.Log("[GameManager] StartGame() completed - new state: " + currentState);
    }

    /// <summary>
    /// 暂停游戏
    /// </summary>
    public void PauseGame()
    {
        if (currentState == GameState.Playing)
        {
            SetState(GameState.Paused);
        }
    }

    /// <summary>
    /// 继续游戏
    /// </summary>
    public void ResumeGame()
    {
        if (currentState == GameState.Paused)
        {
            SetState(GameState.Playing);
        }
    }

    /// <summary>
    /// 重新开始游戏
    /// </summary>
    public void RestartGame()
    {
        StartGame();
    }

    /// <summary>
    /// 返回主菜单
    /// </summary>
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        if (SceneExists(mainMenuSceneName))
            SceneManager.LoadScene(mainMenuSceneName);
        else
            SetState(GameState.Ready);
    }

    /// <summary>
    /// 退出游戏
    /// </summary>
    public void QuitGame()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>
    /// 加载游戏场景
    /// </summary>
    public void LoadGameScene()
    {
        Time.timeScale = 1f;
        if (SceneExists(gameSceneName))
            SceneManager.LoadScene(gameSceneName);
        else
            StartGame();
    }

    /// <summary>
    /// 蛇移动回调 - 检测是否吃到食物
    /// </summary>
    private void OnSnakeMoved(Vector2Int headPos)
    {
        if (foodSpawner != null && foodSpawner.IsFoodAt(headPos))
        {
            foodSpawner.EatFood();
        }
    }

    /// <summary>
    /// 吃到食物回调
    /// </summary>
    private void OnFoodEaten()
    {
        // 增加分数
        if (scoreManager != null)
            scoreManager.AddScore(1);

        // 播放音效（可选）
        // AudioManager.PlayEatSound();
    }

    /// <summary>
    /// 蛇死亡回调
    /// </summary>
    private void OnSnakeDeath()
    {
        SetState(GameState.GameOver);

        // 播放音效（可选）
        // AudioManager.PlayGameOverSound();
    }

    private string[] cachedSceneNames;
    private bool sceneNamesCached;

    private bool SceneExists(string sceneName)
    {
        if (!sceneNamesCached)
        {
            int sceneCount = SceneManager.sceneCountInBuildSettings;
            cachedSceneNames = new string[sceneCount];
            for (int i = 0; i < sceneCount; i++)
            {
                string path = SceneUtility.GetScenePathByBuildIndex(i);
                cachedSceneNames[i] = System.IO.Path.GetFileNameWithoutExtension(path);
            }
            sceneNamesCached = true;
        }

        for (int i = 0; i < cachedSceneNames.Length; i++)
        {
            if (cachedSceneNames[i] == sceneName)
                return true;
        }

        return false;
    }
}
