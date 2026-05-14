using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 暂停视图 — 继续/重启/主菜单/退出按钮
/// </summary>
public class PauseView : MonoBehaviour
{
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;

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
}
