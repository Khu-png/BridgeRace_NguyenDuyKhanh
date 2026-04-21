using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    private const float MenuLevelTextY = 600f;

    [Header("Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject settingPanel;
    [SerializeField] private GameObject pauseButton;
    [SerializeField] private GameObject joystick;

    [Header("Audio")]
    public Slider _musicSlider;
    public Slider _sfxSlider;

    [Header("UI Text")]
    [SerializeField] private TextMeshProUGUI levelText;

    private RectTransform levelTextRect;
    private Vector2 defaultLevelTextAnchoredPosition;
    private bool hasCachedLevelTextPosition;

    private void Start()
    {
        CacheLevelTextPosition();
        InitializeAudioSliders();

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
        if (settingPanel != null) settingPanel.SetActive(false);
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

    public void OnClickSetting()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.MainMenu)
        {
            return;
        }

        if (settingPanel != null)
        {
            settingPanel.SetActive(true);
        }
    }

    public void OnClickExitSetting()
    {
        if (settingPanel != null)
        {
            settingPanel.SetActive(false);
        }
    }

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

    public void SetMusicVolume()
    {
        AudioManager.Instance?.SetMusicVolume(_musicSlider != null ? _musicSlider.value : 1f);
    }

    public void SetSFXVolume()
    {
        AudioManager.Instance?.SetSFXVolume(_sfxSlider != null ? _sfxSlider.value : 1f);
    }

    public void SetMusicVolume(float volume)
    {
        AudioManager.Instance?.SetMusicVolume(volume);
    }

    public void SetSFXVolume(float volume)
    {
        AudioManager.Instance?.SetSFXVolume(volume);
    }

    private void InitializeAudioSliders()
    {
        if (_musicSlider != null)
        {
            _musicSlider.onValueChanged.RemoveListener(SetMusicVolume);
            _musicSlider.onValueChanged.AddListener(SetMusicVolume);
            SetMusicVolume(_musicSlider.value);
        }

        if (_sfxSlider != null)
        {
            _sfxSlider.onValueChanged.RemoveListener(SetSFXVolume);
            _sfxSlider.onValueChanged.AddListener(SetSFXVolume);
            SetSFXVolume(_sfxSlider.value);
        }
    }
}
