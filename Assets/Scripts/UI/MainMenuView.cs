using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private Button startButton;
    [SerializeField] private Button quitButton;

    private void Awake()
    {
        if (titleText == null) CreateFallbackUI();
    }

    private void Start()
    {
        if (startButton != null) startButton.onClick.AddListener(OnStartClicked);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuitClicked);
        EventBus.Subscribe<ScoreChangedEvent>(OnScoreChanged);
    }

    public void Show()
    {
        gameObject.SetActive(true);
        ScoreManager sm = GameServices.Get<ScoreManager>();
        if (highScoreText != null && sm != null)
            highScoreText.text = $"Best: {sm.HighScore}";
    }

    public void Hide() { gameObject.SetActive(false); }

    private void OnStartClicked() => GameServices.Get<GameManager>()?.StartGame();
    private void OnQuitClicked() => GameServices.Get<GameManager>()?.QuitGame();

    private void OnScoreChanged(ScoreChangedEvent evt)
    {
        if (highScoreText != null)
            highScoreText.text = $"Best: {evt.HighScore}";
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<ScoreChangedEvent>(OnScoreChanged);
    }

    private void CreateFallbackUI()
    {
        EnsurePanelBackground(new Color(0.05f, 0.09f, 0.08f, 0.96f));

        titleText = CreateText("Title", "Snake", 92, TextAlignmentOptions.Center);
        SetRect(titleText.rectTransform, 0.5f, 0.65f, 0.5f, 0.65f, 0, 0, 900, 140);

        highScoreText = CreateText("HighScore", "Best: 0", 36, TextAlignmentOptions.Center);
        SetRect(highScoreText.rectTransform, 0.5f, 0.52f, 0.5f, 0.52f, 0, 0, 500, 70);

        startButton = CreateButton("StartBtn", "Start Game");
        SetRect(startButton.GetComponent<RectTransform>(), 0.5f, 0.38f, 0.5f, 0.38f, 0, 0, 320, 86);

        quitButton = CreateButton("QuitBtn", "Quit");
        SetRect(quitButton.GetComponent<RectTransform>(), 0.5f, 0.26f, 0.5f, 0.26f, 0, 0, 320, 86);
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
