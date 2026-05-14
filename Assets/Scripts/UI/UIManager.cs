using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI 管理器 — 订阅 GameStateChangedEvent，路由面板切换
/// View 类的创建和 UI 布局由各自的 View 脚本和 Prefab 负责
/// </summary>
public class UIManager : MonoBehaviour
{
    [SerializeField] private MainMenuView mainMenuView;
    [SerializeField] private GameHudView gameHudView;
    [SerializeField] private PauseView pauseView;
    [SerializeField] private GameOverView gameOverView;

    private void Awake()
    {
        if (mainMenuView == null || gameHudView == null || pauseView == null || gameOverView == null)
            CreateFallbackUI();

        EventBus.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
    }

    private void Start()
    {
        HideAll();
        mainMenuView?.Show();
    }

    private void OnGameStateChanged(GameStateChangedEvent evt)
    {
        HideAll();
        switch (evt.State)
        {
            case GameState.Ready:
                mainMenuView?.Show();
                break;
            case GameState.Playing:
                gameHudView?.Show();
                break;
            case GameState.Paused:
                pauseView?.Show();
                break;
            case GameState.GameOver:
                gameOverView?.Show();
                break;
        }
    }

    private void HideAll()
    {
        mainMenuView?.Hide();
        gameHudView?.Hide();
        pauseView?.Hide();
        gameOverView?.Hide();
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
    }

    /// <summary>
    /// 回退方案：当 View 未通过 Prefab 赋值时，代码创建 UI 面板
    /// </summary>
    private void CreateFallbackUI()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("Snake UI Canvas");
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObject.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        if (mainMenuView == null) mainMenuView = CreateViewOnCanvas<MainMenuView>(canvas.transform, "MainMenuPanel");
        if (gameHudView == null) gameHudView = CreateViewOnCanvas<GameHudView>(canvas.transform, "GameHudPanel");
        if (pauseView == null) pauseView = CreateViewOnCanvas<PauseView>(canvas.transform, "PausePanel");
        if (gameOverView == null) gameOverView = CreateViewOnCanvas<GameOverView>(canvas.transform, "GameOverPanel");
    }

    private T CreateViewOnCanvas<T>(Transform parent, string name) where T : MonoBehaviour
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rect = go.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        go.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0f);
        return go.AddComponent<T>();
    }
}
