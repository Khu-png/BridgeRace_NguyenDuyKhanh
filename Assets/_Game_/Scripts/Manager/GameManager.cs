using System;
using System.Collections;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    private const string FallBrickPoolKey = "FallBrick";

    public enum GameState { MainMenu, Playing, Paused, Win, Lose }

    private static bool hasState;
    private static GameState gameState;
    private GameState resumeState = GameState.MainMenu;
    private bool isChangingLevel;
    private Coroutine levelChangeRoutine;
    private Coroutine startRoutine;

    public GameState CurrentState => gameState;
    public GameState ResumeState => resumeState;
    public bool IsPlaying => IsState(GameState.Playing);
    public bool IsPaused => IsState(GameState.Paused);
    public bool IsChangingLevel => isChangingLevel;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public static void ChangeState(GameState state, bool forceRefresh = false)
    {
        if (!forceRefresh && hasState && gameState == state)
        {
            return;
        }

        hasState = true;
        gameState = state;

        if (Instance != null)
        {
            Instance.OnStateChanged(state);
        }
    }

    public static bool IsState(GameState state) => gameState == state;

    public void GameWin()
    {
        if (IsState(GameState.Win) || isChangingLevel)
        {
            return;
        }

        ChangeState(GameState.Win);
        LevelManager.Instance?.SetGameplayActorsPaused(true);
    }

    public void GameLose()
    {
        StopPendingLevelChange();
        ChangeState(GameState.Lose);
        LevelManager.Instance?.SetGameplayActorsPaused(true);
    }

    public void GameBegin()
    {
        StopPendingLevelChange();
        ChangeState(GameState.MainMenu);
        LevelManager.Instance?.SetGameplayActorsPaused(true);
    }

    public void GameStart()
    {
        StopPendingLevelChange();
        UIManager.Instance?.CloseAll();
        ChangeState(GameState.MainMenu);
        LevelManager.Instance?.SetGameplayActorsPaused(true);
        startRoutine = StartCoroutine(GameStartRoutine());
    }

    public void GamePause()
    {
        if (IsState(GameState.Paused))
        {
            return;
        }

        resumeState = gameState;
        ChangeState(GameState.Paused);
        LevelManager.Instance?.SetGameplayActorsPaused(true);
    }

    public void GameResume()
    {
        ChangeState(resumeState);

        if (IsState(GameState.MainMenu))
        {
            return;
        }

        LevelManager.Instance?.SetGameplayActorsPaused(false);
    }

    public void GameRestart(bool startAfterRestart = false)
    {
        StopPendingLevelChange();
        ClearGameplayObjects();
        UIManager.Instance?.CloseAll();
        LevelManager.Instance?.OnReplay();
        LevelManager.Instance?.SetGameplayActorsPaused(true);
        ChangeState(GameState.MainMenu, true);

        if (startAfterRestart)
        {
            startRoutine = StartCoroutine(GameStartRoutine());
        }
    }

    public void GameNextLevel(Action onCompleted = null)
    {
        if (!IsState(GameState.Win) || isChangingLevel)
        {
            return;
        }

        isChangingLevel = true;
        levelChangeRoutine = StartCoroutine(GameNextLevelRoutine(onCompleted));
    }

    public void GameMenu(Action onCompleted = null)
    {
        StopPendingLevelChange();
        UIManager.Instance?.CloseAll();
        LevelManager.Instance?.SetGameplayActorsPaused(true);

        LevelManager.Instance?.PlayTransition(() =>
        {
            ClearGameplayObjects();
            LevelManager.Instance?.OnReplay();
            onCompleted?.Invoke();
            ChangeState(GameState.MainMenu);
        });
    }

    public void GameResetLevel(Action onCompleted = null)
    {
        StopPendingLevelChange();
        UIManager.Instance?.CloseAll();
        LevelManager.Instance?.SetGameplayActorsPaused(true);

        LevelManager.Instance?.PlayTransition(() =>
        {
            ClearGameplayObjects();
            LevelManager.Instance?.ResetToLevel1();
            onCompleted?.Invoke();
            ChangeState(GameState.MainMenu);
        });
    }

    private IEnumerator GameNextLevelRoutine(Action onCompleted)
    {
        yield return new WaitForSeconds(1);

        UIManager.Instance?.CloseAll();

        LevelManager.Instance?.PlayTransition(() =>
        {
            LevelManager.Instance?.OnNextLevel();
            onCompleted?.Invoke();
            ChangeState(GameState.MainMenu);
        }, () =>
        {
            isChangingLevel = false;
            levelChangeRoutine = null;
        });
    }

    private void StopPendingLevelChange()
    {
        if (startRoutine != null)
        {
            StopCoroutine(startRoutine);
            startRoutine = null;
            UIManager.Instance?.CloseUI<Counter>();
        }

        if (levelChangeRoutine != null)
        {
            StopCoroutine(levelChangeRoutine);
            levelChangeRoutine = null;
        }

        isChangingLevel = false;
    }

    private void ClearGameplayObjects() => PoolManager.Instance?.ReturnAllActive(FallBrickPoolKey);

    private IEnumerator GameStartRoutine()
    {
        Counter counter = UIManager.Instance?.OpenUI<Counter>();
        bool isCounterFinished = counter == null;

        if (counter != null)
        {
            counter.Play(() => isCounterFinished = true);
        }

        yield return new WaitUntil(() => isCounterFinished);

        ChangeState(GameState.Playing);
        LevelManager.Instance?.SetGameplayActorsPaused(false);
        LevelManager.Instance?.GrantPendingRewardedAds();
        startRoutine = null;
    }

    private void OnStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.MainMenu:
                UIManager.Instance?.CloseAll();
                UIManager.Instance?.OpenUI<Mainmenu>();
                break;

            case GameState.Playing:
                UIManager.Instance?.CloseAll();

                if (LevelManager.Instance != null)
                {
                    UIManager.Instance?.OpenUI<LevelText>()?.SetLevel(LevelManager.Instance.CurrentLevel + 1);
                }

                UIManager.Instance?.OpenUI<PauseButton>();
                break;

            case GameState.Paused:
                UIManager.Instance?.CloseAll();
                UIManager.Instance?.OpenUI<Pause>();
                break;

            case GameState.Win:
                UIManager.Instance?.CloseAll();
                UIManager.Instance?.OpenUI<Win>();
                break;

            case GameState.Lose:
                UIManager.Instance?.CloseAll();
                UIManager.Instance?.OpenUI<Lose>();
                break;
        }
    }
}
