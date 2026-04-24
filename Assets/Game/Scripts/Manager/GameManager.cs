using System;
using System.Collections;
using System.Collections.Generic;
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
        // TODO : Player.OnInit();
        Player.CanMove = false;
        UIManager.Instance?.OpenUI<Mainmenu>();
    }

    public void GameStart()
    {
        bool isStartingFromMenu = currentState == GameState.MainMenu;

        resumeState = GameState.Playing;
        ChangeState(GameState.Playing);
        // TODO : Player.OnMove();
        Player.CanMove = true;

        UIManager.Instance?.CloseAll();

        if (isStartingFromMenu)
        {
            LevelManager.Instance?.OnReplay();
        }

        ShowGameplayUI();
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
        UIManager.Instance?.CloseAll();
        UIManager.Instance?.OpenUI<Pause>();
    }

    public void GameResume()
    {
        ChangeState(resumeState);
        UIManager.Instance?.CloseAll();

        if (resumeState == GameState.MainMenu)
        {
            Player.CanMove = false;
            UIManager.Instance?.OpenUI<Mainmenu>();
            return;
        }

        Player.CanMove = true;
        ResumeGameplayActors();
        ShowGameplayUI();
    }

    public void GameWin()
    {
        resumeState = GameState.Win;
        ChangeState(GameState.Win);
        Player.CanMove = false;
        FreezeGameplayActors();
        UIManager.Instance?.CloseAll();
        UIManager.Instance?.OpenUI<Win>();
    }

    public void GameLose()
    {
        resumeState = GameState.Lose;
        ChangeState(GameState.Lose);
        Player.CanMove = false;
        FreezeGameplayActors();
        UIManager.Instance?.CloseAll();
        UIManager.Instance?.OpenUI<Lose>();
    }

    public void GameRestart()
    {
        PoolManager.Instance?.ReturnAllActive(FallBrickPoolKey);
        resumeState = GameState.Playing;
        ChangeState(GameState.Playing);
        Player.CanMove = true;
        UIManager.Instance?.CloseAll();
        LevelManager.Instance?.OnReplay();
        ShowGameplayUI();
    }

    // TODO : Chỉnh lại ý nghĩa hàm.
    public void GameNextLevel()
    {
        StartCoroutine(GameNextLevelRoutine());
    }

    private IEnumerator GameNextLevelRoutine()
    {
        yield return new WaitForSeconds(1);

        UIManager.Instance?.CloseAll();
        LevelManager.Instance?.OnNextLevel();
        resumeState = GameState.MainMenu;
        ChangeState(GameState.MainMenu);
        Player.CanMove = false;
        UIManager.Instance?.OpenUI<Mainmenu>();
    }

    public void GameMenu()
    {
        PoolManager.Instance?.ReturnAllActive(FallBrickPoolKey);
        resumeState = GameState.MainMenu;
        ChangeState(GameState.MainMenu);
        LevelManager.Instance?.OnReplay();
        Player.CanMove = false;
        UIManager.Instance?.CloseAll();
        UIManager.Instance?.OpenUI<Mainmenu>();
    }

    private void ChangeState(GameState newState)
    {
        currentState = newState;
    }

    private void ShowGameplayUI()
    {
        if (LevelManager.Instance != null)
        {
            LevelText levelText = UIManager.Instance?.OpenUI<LevelText>();
            levelText?.SetLevel(LevelManager.Instance.CurrentLevel + 1);
        }

        UIManager.Instance?.OpenUI<PauseButton>();
    }

    private void FreezeGameplayActors()
    {
        //TODO : Không được dùng FindObjects vì có thể gây Lag nếu có quá nhiều đối tượng. => Tạo List Enemy để quản lý.
        Player player = FindFirstObjectByType<Player>();
        player?.ResetMovementState();

        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (Enemy enemy in enemies)
        {
            enemy?.PauseMovement();
        }
    }

    [SerializeField] Player player;
    List<Character> characters = new List<Character>();
    
    private void ResumeGameplayActors()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (Enemy enemy in enemies)
        {
            enemy?.ResumeMovement();
        }
    }
}
