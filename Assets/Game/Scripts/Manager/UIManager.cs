using TMPro;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    [Header("Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;
    [SerializeField] private GameObject menuPanel;

    [Header("Button")]
    [SerializeField] private GameObject pauseButton;

    [Header("UI Text")]
    [SerializeField] private TextMeshProUGUI levelText;

    private void Start()
    {
        if (pauseButton != null)
        {
            pauseButton.SetActive(false);
        }

        if (LevelManager.Instance != null)
        {
            UpdateLevelText(LevelManager.Instance.CurrentLevel + 1);
        }
    }

    public void UpdateLevelText(int currentLevel)
    {
        if (levelText != null)
        {
            levelText.text = "Level " + currentLevel;
        }
    }

    public void UIWin()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }
    }

    public void UILose()
    {
        if (losePanel != null)
        {
            losePanel.SetActive(true);
        }
    }

    public void UIPause()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
    }

    public void ResetUI()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
    }

    public void UIPlay()
    {
        if (menuPanel != null) menuPanel.SetActive(false);
        if (levelText != null) levelText.gameObject.SetActive(true);
        if (pauseButton != null) pauseButton.SetActive(true);
    }

    public void OnClickMenu()
    {
        ResetUI();

        if (menuPanel != null) menuPanel.SetActive(true);
        if (pauseButton != null) pauseButton.SetActive(false);
        if (levelText != null) levelText.gameObject.SetActive(false);

        GameManager.Instance.GameMenu();
    }

    public void OnClickPause() => GameManager.Instance.GamePause();
    public void OnClickResume() => GameManager.Instance.GameResume();
    public void OnClickNext() => GameManager.Instance.GameNextLevel();

    public void OnClickRestart()
    {
        GameManager.Instance.GameRestart();
    }
}
