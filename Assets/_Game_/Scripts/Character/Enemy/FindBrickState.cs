using UnityEngine;

public class FindBrickState : IEnemyState
{
    private readonly Enemy enemy;

    public FindBrickState(Enemy enemy)
    {
        this.enemy = enemy;
    }

    public void OnEnter()
    {
        if (enemy.HasNoBricks())
        {
            enemy.RandomizeTargetBrickCount();
        }

        enemy.ResetRefreshTimer();
    }

    public void OnExecute()
    {
        if (enemy.ShouldMoveToGoal())
        {
            enemy.RefreshGoalTarget();
            return;
        }

        if (enemy.HasEnoughBricksToBuild())
        {
            enemy.ChangeState(new BuildBridgeState(enemy));
            return;
        }

        enemy.TickRefreshTimer();
        if (enemy.ShouldRefreshDestination())
        {
            enemy.RefreshBrickTarget();
            enemy.ResetRefreshCooldown();
        }
    }

    public void OnExit()
    {
    }
}
