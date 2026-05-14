using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameOverView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private TextMeshProUGUI newRecordText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;

    private void Awake()
    {
        if (scoreText == null) CreateFallbackUI();
    }

    private void Start()
    {
        if (restartButton != null) restartButton.onClick.AddListener(OnRestartClicked);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuitClicked);
        EventBus.Subscribe<ScoreChangedEvent>(OnScoreChanged);
    }

    public void Show()
    {
        gameObject.SetActive(true);
        ScoreManager sm = GameServices.Get<ScoreManager>();
        if (scoreText != null && sm != null)
            scoreText.text = $"Score: {sm.CurrentScore}";
        if (highScoreText != null && sm != null)
            highScoreText.text = $"Best: {sm.HighScore}";
        if (newRecordText != null && sm != null)
            newRecordText.gameObject.SetActive(sm.IsNewRecord);
    }

    public void Hide() { gameObject.SetActive(false); }

    private void OnScoreChanged(ScoreChangedEvent evt)
    {
        if (scoreText != null) scoreText.text = $"Score: {evt.Score}";
        if (highScoreText != null) highScoreText.text = $"Best: {evt.HighScore}";
        if (newRecordText != null) newRecordText.gameObject.SetActive(evt.IsNewRecord);
    }

    private void OnRestartClicked() => GameServices.Get<GameManager>()?.RestartGame();
    private void OnMainMenuClicked() => GameServices.Get<GameManager>()?.GoToMainMenu();
    private void OnQuitClicked() => GameServices.Get<GameManager>()?.QuitGame();

    private void OnDestroy()
    {
        EventBus.Unsubscribe<ScoreChangedEvent>(OnScoreChanged);
    }

    private void CreateFallbackUI()
    {
        EnsurePanelBackground(new Color(0.08f, 0.03f, 0.03f, 0.88f));

        TextMeshProUGUI heading = CreateText("Title", "Game Over", 64, TextAlignmentOptions.Center);
        SetRect(heading.rectTransform, 0.5f, 0.68f, 0.5f, 0.68f, 0, 0, 560, 90);

        scoreText = CreateText("Score", "Score: 0", 38, TextAlignmentOptions.Center);
        highScoreText = CreateText("HighScore", "Best: 0", 34, TextAlignmentOptions.Center);
        newRecordText = CreateText("NewRecord", "New Record!", 34, TextAlignmentOptions.Center);
        newRecordText.color = new Color(1f, 0.83f, 0.2f);

        SetRect(scoreText.rectTransform, 0.5f, 0.57f, 0.5f, 0.57f, 0, 0, 400, 60);
        SetRect(highScoreText.rectTransform, 0.5f, 0.50f, 0.5f, 0.50f, 0, 0, 400, 60);
        SetRect(newRecordText.rectTransform, 0.5f, 0.44f, 0.5f, 0.44f, 0, 0, 400, 60);

        restartButton = CreateButton("RestartBtn", "Restart");
        mainMenuButton = CreateButton("MenuBtn", "Main Menu");
        quitButton = CreateButton("QuitBtn", "Quit");

        SetRect(restartButton.GetComponent<RectTransform>(), 0.5f, 0.32f, 0.5f, 0.32f, 0, 0, 300, 72);
        SetRect(mainMenuButton.GetComponent<RectTransform>(), 0.5f, 0.22f, 0.5f, 0.22f, 0, 0, 300, 72);
        SetRect(quitButton.GetComponent<RectTransform>(), 0.5f, 0.12f, 0.5f, 0.12f, 0, 0, 300, 72);
    }

    private void EnsurePanelBackground(Color color)
    {
        Image image = GetComponent<Image>();
        if (image == null) image = gameObject.AddComponent<Image>();
        image.color = color;
    }

    private TextMeshProUGUI CreateText(string name, string value, int fontSize, TextAlignmentOptions align)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(transform, false);
        TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = align;
        text.enableWordWrapping = false;
        return text;
    }

    private Button CreateButton(string name, string label)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(transform, false);
        Image img = go.AddComponent<Image>();
        img.color = new Color(0.18f, 0.56f, 0.31f);
        Button btn = go.AddComponent<Button>();
        TextMeshProUGUI txt = CreateText("Label", label, 30, TextAlignmentOptions.Center);
        txt.rectTransform.anchorMin = Vector2.zero;
        txt.rectTransform.anchorMax = Vector2.one;
        txt.rectTransform.offsetMin = Vector2.zero;
        txt.rectTransform.offsetMax = Vector2.zero;
        return btn;
    }

    private void SetRect(RectTransform rect, float anchorMinX, float anchorMinY,
        float anchorMaxX, float anchorMaxY, float posX, float posY, float width, float height)
    {
        rect.anchorMin = new Vector2(anchorMinX, anchorMinY);
        rect.anchorMax = new Vector2(anchorMaxX, anchorMaxY);
        rect.anchoredPosition = new Vector2(posX, posY);
        rect.sizeDelta = new Vector2(width, height);
    }
}
