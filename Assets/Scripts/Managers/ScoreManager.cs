using UnityEngine;

/// <summary>
/// 计分管理器 - 管理得分和最高分
/// </summary>
public class ScoreManager : MonoBehaviour
{
    private const string HIGH_SCORE_KEY = "SnakeGame_HighScore";

    private int currentScore;
    private int highScore;

    public int CurrentScore => currentScore;
    public int HighScore => highScore;
    public bool IsNewRecord { get; private set; }

    public System.Action<int> OnScoreChanged;
    public System.Action<int> OnHighScoreChanged;

    private void Awake()
    {
        LoadHighScore();
    }

    /// <summary>
    /// 增加分数
    /// </summary>
    public void AddScore(int points = 1)
    {
        currentScore += points;
        OnScoreChanged?.Invoke(currentScore);

        // 检查是否打破最高分
        if (currentScore > highScore)
        {
            highScore = currentScore;
            IsNewRecord = true;
            SaveHighScore();
            OnHighScoreChanged?.Invoke(highScore);
        }
    }

    /// <summary>
    /// 重置当前分数，并从持久化存储重新加载最高分
    /// </summary>
    public void ResetScore()
    {
        currentScore = 0;
        IsNewRecord = false;
        // 重新加载最高分，确保与其他会话保持一致
        LoadHighScore();
        OnScoreChanged?.Invoke(currentScore);
    }

    /// <summary>
    /// 加载最高分
    /// </summary>
    private void LoadHighScore()
    {
        highScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
    }

    /// <summary>
    /// 保存最高分
    /// </summary>
    private void SaveHighScore()
    {
        PlayerPrefs.SetInt(HIGH_SCORE_KEY, highScore);
        PlayerPrefs.Save();
    }
}