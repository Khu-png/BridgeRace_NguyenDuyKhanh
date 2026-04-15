public class BuildBridgeState : IEnemyState
{
    private readonly Enemy enemy;
    private bool hasReachedBuildPoint;
    private bool isMovingManually;

    public BuildBridgeState(Enemy enemy)
    {
        this.enemy = enemy;
    }

    public void OnEnter()
    {
        hasReachedBuildPoint = false;
        isMovingManually = false;
        enemy.SetBridgeBuildingState(false);
        enemy.SetTransformDrivenMovement(false);

        if (!enemy.TryPrepareBuild())
        {
            enemy.SetBridgeBuildingState(false);
            enemy.ChangeState(new FindBrickState(enemy));
        }
    }

    public void OnExecute()
    {
        if (enemy.IsCrossingBridge) return;

        if (enemy.TargetBridgeWall == null)
        {
            enemy.ChangeState(new FindBrickState(enemy));
            return;
        }

        if (enemy.TargetBridgeWall.Bridge != null && enemy.TargetBridgeWall.Bridge.IsFull())
        {
            if (hasReachedBuildPoint || isMovingManually)
            {
                enemy.TargetBridgeWall.TryAdvance(enemy);
            }
            else
            {
                enemy.ChangeState(new FindBrickState(enemy));
            }
            return;
        }

        if (!hasReachedBuildPoint)
        {
            if (enemy.IsAtBuildPoint() || enemy.IsNearDestination())
            {
                enemy.DisableAgentMovement();
                hasReachedBuildPoint = true;
                isMovingManually = true;
                enemy.SetBridgeBuildingState(true);
                enemy.SetTransformDrivenMovement(true);
            }

            return;
        }

        if (enemy.HasNoBricks())
        {
            if (enemy.TrySnapToNavMesh())
            {
                enemy.SetBridgeBuildingState(false);
                enemy.SetTransformDrivenMovement(false);
                enemy.EnableAgentMovement();
                enemy.ChangeState(new FindBrickState(enemy));
            }
            else
            {
                enemy.MoveBackFromBridge();
            }
            return;
        }

        if (isMovingManually)
        {
            enemy.MoveForwardOnBridge();
        }
    }

    public void OnExit()
    {
        if (enemy.TargetBridgeWall != null)
        {
            enemy.TargetBridgeWall.ReleaseEnemySlot(enemy);
        }
        enemy.SetBridgeBuildingState(false);
        enemy.SetTransformDrivenMovement(false);
        enemy.EnableAgentMovement();
    }
}
