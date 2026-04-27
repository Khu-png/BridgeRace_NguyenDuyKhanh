using System.Collections;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    private const string FallBrickPoolKey = "FallBrick";

    public enum GameState { MainMenu, Playing, Paused, Win, Lose }

    private static GameState gameState;
    private GameState resumeState = GameState.MainMenu;
    private bool isChangingLevel;
    private Coroutine levelChangeRoutine;

    public GameState CurrentState => gameState;
    public bool IsPlaying => IsState(GameState.Playing);
    public bool IsPaused => IsState(GameState.Paused);

    protected override void Awake()
    {
        base.Awake();
        Input.multiTouchEnabled = false;
        Application.targetFrameRate = 60;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        const int maxScreenHeight = 1280;
        float ratio = (float)Screen.currentResolution.width / Screen.currentResolution.height;
        if (Screen.currentResolution.height > maxScreenHeight)
        {
            Screen.SetResolution(Mathf.RoundToInt(ratio * maxScreenHeight), maxScreenHeight, true);
        }
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        EnterState(GameState.MainMenu, false);
        OpenMainMenu();
    }

    public static void ChangeState(GameState state) => gameState = state;
    public static bool IsState(GameState state) => gameState == state;

    public void GameStart()
    {
        StopPendingLevelChange();
        EnterState(GameState.Playing, true);
        UIManager.Instance?.CloseAll();
        ShowGameplayUI();
    }

    public void GamePause()
    {
        if (IsState(GameState.Paused))
        {
            return;
        }

        resumeState = gameState;
        EnterState(GameState.Paused, false, false);
        LevelManager.Instance?.SetGameplayActorsPaused(true);
        UIManager.Instance?.CloseAll();
        UIManager.Instance?.OpenUI<Pause>();
    }

    public void GameResume()
    {
        EnterState(resumeState, resumeState != GameState.MainMenu);
        UIManager.Instance?.CloseAll();

        if (resumeState == GameState.MainMenu)
        {
            OpenMainMenu();
            return;
        }

        LevelManager.Instance?.SetGameplayActorsPaused(false);
        ShowGameplayUI();
    }

    public void GameWin()
    {
        if (IsState(GameState.Win) || isChangingLevel)
        {
            return;
        }

        ShowResult<Win>(GameState.Win);
    }

    public void GameLose()
    {
        StopPendingLevelChange();
        ShowResult<Lose>(GameState.Lose);
    }

    public void GameRestart()
    {
        StopPendingLevelChange();
        ClearGameplayObjects();
        EnterState(GameState.Playing, true);
        UIManager.Instance?.CloseAll();
        LevelManager.Instance?.OnReplay();
        ShowGameplayUI();
    }

    public void GameNextLevel()
    {
        if (!IsState(GameState.Win) || isChangingLevel)
        {
            return;
        }

        isChangingLevel = true;
        levelChangeRoutine = StartCoroutine(GameNextLevelRoutine());
    }

    public void GameMenu()
    {
        StopPendingLevelChange();
        ClearGameplayObjects();
        EnterState(GameState.MainMenu, false);
        LevelManager.Instance?.OnReplay();
        OpenMainMenu();
    }

    public void GameResetLevel()
    {
        StopPendingLevelChange();
        ClearGameplayObjects();
        EnterState(GameState.MainMenu, false);
        LevelManager.Instance?.ResetToLevel1();
        OpenMainMenu();
    }

    private IEnumerator GameNextLevelRoutine()
    {
        yield return new WaitForSeconds(1);

        UIManager.Instance?.CloseAll();
        LevelManager.Instance?.OnNextLevel();
        isChangingLevel = false;
        levelChangeRoutine = null;
        EnterState(GameState.MainMenu, false);
        OpenMainMenu();
    }

    private void EnterState(GameState state, bool canMove, bool updateResumeState = true)
    {
        if (updateResumeState)
        {
            resumeState = state;
        }

        ChangeState(state);
        Player.CanMove = canMove;
    }

    private void ShowResult<T>(GameState state) where T : UICanvas
    {
        EnterState(state, false);
        LevelManager.Instance?.SetGameplayActorsPaused(true);
        UIManager.Instance?.CloseAll();
        UIManager.Instance?.OpenUI<T>();
    }

    private void OpenMainMenu()
    {
        UIManager.Instance?.CloseAll();
        UIManager.Instance?.OpenUI<Mainmenu>();
    }

    private void StopPendingLevelChange()
    {
        if (levelChangeRoutine != null)
        {
            StopCoroutine(levelChangeRoutine);
            levelChangeRoutine = null;
        }

        isChangingLevel = false;
    }

    private void ShowGameplayUI()
    {
        if (LevelManager.Instance != null)
        {
            UIManager.Instance?.OpenUI<LevelText>()?.SetLevel(LevelManager.Instance.CurrentLevel + 1);
        }

        UIManager.Instance?.OpenUI<PauseButton>();
    }

    private void ClearGameplayObjects() => PoolManager.Instance?.ReturnAllActive(FallBrickPoolKey);
}
