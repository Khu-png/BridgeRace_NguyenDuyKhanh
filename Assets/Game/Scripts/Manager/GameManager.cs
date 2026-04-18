using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public enum GameState { MainMenu, Playing, Paused, Win, Lose }

    private GameState currentState;
    public GameState CurrentState => currentState;

    private bool gameStart;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        Application.targetFrameRate = 120;
    }

    public void GameStart() { }

    public void GamePause() { }
    public void GameResume() { }

    public void GameWin() { }

    public void GameLose() { }

    public void GameRestart() { }

    public void GameNextLevel() { }

    public void GameMenu() { }

    private void ChangeState(GameState newState)
    {
        currentState = newState;
    }
}
