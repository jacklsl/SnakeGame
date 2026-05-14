using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 游戏 HUD 视图 — 得分、最高分、暂停按钮
/// </summary>
public class GameHudView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private Button pauseButton;

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
}
