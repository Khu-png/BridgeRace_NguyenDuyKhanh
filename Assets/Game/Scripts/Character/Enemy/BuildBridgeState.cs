public class BuildBridgeState : IEnemyState
{
    private readonly Enemy enemy;
    private bool hasReachedBuildPoint;
    private bool isMovingManually;

    public BuildBridgeState(Enemy enemy)
    {
        this.enemy = enemy;
    }
    // Todo : quá nhiều logic, bỏ enemey khỏi các States.  Làm như bài chép game về phân state. 
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

        if (enemy.TargetBridge == null)
        {
            enemy.ChangeState(new FindBrickState(enemy));
            return;
        }

        if (enemy.TargetBridge.IsFull())
        {
            Bridge completedBridge = enemy.TargetBridge;
            bool isBridgeCompleter = completedBridge.BridgeCompleter == enemy;

            if (hasReachedBuildPoint && isBridgeCompleter)
            {
                StageController nextStage = completedBridge.TargetStage;
                BrickSpawner targetSpawner = completedBridge.TargetSpawner;

                completedBridge.ReleaseEnemy(enemy);

                if (nextStage != null)
                {
                    enemy.CrossBridge(completedBridge, nextStage, targetSpawner);
                    return;
                }
            }

            if (hasReachedBuildPoint && enemy.IsTransformDrivenMovement && enemy.TryReturnFromBridgeInstantly(completedBridge))
            {
                enemy.SetBridgeBuildingState(false);
                enemy.SetTransformDrivenMovement(false);
                enemy.EnableAgentMovement();
                enemy.ChangeState(new FindBrickState(enemy));
            }
            else
            {
                enemy.SetBridgeBuildingState(false);
                enemy.SetTransformDrivenMovement(false);
                enemy.EnableAgentMovement();
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
        if (enemy.TargetBridge != null)
        {
            enemy.TargetBridge.ReleaseEnemy(enemy);
        }

        enemy.SetBridgeBuildingState(false);
        enemy.SetTransformDrivenMovement(false);
        enemy.EnableAgentMovement();
    }
}
