using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameHudView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private Button pauseButton;

    private void Awake()
    {
        if (scoreText == null) CreateFallbackUI();
    }

    private void Start()
    {
        if (pauseButton != null) pauseButton.onClick.AddListener(OnPauseClicked);
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
    }

    public void Hide() { gameObject.SetActive(false); }

    private void OnScoreChanged(ScoreChangedEvent evt)
    {
        if (scoreText != null) scoreText.text = $"Score: {evt.Score}";
        if (highScoreText != null) highScoreText.text = $"Best: {evt.HighScore}";
    }

    private void OnPauseClicked() => GameServices.Get<GameManager>()?.PauseGame();

    private void OnDestroy()
    {
        EventBus.Unsubscribe<ScoreChangedEvent>(OnScoreChanged);
    }

    private void CreateFallbackUI()
    {
        scoreText = CreateText("Score", "Score: 0", 34, TextAlignmentOptions.Left);
        SetRect(scoreText.rectTransform, 0, 1, 0, 1, 190, -55, 320, 60);

        highScoreText = CreateText("HighScore", "Best: 0", 34, TextAlignmentOptions.Right);
        SetRect(highScoreText.rectTransform, 1, 1, 1, 1, -245, -55, 360, 60);

        pauseButton = CreateButton("PauseBtn", "Pause");
        SetRect(pauseButton.GetComponent<RectTransform>(), 1, 1, 1, 1, -85, -55, 120, 56);
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
        TextMeshProUGUI txt = CreateTextOnParent("Label", label, 30, TextAlignmentOptions.Center, go.transform);
        txt.rectTransform.anchorMin = Vector2.zero;
        txt.rectTransform.anchorMax = Vector2.one;
        txt.rectTransform.offsetMin = Vector2.zero;
        txt.rectTransform.offsetMax = Vector2.zero;
        txt.raycastTarget = false;
        return btn;
    }

    private TextMeshProUGUI CreateTextOnParent(string name, string value, int fontSize,
        TextAlignmentOptions align, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = align;
        text.enableWordWrapping = false;
        return text;
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
