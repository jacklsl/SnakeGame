using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 游戏结束视图 — 得分、最高分、新纪录提示、按钮
/// </summary>
public class GameOverView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private TextMeshProUGUI newRecordText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;

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
}
