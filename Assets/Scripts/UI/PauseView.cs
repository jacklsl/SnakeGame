using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PauseView : MonoBehaviour
{
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;

    private void Awake()
    {
        if (resumeButton == null) CreateFallbackUI();
    }

    private void Start()
    {
        if (resumeButton != null) resumeButton.onClick.AddListener(OnResumeClicked);
        if (restartButton != null) restartButton.onClick.AddListener(OnRestartClicked);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuitClicked);
    }

    public void Show() { gameObject.SetActive(true); }
    public void Hide() { gameObject.SetActive(false); }

    private void OnResumeClicked() => GameServices.Get<GameManager>()?.ResumeGame();
    private void OnRestartClicked() => GameServices.Get<GameManager>()?.RestartGame();
    private void OnMainMenuClicked() => GameServices.Get<GameManager>()?.GoToMainMenu();
    private void OnQuitClicked() => GameServices.Get<GameManager>()?.QuitGame();

    private void CreateFallbackUI()
    {
        EnsurePanelBackground(new Color(0.02f, 0.03f, 0.03f, 0.82f));

        TextMeshProUGUI heading = CreateText("Title", "Paused", 58, TextAlignmentOptions.Center);
        SetRect(heading.rectTransform, 0.5f, 0.66f, 0.5f, 0.66f, 0, 0, 520, 90);

        resumeButton = CreateButton("ResumeBtn", "Resume");
        restartButton = CreateButton("RestartBtn", "Restart");
        mainMenuButton = CreateButton("MenuBtn", "Main Menu");
        quitButton = CreateButton("QuitBtn", "Quit");

        SetRect(resumeButton.GetComponent<RectTransform>(), 0.5f, 0.52f, 0.5f, 0.52f, 0, 0, 300, 72);
        SetRect(restartButton.GetComponent<RectTransform>(), 0.5f, 0.42f, 0.5f, 0.42f, 0, 0, 300, 72);
        SetRect(mainMenuButton.GetComponent<RectTransform>(), 0.5f, 0.32f, 0.5f, 0.32f, 0, 0, 300, 72);
        SetRect(quitButton.GetComponent<RectTransform>(), 0.5f, 0.22f, 0.5f, 0.22f, 0, 0, 300, 72);
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
