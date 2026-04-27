using System.Collections;
using NavMesh;
using UnityEngine;
[RequireComponent(typeof(UnityEngine.AI.NavMeshAgent), typeof(NavmeshAgent))]
public class Enemy : Character
{
    [Header("Enemy AI")]
    [SerializeField] private NavmeshAgent agent;
    [SerializeField] private int targetBrickCount = 5;
    [SerializeField] private float destinationRefreshInterval = 0.25f;
    [SerializeField] private float destinationReachedDistance = 0.2f;
    [SerializeField] private float rotationSpeed = 12f;
    [SerializeField] private float manualBridgeMoveSpeed = 3f;
    private EnemyStateManager stateManager;
    private BridgeWall targetBridgeWall;
    private Vector3 buildPoint;
    private Vector3 buildMoveDirection;
    private float refreshTimer;
    private bool isRunning;
    private bool isCrossingBridge;
    private bool didManualMoveThisFrame;
    private bool isTransformDrivenMovement;
    public BridgeWall TargetBridgeWall => targetBridgeWall;
    public bool IsCrossingBridge => isCrossingBridge;
    public bool IsTransformDrivenMovement => isTransformDrivenMovement || isCrossingBridge;
    protected override void Start()
    {
        base.Start();
        PrepareRigidbody();
        agent.ConfigureForEnemy();
        stateManager = new EnemyStateManager();
        ChangeState(new FindBrickState(this));
    }
    private void Update()
    {
        if (CurrentStage == null || stateManager == null || isCrossingBridge) return;
        if (HasReachedGoal) { didManualMoveThisFrame = false; return; }
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) { PauseMovement(); return; }
        RefreshKnockdownState();
        if (IsStunned) { agent.PauseAtCurrentPosition(); didManualMoveThisFrame = false; UpdateAnimation(); return; }
        didManualMoveThisFrame = false;
        stateManager.Execute();
        UpdateRotation();
        UpdateAnimation();
    }
    public bool HasEnoughBricksToBuild() => BrickCount >= targetBrickCount;
    public bool HasNoBricks() => BrickCount <= 0;
    public void ResetRefreshTimer() => refreshTimer = 0f;
    public void TickRefreshTimer() => refreshTimer -= Time.deltaTime;
    public bool ShouldRefreshDestination() => refreshTimer <= 0f;
    public void ResetRefreshCooldown() => refreshTimer = destinationRefreshInterval;
    public bool IsAtBuildPoint() => Vector3.Distance(transform.position, buildPoint) <= destinationReachedDistance;
    public bool IsNearDestination() => !agent.PathPending && agent.RemainingDistance <= Mathf.Max(agent.StoppingDistance, destinationReachedDistance);
    public bool CanReachBridge(Bridge bridge) => bridge != null && CanReachBuildPoint(bridge.GetBridgeEntryPosition());
    public void ChangeState(IEnemyState newState) => stateManager.ChangeState(newState);
    public void StopAgentAtCurrentPosition() => agent?.StopAtCurrentPosition();
    public void ReturnToBuildPoint() => agent?.SetDestination(buildPoint);
    public void DisableAgentMovement() => agent?.DisableMovement();
    public void EnableAgentMovement() { agent?.EnableMovement(); isTransformDrivenMovement = false; }
    public void SetTransformDrivenMovement(bool isActive) => isTransformDrivenMovement = isActive;
    public void MoveForwardOnBridge() => MoveManually(buildMoveDirection);
    public void MoveBackFromBridge() => MoveManually(-buildMoveDirection);
    public bool TrySnapToNavMesh() => agent.TrySnapToNavMesh(transform.position, 0.35f);
    public bool TryReturnFromBridgeInstantly(Bridge bridge) => (bridge != null && agent.TrySnapToNavMesh(bridge.GetBridgeEntryPosition(), 1.5f)) || agent.TrySnapToNavMesh(transform.position, 1.5f);
    public void StopAllMovement() => StopForGoal();
    public void RefreshBrickTarget()
    {
        BrickSpawner spawner = CurrentBrickSpawner != null ? CurrentBrickSpawner : CurrentStage.BrickSpawner;
        Brick brick = spawner != null ? spawner.GetClosestBrick(transform.position, characterColor) : null;
        MoveToTargetOrStop(brick != null ? brick.transform.position : null);
    }
    public bool ShouldMoveToGoal() => CurrentStage != null && !CurrentStage.HasBrickSpawners && FindFirstObjectByType<Goal>() != null;
    public void RefreshGoalTarget()
    {
        Goal goal = FindFirstObjectByType<Goal>();
        if (goal != null && agent.IsEnabled && ShouldRefreshDestination()) { agent.SetDestination(goal.transform.position); ResetRefreshCooldown(); }
    }
    public bool TryPrepareBuild()
    {
        ReleaseBridgeReservation();
        targetBridgeWall = CurrentStage.GetBestBridgeWallForEnemy(transform.position, this);
        Bridge bridge = targetBridgeWall != null ? targetBridgeWall.Bridge : null;
        if (bridge == null || !bridge.TryReserveEnemy(this)) { StopAgentAtCurrentPosition(); return false; }
        buildPoint = bridge.GetBridgeEntryPosition();
        if (!CanReachBuildPoint(buildPoint)) { ReleaseBridgeReservation(); StopAgentAtCurrentPosition(); return false; }
        if (agent.TrySamplePosition(buildPoint, 0.2f, out Vector3 sampledPoint)) buildPoint = sampledPoint;
        buildMoveDirection = bridge.GetBuildMoveDirection();
        agent.SetDestination(buildPoint);
        return true;
    }
    public void PauseMovement()
    {
        StopAgentAtCurrentPosition();
        didManualMoveThisFrame = false;
        StopRigidbody();
        UpdateAnimation();
    }
    public void ResumeMovement()
    {
        if (!HasReachedGoal && stateManager != null && agent.IsEnabled && !isCrossingBridge) agent.ResumeAtCurrentPosition();
    }
    public void CrossBridge(Bridge bridge, StageController nextStage, BrickSpawner targetSpawner = null)
    {
        if (bridge == null || nextStage == null || isCrossingBridge) return;
        ReleaseBridgeReservation();
        StartCoroutine(CrossBridgeRoutine(bridge, nextStage, targetSpawner));
    }
    protected override void OnKnockedDown() { ReleaseBridgeReservation(); agent?.PauseAtCurrentPosition(); }
    protected override void StopForGoal()
    {
        ReleaseBridgeReservation();
        refreshTimer = 0f;
        buildPoint = transform.position;
        buildMoveDirection = Vector3.zero;
        agent.DisableAgent();
        StopRigidbody();
        StopAllCoroutines();
        stateManager = null;
        isCrossingBridge = false;
        didManualMoveThisFrame = false;
        isTransformDrivenMovement = false;
        isRunning = false;
        characterAnimation.SetRootMotion(false);
        enabled = false;
    }
    private IEnumerator CrossBridgeRoutine(Bridge bridge, StageController nextStage, BrickSpawner targetSpawner)
    {
        isCrossingBridge = true;
        isTransformDrivenMovement = true;
        isRunning = false;
        DisableAgentMovement();
        Vector3 targetPosition = bridge.GetBridgeEndPosition();
        while (Vector3.Distance(transform.position, targetPosition) > 0.05f)
        {
            if (GameManager.Instance != null && GameManager.Instance.IsPaused) { agent?.SyncNextPosition(); yield return null; continue; }
            MoveManually(Flatten(targetPosition - transform.position), 3.5f);
            yield return null;
        }
        SetCurrentStage(nextStage, targetSpawner);
        EnableAgentMovement();
        isCrossingBridge = false;
        isTransformDrivenMovement = false;
        ChangeState(new FindBrickState(this));
    }
    private bool CanReachBuildPoint(Vector3 point) => agent.CanReach(point, 0.2f);
    private void MoveToTargetOrStop(Vector3? targetPosition) { if (targetPosition.HasValue) agent.SetDestination(targetPosition.Value); else StopAgentAtCurrentPosition(); }
    private void MoveManually(Vector3 direction, float speed = -1f)
    {
        if (direction.sqrMagnitude <= 0.001f) return;
        Vector3 normalizedDirection = direction.normalized;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(normalizedDirection), Time.deltaTime * rotationSpeed);
        transform.position += normalizedDirection * (speed > 0f ? speed : manualBridgeMoveSpeed) * Time.deltaTime;
        didManualMoveThisFrame = true;
        agent?.SyncNextPosition();
    }
    private void UpdateRotation()
    {
        if (didManualMoveThisFrame || isCrossingBridge || !agent.UpdatePosition || agent.IsStopped) return;
        Vector3 velocity = Flatten(agent.Velocity);
        if (velocity.sqrMagnitude > 0.001f) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(velocity.normalized), Time.deltaTime * rotationSpeed);
    }
    private void UpdateAnimation()
    {
        if (IsStunned) { isRunning = false; return; }
        bool isMoving = isCrossingBridge || didManualMoveThisFrame || agent.Velocity.sqrMagnitude > 0.05f;
        if (isMoving == isRunning) return;
        characterAnimation.SetMoving(isMoving);
        isRunning = isMoving;
    }
    private void PrepareRigidbody()
    {
        StopRigidbody();
        characterRigidbody.useGravity = false;
        characterRigidbody.isKinematic = true;
    }
    private void StopRigidbody()
    {
        if (characterRigidbody.isKinematic) return;
        characterRigidbody.linearVelocity = Vector3.zero;
        characterRigidbody.angularVelocity = Vector3.zero;
    }
    private void ReleaseBridgeReservation() { targetBridgeWall?.Bridge?.ReleaseEnemy(this); targetBridgeWall = null; }
    private static Vector3 Flatten(Vector3 vector) { vector.y = 0f; return vector; }
}
