using System.Collections;
using UnityEngine;

public class EnemyBridgeBuilder
{
    private readonly Enemy enemy;
    private readonly EnemyMovement movement;

    private Bridge targetBridge;
    private Vector3 buildPoint;
    private Vector3 buildMoveDirection;

    public EnemyBridgeBuilder(Enemy enemy, EnemyMovement movement)
    {
        this.enemy = enemy;
        this.movement = movement;
        Reset(enemy.transform.position);
    }

    public Bridge TargetBridge => targetBridge;
    public bool IsAtBuildPoint => movement.IsAtPosition(buildPoint);
    public bool IsNearDestination => movement.IsNearDestination;

    public void Reset(Vector3 position)
    {
        targetBridge = null;
        buildPoint = position;
        buildMoveDirection = Vector3.zero;
    }

    public bool CanReachBridge(Bridge bridge)
    {
        return bridge != null && movement.CanReach(bridge.GetBridgeEntryPosition(), 0.2f);
    }

    public bool TryPrepareBuild(StageController stage)
    {
        ReleaseReservation();
        targetBridge = stage.GetBestBridgeForEnemy(enemy.transform.position, enemy);
        Bridge bridge = targetBridge;

        if (bridge == null || !bridge.TryReserveEnemy(enemy)) return FailPrepare();

        buildPoint = bridge.GetBridgeEntryPosition();
        if (!movement.CanReach(buildPoint, 0.2f))
        {
            ReleaseReservation();
            return FailPrepare();
        }

        if (movement.TrySamplePosition(buildPoint, 0.2f, out Vector3 sampledPoint)) buildPoint = sampledPoint;

        buildMoveDirection = bridge.GetBuildMoveDirection();
        movement.SetDestination(buildPoint);
        return true;
    }

    public void ReturnToBuildPoint() => movement.SetDestination(buildPoint);
    public void MoveForward() => movement.MoveManually(buildMoveDirection);
    public void MoveBack() => movement.MoveManually(-buildMoveDirection);

    public IEnumerator CrossBridgeRoutine(Bridge bridge, StageController nextStage, BrickSpawner targetSpawner)
    {
        movement.SetCrossingBridge(true);
        movement.DisableAgentMovement();

        Vector3 targetPosition = bridge.GetBridgeEndPosition();
        while (Flatten(targetPosition - enemy.transform.position).sqrMagnitude > 0.05f * 0.05f)
        {
            if (GameManager.Instance != null && GameManager.Instance.IsPaused)
            {
                movement.SyncNextPosition();
                yield return null;
                continue;
            }

            movement.MoveManually(Flatten(targetPosition - enemy.transform.position), 3.5f);
            yield return null;
        }

        enemy.transform.position = new Vector3(targetPosition.x, enemy.transform.position.y, targetPosition.z);
        enemy.SetCurrentStage(nextStage, targetSpawner);
        movement.EnableAgentMovement();
        movement.SetCrossingBridge(false);
        movement.SetTransformDrivenMovement(false);
        enemy.ChangeState(new FindBrickState(enemy));
    }

    public void ReleaseReservation()
    {
        targetBridge?.ReleaseEnemy(enemy);
        targetBridge = null;
    }

    private bool FailPrepare()
    {
        movement.StopAgentAtCurrentPosition();
        return false;
    }

    private static Vector3 Flatten(Vector3 vector)
    {
        vector.y = 0f;
        return vector;
    }
}
