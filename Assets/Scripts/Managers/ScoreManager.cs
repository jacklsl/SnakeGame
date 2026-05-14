using UnityEngine;

/// <summary>
/// 计分管理器 — 管理得分和最高分，通过 EventBus 发布分数变化
/// </summary>
public class ScoreManager : MonoBehaviour
{
    private const string HIGH_SCORE_KEY = "SnakeGame_HighScore";

    private int currentScore;
    private int highScore;

    public int CurrentScore => currentScore;
    public int HighScore => highScore;
    public bool IsNewRecord { get; private set; }

    private void Awake()
    {
        LoadHighScore();
        GameServices.Register(this);
    }

    public void AddScore(int points = 1)
    {
        currentScore += points;
        IsNewRecord = false;

        if (currentScore > highScore)
        {
            highScore = currentScore;
            IsNewRecord = true;
            SaveHighScore();
        }

        EventBus.Publish(new ScoreChangedEvent
        {
            Score = currentScore,
            HighScore = highScore,
            IsNewRecord = IsNewRecord
        });
    }

    public void ResetScore()
    {
        currentScore = 0;
        IsNewRecord = false;
        LoadHighScore();
        EventBus.Publish(new ScoreChangedEvent
        {
            Score = currentScore,
            HighScore = highScore,
            IsNewRecord = false
        });
    }

    private void LoadHighScore()
    {
        highScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
    }

    private void SaveHighScore()
    {
        PlayerPrefs.SetInt(HIGH_SCORE_KEY, highScore);
        PlayerPrefs.Save();
    }

    private void OnDestroy()
    {
        GameServices.Unregister<ScoreManager>();
    }
}
