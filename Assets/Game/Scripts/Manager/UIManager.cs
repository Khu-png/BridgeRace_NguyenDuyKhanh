using TMPro;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    private const float MenuLevelTextY = 600f;

    [Header("Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject pauseButton;
    [SerializeField] private GameObject joystick;

    [Header("UI Text")]
    [SerializeField] private TextMeshProUGUI levelText;

    private RectTransform levelTextRect;
    private Vector2 defaultLevelTextAnchoredPosition;
    private bool hasCachedLevelTextPosition;

    private void Start()
    {
        CacheLevelTextPosition();

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

        RestoreDefaultLevelTextPosition();

        if (levelText != null)
        {
            levelText.gameObject.SetActive(true);
        }

        if (pauseButton != null)
        {
            pauseButton.SetActive(false);
        }

        if (joystick != null)
        {
            joystick.SetActive(false);
        }
    }

    public void UILose()
    {
        if (losePanel != null)
        {
            losePanel.SetActive(true);
        }

        RestoreDefaultLevelTextPosition();

        if (levelText != null)
        {
            levelText.gameObject.SetActive(true);
        }

        if (pauseButton != null)
        {
            pauseButton.SetActive(false);
        }

        if (joystick != null)
        {
            joystick.SetActive(false);
        }
    }

    public void UIPause()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        if (levelText != null)
        {
            levelText.gameObject.SetActive(false);
        }

        if (pauseButton != null)
        {
            pauseButton.SetActive(false);
        }

        if (joystick != null)
        {
            joystick.SetActive(false);
        }
    }

    public void ResetUI()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
        if (pauseButton != null) pauseButton.SetActive(false);
    }

    public void UIPlay()
    {
        RestoreDefaultLevelTextPosition();
        if (menuPanel != null) menuPanel.SetActive(false);
        if (levelText != null) levelText.gameObject.SetActive(false);
        if (pauseButton != null) pauseButton.SetActive(true);
        if (joystick != null) joystick.SetActive(true);
    }

    public void UIMenu()
    {
        ResetUI();
        if (menuPanel != null) menuPanel.SetActive(true);
        SetMenuLevelTextPosition();
        if (levelText != null) levelText.gameObject.SetActive(true);
        if (pauseButton != null) pauseButton.SetActive(false);
        if (joystick != null) joystick.SetActive(false);
    }

    public void OnClickStart() => GameManager.Instance.GameStart();

    public void OnClickMenu()
    {
        GameManager.Instance.GameMenu();
    }

    public void OnClickPause() => GameManager.Instance.GamePause();
    public void OnClickResume() => GameManager.Instance.GameResume();
    public void OnClickNext() => GameManager.Instance.GameNextLevel();

    public void OnClickRestart()
    {
        GameManager.Instance.GameRestart();
    }

    public void OnClickResetToLevel1()
    {
        LevelManager.Instance?.ResetToLevel1();
        GameManager.Instance?.GameMenu();
    }

    private void CacheLevelTextPosition()
    {
        if (hasCachedLevelTextPosition || levelText == null)
        {
            return;
        }

        levelTextRect = levelText.rectTransform;
        if (levelTextRect == null)
        {
            return;
        }

        defaultLevelTextAnchoredPosition = levelTextRect.anchoredPosition;
        hasCachedLevelTextPosition = true;
    }

    private void RestoreDefaultLevelTextPosition()
    {
        CacheLevelTextPosition();

        if (levelTextRect == null)
        {
            return;
        }

        levelTextRect.anchoredPosition = defaultLevelTextAnchoredPosition;
    }

    private void SetMenuLevelTextPosition()
    {
        CacheLevelTextPosition();

        if (levelTextRect == null)
        {
            return;
        }

        Vector2 menuPosition = defaultLevelTextAnchoredPosition;
        menuPosition.y = MenuLevelTextY;
        levelTextRect.anchoredPosition = menuPosition;
    }
}
