using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 主菜单视图 — 标题、最高分、开始/退出按钮
/// </summary>
public class MainMenuView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private Button startButton;
    [SerializeField] private Button quitButton;

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
}
