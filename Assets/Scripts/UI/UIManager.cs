using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI管理器 - 管理所有UI界面
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("主菜单界面")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private Button startButton;
    [SerializeField] private Button quitButton;

    [Header("游戏界面")]
    [SerializeField] private GameObject gamePanel;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI highScoreGameText;
    [SerializeField] private Button pauseButton;

    [Header("暂停界面")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartFromPauseButton;
    [SerializeField] private Button mainMenuFromPauseButton;
    [SerializeField] private Button quitFromPauseButton;

    [Header("游戏结束界面")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI gameOverScoreText;
    [SerializeField] private TextMeshProUGUI gameOverHighScoreText;
    [SerializeField] private TextMeshProUGUI newRecordText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitFromGameOverButton;

    private GameManager gameManager;
    private ScoreManager scoreManager;

    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
        scoreManager = FindObjectOfType<ScoreManager>();
        EnsureDefaultUI();
    }

    private void Start()
    {
        // 注册游戏状态变化事件
        if (gameManager != null)
        {
            gameManager.OnGameStateChanged += OnGameStateChanged;
        }

        // 注册分数变化事件
        if (scoreManager != null)
        {
            scoreManager.OnScoreChanged += OnScoreChanged;
            scoreManager.OnHighScoreChanged += OnHighScoreChanged;
        }

        // 注册按钮事件
        RegisterButtonEvents();

        // 初始显示主菜单
        ShowMainMenu();
    }

    private void OnDestroy()
    {
        if (gameManager != null)
            gameManager.OnGameStateChanged -= OnGameStateChanged;

        if (scoreManager != null)
        {
            scoreManager.OnScoreChanged -= OnScoreChanged;
            scoreManager.OnHighScoreChanged -= OnHighScoreChanged;
        }
    }

    /// <summary>
    /// 注册按钮点击事件
    /// </summary>
    private void RegisterButtonEvents()
    {
        if (startButton != null)
            startButton.onClick.AddListener(() => gameManager?.StartGame());

        if (quitButton != null)
            quitButton.onClick.AddListener(() => gameManager?.QuitGame());

        if (pauseButton != null)
            pauseButton.onClick.AddListener(() => gameManager?.PauseGame());

        if (resumeButton != null)
            resumeButton.onClick.AddListener(() => gameManager?.ResumeGame());

        if (restartFromPauseButton != null)
            restartFromPauseButton.onClick.AddListener(() => gameManager?.RestartGame());

        if (mainMenuFromPauseButton != null)
            mainMenuFromPauseButton.onClick.AddListener(() => gameManager?.GoToMainMenu());

        if (quitFromPauseButton != null)
            quitFromPauseButton.onClick.AddListener(() => gameManager?.QuitGame());

        if (restartButton != null)
            restartButton.onClick.AddListener(() => gameManager?.RestartGame());

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(() => gameManager?.GoToMainMenu());

        if (quitFromGameOverButton != null)
            quitFromGameOverButton.onClick.AddListener(() => gameManager?.QuitGame());
    }

    /// <summary>
    /// 游戏状态变化回调
    /// </summary>
    private void OnGameStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.Ready:
                ShowMainMenu();
                break;
            case GameState.Playing:
                ShowGamePanel();
                break;
            case GameState.Paused:
                ShowPausePanel();
                break;
            case GameState.GameOver:
                ShowGameOverPanel();
                break;
        }
    }

    /// <summary>
    /// 分数变化回调
    /// </summary>
    private void OnScoreChanged(int score)
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score}";
    }

    /// <summary>
    /// 最高分变化回调
    /// </summary>
    private void OnHighScoreChanged(int highScore)
    {
        if (highScoreGameText != null)
            highScoreGameText.text = $"Best: {highScore}";
        if (highScoreText != null)
            highScoreText.text = $"Best: {highScore}";
    }

    /// <summary>
    /// 显示主菜单
    /// </summary>
    private void ShowMainMenu()
    {
        SetAllPanelsInactive();
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
            if (highScoreText != null && scoreManager != null)
                highScoreText.text = $"Best: {scoreManager.HighScore}";
        }
    }

    /// <summary>
    /// 显示游戏界面
    /// </summary>
    private void ShowGamePanel()
    {
        SetAllPanelsInactive();
        if (gamePanel != null)
        {
            gamePanel.SetActive(true);
            // 更新分数显示
            if (scoreText != null && scoreManager != null)
                scoreText.text = $"Score: {scoreManager.CurrentScore}";
            if (highScoreGameText != null && scoreManager != null)
                highScoreGameText.text = $"Best: {scoreManager.HighScore}";
        }
    }

    /// <summary>
    /// 显示暂停界面
    /// </summary>
    private void ShowPausePanel()
    {
        if (pausePanel != null)
            pausePanel.SetActive(true);
    }

    /// <summary>
    /// 显示游戏结束界面
    /// </summary>
    private void ShowGameOverPanel()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (gameOverScoreText != null && scoreManager != null)
                gameOverScoreText.text = $"Score: {scoreManager.CurrentScore}";
            if (gameOverHighScoreText != null && scoreManager != null)
                gameOverHighScoreText.text = $"Best: {scoreManager.HighScore}";

            // 显示新纪录提示
            if (newRecordText != null && scoreManager != null)
                newRecordText.gameObject.SetActive(scoreManager.IsNewRecord);
        }
    }

    /// <summary>
    /// 隐藏所有面板
    /// </summary>
    private void SetAllPanelsInactive()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (gamePanel != null) gamePanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    private void EnsureDefaultUI()
    {
        if (mainMenuPanel != null && gamePanel != null && pausePanel != null && gameOverPanel != null)
            return;

        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("Snake UI Canvas");
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObject.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        if (mainMenuPanel == null)
            CreateMainMenu(canvas.transform);
        if (gamePanel == null)
            CreateGameHud(canvas.transform);
        if (pausePanel == null)
            CreatePauseMenu(canvas.transform);
        if (gameOverPanel == null)
            CreateGameOverMenu(canvas.transform);
    }

    private void CreateMainMenu(Transform parent)
    {
        mainMenuPanel = CreatePanel("Main Menu Panel", parent, new Color(0.05f, 0.09f, 0.08f, 0.96f));
        titleText = CreateText("Title", mainMenuPanel.transform, "Snake", 92, TextAlignmentOptions.Center);
        SetRect(titleText.rectTransform, new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.65f), new Vector2(0f, 0f), new Vector2(900f, 140f));

        highScoreText = CreateText("High Score", mainMenuPanel.transform, "Best: 0", 36, TextAlignmentOptions.Center);
        SetRect(highScoreText.rectTransform, new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), Vector2.zero, new Vector2(500f, 70f));

        startButton = CreateButton("Start Button", mainMenuPanel.transform, "Start Game");
        SetRect(startButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.38f), new Vector2(0.5f, 0.38f), Vector2.zero, new Vector2(320f, 86f));

        quitButton = CreateButton("Quit Button", mainMenuPanel.transform, "Quit");
        SetRect(quitButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.26f), new Vector2(0.5f, 0.26f), Vector2.zero, new Vector2(320f, 86f));
    }

    private void CreateGameHud(Transform parent)
    {
        gamePanel = CreatePanel("Game HUD", parent, new Color(0f, 0f, 0f, 0f));
        scoreText = CreateText("Score", gamePanel.transform, "Score: 0", 34, TextAlignmentOptions.Left);
        SetRect(scoreText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(190f, -55f), new Vector2(320f, 60f));

        highScoreGameText = CreateText("High Score", gamePanel.transform, "Best: 0", 34, TextAlignmentOptions.Right);
        SetRect(highScoreGameText.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-245f, -55f), new Vector2(360f, 60f));

        pauseButton = CreateButton("Pause Button", gamePanel.transform, "Pause");
        SetRect(pauseButton.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-85f, -55f), new Vector2(120f, 56f));
    }

    private void CreatePauseMenu(Transform parent)
    {
        pausePanel = CreatePanel("Pause Panel", parent, new Color(0.02f, 0.03f, 0.03f, 0.82f));
        TextMeshProUGUI heading = CreateText("Pause Title", pausePanel.transform, "Paused", 58, TextAlignmentOptions.Center);
        SetRect(heading.rectTransform, new Vector2(0.5f, 0.66f), new Vector2(0.5f, 0.66f), Vector2.zero, new Vector2(520f, 90f));

        resumeButton = CreateButton("Resume Button", pausePanel.transform, "Resume");
        restartFromPauseButton = CreateButton("Restart Button", pausePanel.transform, "Restart");
        mainMenuFromPauseButton = CreateButton("Menu Button", pausePanel.transform, "Main Menu");
        quitFromPauseButton = CreateButton("Quit Button", pausePanel.transform, "Quit");
        SetRect(resumeButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), Vector2.zero, new Vector2(300f, 72f));
        SetRect(restartFromPauseButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.42f), Vector2.zero, new Vector2(300f, 72f));
        SetRect(mainMenuFromPauseButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.32f), new Vector2(0.5f, 0.32f), Vector2.zero, new Vector2(300f, 72f));
        SetRect(quitFromPauseButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.22f), Vector2.zero, new Vector2(300f, 72f));
    }

    private void CreateGameOverMenu(Transform parent)
    {
        gameOverPanel = CreatePanel("Game Over Panel", parent, new Color(0.08f, 0.03f, 0.03f, 0.88f));
        TextMeshProUGUI heading = CreateText("Game Over Title", gameOverPanel.transform, "Game Over", 64, TextAlignmentOptions.Center);
        SetRect(heading.rectTransform, new Vector2(0.5f, 0.68f), new Vector2(0.5f, 0.68f), Vector2.zero, new Vector2(560f, 90f));

        gameOverScoreText = CreateText("Final Score", gameOverPanel.transform, "Score: 0", 38, TextAlignmentOptions.Center);
        gameOverHighScoreText = CreateText("Final High Score", gameOverPanel.transform, "Best: 0", 34, TextAlignmentOptions.Center);
        newRecordText = CreateText("New Record", gameOverPanel.transform, "New Record!", 34, TextAlignmentOptions.Center);
        newRecordText.color = new Color(1f, 0.83f, 0.2f);
        SetRect(gameOverScoreText.rectTransform, new Vector2(0.5f, 0.57f), new Vector2(0.5f, 0.57f), Vector2.zero, new Vector2(400f, 60f));
        SetRect(gameOverHighScoreText.rectTransform, new Vector2(0.5f, 0.50f), new Vector2(0.5f, 0.50f), Vector2.zero, new Vector2(400f, 60f));
        SetRect(newRecordText.rectTransform, new Vector2(0.5f, 0.44f), new Vector2(0.5f, 0.44f), Vector2.zero, new Vector2(400f, 60f));

        restartButton = CreateButton("Restart Button", gameOverPanel.transform, "Restart");
        mainMenuButton = CreateButton("Menu Button", gameOverPanel.transform, "Main Menu");
        quitFromGameOverButton = CreateButton("Quit Button", gameOverPanel.transform, "Quit");
        SetRect(restartButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.32f), new Vector2(0.5f, 0.32f), Vector2.zero, new Vector2(300f, 72f));
        SetRect(mainMenuButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.22f), Vector2.zero, new Vector2(300f, 72f));
        SetRect(quitFromGameOverButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.12f), new Vector2(0.5f, 0.12f), Vector2.zero, new Vector2(300f, 72f));
    }

    private GameObject CreatePanel(string panelName, Transform parent, Color color)
    {
        GameObject panel = new GameObject(panelName);
        panel.transform.SetParent(parent, false);
        RectTransform rectTransform = panel.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        Image image = panel.AddComponent<Image>();
        image.color = color;
        return panel;
    }

    private Button CreateButton(string buttonName, Transform parent, string label)
    {
        GameObject buttonObject = new GameObject(buttonName);
        buttonObject.transform.SetParent(parent, false);
        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.18f, 0.56f, 0.31f);
        Button button = buttonObject.AddComponent<Button>();

        TextMeshProUGUI text = CreateText("Label", buttonObject.transform, label, 30, TextAlignmentOptions.Center);
        RectTransform textRect = text.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        return button;
    }

    private TextMeshProUGUI CreateText(string textName, Transform parent, string value, int fontSize, TextAlignmentOptions alignment)
    {
        GameObject textObject = new GameObject(textName);
        textObject.transform.SetParent(parent, false);
        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = alignment;
        text.enableWordWrapping = false;

        // 尝试加载 TMP 默认字体资产，避免显示为方块
        TMPro.TMP_FontAsset defaultFont = Resources.Load<TMPro.TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (defaultFont != null)
            text.font = defaultFont;

        return text;
    }

    private void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;
    }
}
