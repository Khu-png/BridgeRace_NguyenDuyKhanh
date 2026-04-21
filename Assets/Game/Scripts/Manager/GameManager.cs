using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    private const string FallBrickPoolKey = "FallBrick";

    public enum GameState { MainMenu, Playing, Paused, Win, Lose }

    private GameState currentState;
    private GameState resumeState = GameState.MainMenu;
    public GameState CurrentState => currentState;
    public bool IsPlaying => currentState == GameState.Playing;
    public bool IsPaused => currentState == GameState.Paused;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        Application.targetFrameRate = 120;
        ChangeState(GameState.MainMenu);
        Player.CanMove = false;
        UIManager.Instance?.UIMenu();
    }

    public void GameStart()
    {
        bool isStartingFromMenu = currentState == GameState.MainMenu;

        resumeState = GameState.Playing;
        ChangeState(GameState.Playing);
        Player.CanMove = true;

        if (isStartingFromMenu)
        {
            LevelManager.Instance?.OnReplay();
        }

        UIManager.Instance?.ResetUI();
        UIManager.Instance?.UIPlay();
    }

    public void GamePause()
    {
        if (currentState == GameState.Paused)
        {
            return;
        }

        resumeState = currentState;
        ChangeState(GameState.Paused);
        Player.CanMove = false;
        FreezeGameplayActors();
        UIManager.Instance?.UIPause();
    }

    public void GameResume()
    {
        ChangeState(resumeState);
        UIManager.Instance?.ResetUI();

        if (resumeState == GameState.MainMenu)
        {
            Player.CanMove = false;
            UIManager.Instance?.UIMenu();
            return;
        }

        Player.CanMove = true;
        ResumeGameplayActors();
        UIManager.Instance?.UIPlay();
    }

    public void GameWin()
    {
        resumeState = GameState.Win;
        ChangeState(GameState.Win);
        Player.CanMove = false;
        FreezeGameplayActors();
        UIManager.Instance?.UIWin();
    }

    public void GameLose()
    {
        resumeState = GameState.Lose;
        ChangeState(GameState.Lose);
        Player.CanMove = false;
        FreezeGameplayActors();
        UIManager.Instance?.UILose();
    }

    public void GameRestart()
    {
        PoolManager.Instance?.ReturnAllActive(FallBrickPoolKey);
        resumeState = GameState.Playing;
        ChangeState(GameState.Playing);
        Player.CanMove = true;
        UIManager.Instance?.ResetUI();
        UIManager.Instance?.UIPlay();
        LevelManager.Instance?.OnReplay();
    }

    public void GameNextLevel()
    {
        UIManager.Instance?.ResetUI();
        LevelManager.Instance?.OnNextLevel();
        resumeState = GameState.MainMenu;
        ChangeState(GameState.MainMenu);
        Player.CanMove = false;
        UIManager.Instance?.UIMenu();
    }

    public void GameMenu()
    {
        PoolManager.Instance?.ReturnAllActive(FallBrickPoolKey);
        resumeState = GameState.MainMenu;
        ChangeState(GameState.MainMenu);
        LevelManager.Instance?.OnReplay();
        Player.CanMove = false;
        UIManager.Instance?.UIMenu();
    }

    private void ChangeState(GameState newState)
    {
        currentState = newState;
    }

    private void FreezeGameplayActors()
    {
        Player player = FindFirstObjectByType<Player>();
        player?.ResetMovementState();

        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (Enemy enemy in enemies)
        {
            enemy?.PauseMovement();
        }
    }

    private void ResumeGameplayActors()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (Enemy enemy in enemies)
        {
            enemy?.ResumeMovement();
        }
    }
}
