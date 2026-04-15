public class EnemyStateManager
{
    private IEnemyState currentState;

    public void ChangeState(IEnemyState newState)
    {
        currentState?.OnExit();
        currentState = newState;
        currentState?.OnEnter();
    }

    public void Execute()
    {
        currentState?.OnExecute();
    }
}
