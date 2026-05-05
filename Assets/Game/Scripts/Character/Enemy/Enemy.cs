using UnityEngine;

[RequireComponent(typeof(UnityEngine.AI.NavMeshAgent), typeof(NavMesh.NavmeshAgent), typeof(EnemyMovement))]
public class Enemy : Character
{
    [Header("Enemy AI")]
    [SerializeField] private EnemyMovement movement;
    [SerializeField] private int minTargetBrickCount = 5;
    [SerializeField] private int maxTargetBrickCount = 15;
    [SerializeField] private float destinationRefreshInterval = 0.25f;

    private EnemyStateManager stateManager;
    private EnemyBridgeBuilder bridgeBuilder;
    private float refreshTimer;
    private int targetBrickCount;
    private bool isRunning;

    public Bridge TargetBridge => bridgeBuilder.TargetBridge;
    public bool IsCrossingBridge => movement != null && movement.IsCrossingBridge;
    public bool IsTransformDrivenMovement => movement != null && movement.IsTransformDrivenMovement;

    public override void OnInit()
    {
        base.OnInit();
        ResolveMovement();
        movement.OnInit(characterRigidbody);

        stateManager = new EnemyStateManager();
        bridgeBuilder = new EnemyBridgeBuilder(this, movement);
        refreshTimer = 0f;
        isRunning = false;
        RandomizeTargetBrickCount();

        ChangeState(new FindBrickState(this));
    }

    public override void OnDespawn()
    {
        bridgeBuilder?.ReleaseReservation();
        base.OnDespawn();
    }

    private void Update()
    {
        if (ShouldSkipUpdate()) return;
        if (TryHandlePausedOrStunned()) return;

        movement.BeginFrame();
        stateManager.Execute();
        movement.UpdateRotation();
        UpdateAnimation();
    }

    public void ChangeState(IEnemyState newState) => stateManager.ChangeState(newState);
    public bool HasEnoughBricksToBuild() => BrickCount >= targetBrickCount;
    public bool HasNoBricks() => BrickCount <= 0;
    public void RandomizeTargetBrickCount()
    {
        int min = Mathf.Max(1, minTargetBrickCount);
        int max = Mathf.Max(min, maxTargetBrickCount);
        targetBrickCount = Random.Range(min, max + 1);
    }

    public void ResetRefreshTimer() => refreshTimer = 0f;
    public void TickRefreshTimer() => refreshTimer -= Time.deltaTime;
    public bool ShouldRefreshDestination() => refreshTimer <= 0f;
    public void ResetRefreshCooldown() => refreshTimer = destinationRefreshInterval;
    public bool IsAtBuildPoint() => bridgeBuilder.IsAtBuildPoint;
    public bool IsNearDestination() => bridgeBuilder.IsNearDestination;
    public bool CanReachBridge(Bridge bridge) => bridgeBuilder.CanReachBridge(bridge);
    public void StopAgentAtCurrentPosition() => movement.StopAgentAtCurrentPosition();
    public void ReturnToBuildPoint() => bridgeBuilder.ReturnToBuildPoint();
    public void DisableAgentMovement() => movement.DisableAgentMovement();
    public void EnableAgentMovement() => movement.EnableAgentMovement();
    public void SetTransformDrivenMovement(bool isActive) => movement.SetTransformDrivenMovement(isActive);
    public void MoveForwardOnBridge() => bridgeBuilder.MoveForward();
    public void MoveBackFromBridge() => bridgeBuilder.MoveBack();
    public bool TrySnapToNavMesh() => movement.TrySnapToNavMesh(transform.position, 0.35f);
    public void StopAllMovement() => StopForGoal();

    public bool TryReturnFromBridgeInstantly(Bridge bridge)
    {
        return (bridge != null && movement.TrySnapToNavMesh(bridge.GetBridgeEntryPosition(), 1.5f))
            || movement.TrySnapToNavMesh(transform.position, 1.5f);
    }

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
        if (goal == null || !movement.IsAgentEnabled || !ShouldRefreshDestination()) return;

        movement.SetDestination(goal.transform.position);
        ResetRefreshCooldown();
    }

    public bool TryPrepareBuild() => bridgeBuilder.TryPrepareBuild(CurrentStage);

    public void PauseMovement()
    {
        StopAgentAtCurrentPosition();
        movement.BeginFrame();
        movement.StopRigidbody();
        UpdateAnimation();
    }

    public void ResumeMovement()
    {
        if (!HasReachedGoal && stateManager != null && movement.IsAgentEnabled && !IsCrossingBridge)
            movement.ResumeAgentAtCurrentPosition();
    }

    public void CrossBridge(Bridge bridge, StageController nextStage, BrickSpawner targetSpawner = null)
    {
        if (bridge == null || nextStage == null || IsCrossingBridge) return;

        bridgeBuilder.ReleaseReservation();
        StartCoroutine(bridgeBuilder.CrossBridgeRoutine(bridge, nextStage, targetSpawner));
    }

    protected override void OnKnockedDown()
    {
        bridgeBuilder.ReleaseReservation();
        movement.PauseAgentAtCurrentPosition();
    }

    protected override void StopForGoal()
    {
        bridgeBuilder.ReleaseReservation();
        refreshTimer = 0f;
        bridgeBuilder.Reset(transform.position);
        movement.DisableAgent();
        movement.StopRigidbody();
        StopAllCoroutines();
        stateManager = null;
        movement.ResetState();
        isRunning = false;
        characterAnimation.SetRootMotion(false);
        enabled = false;
    }

    private bool ShouldSkipUpdate()
    {
        if (CurrentStage == null || stateManager == null || IsCrossingBridge) return true;
        if (!HasReachedGoal) return false;

        movement.BeginFrame();
        return true;
    }

    private bool TryHandlePausedOrStunned()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
        {
            PauseMovement();
            return true;
        }

        RefreshKnockdownState();
        if (!IsStunned) return false;

        movement.PauseAgentAtCurrentPosition();
        movement.BeginFrame();
        UpdateAnimation();
        return true;
    }

    private void MoveToTargetOrStop(Vector3? targetPosition)
    {
        if (targetPosition.HasValue) movement.SetDestination(targetPosition.Value);
        else StopAgentAtCurrentPosition();
    }

    private void UpdateAnimation()
    {
        if (IsStunned)
        {
            isRunning = false;
            return;
        }

        bool isMoving = movement.IsMoving;
        if (isMoving == isRunning) return;

        characterAnimation.SetMoving(isMoving);
        isRunning = isMoving;
    }

    private void ResolveMovement()
    {
        if (movement != null) return;

        movement = GetComponent<EnemyMovement>();
        if (movement == null) movement = gameObject.AddComponent<EnemyMovement>();
    }
}
