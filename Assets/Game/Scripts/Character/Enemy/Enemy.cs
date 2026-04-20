using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : Character
{
    [Header("Enemy AI")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Rigidbody enemyRigidbody;
    [SerializeField] private Animator animator;
    [SerializeField] private int targetBrickCount = 5;
    [SerializeField] private float destinationRefreshInterval = 0.25f;
    [SerializeField] private float destinationReachedDistance = 0.2f;
    [SerializeField] private float rotationSpeed = 12f;
    [SerializeField] private float manualBridgeMoveSpeed = 3f;

    private EnemyStateManager stateManager;
    private float refreshTimer;
    private Vector3 buildPoint;
    private Vector3 buildMoveDirection;
    private BridgeWall targetBridgeWall;
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

        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }

        if (enemyRigidbody == null)
        {
            enemyRigidbody = GetComponent<Rigidbody>();
        }

        if (enemyRigidbody != null)
        {
            if (!enemyRigidbody.isKinematic)
            {
                enemyRigidbody.linearVelocity = Vector3.zero;
                enemyRigidbody.angularVelocity = Vector3.zero;
            }

            enemyRigidbody.useGravity = false;
            enemyRigidbody.isKinematic = true;
        }

        agent.updateRotation = false;
        agent.baseOffset = 0f;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;

        stateManager = new EnemyStateManager();
        stateManager.ChangeState(new FindBrickState(this));
    }

    private void Update()
    {
        if (agent == null || CurrentStage == null || stateManager == null || isCrossingBridge) return;

        if (HasReachedGoal)
        {
            didManualMoveThisFrame = false;
            return;
        }

        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
        {
            StopAgentAtCurrentPosition();
            didManualMoveThisFrame = false;
            UpdateAnimation();
            return;
        }

        RefreshKnockdownState();

        if (IsStunned)
        {
            if (agent.enabled)
            {
                agent.isStopped = true;
                agent.nextPosition = transform.position;
            }

            didManualMoveThisFrame = false;
            UpdateAnimation();
            return;
        }

        didManualMoveThisFrame = false;
        stateManager.Execute();
        UpdateRotation();
        UpdateAnimation();
    }

    public bool HasEnoughBricksToBuild()
    {
        return BrickCount >= targetBrickCount;
    }

    public bool HasNoBricks()
    {
        return BrickCount <= 0;
    }

    public void ResetRefreshTimer()
    {
        refreshTimer = 0f;
    }

    public void TickRefreshTimer()
    {
        refreshTimer -= Time.deltaTime;
    }

    public bool ShouldRefreshDestination()
    {
        return refreshTimer <= 0f;
    }

    public void ResetRefreshCooldown()
    {
        refreshTimer = destinationRefreshInterval;
    }

    public void RefreshBrickTarget()
    {
        BrickSpawner spawner = CurrentBrickSpawner != null ? CurrentBrickSpawner : CurrentStage.BrickSpawner;
        if (spawner == null)
        {
            StopAgentAtCurrentPosition();
            return;
        }

        Brick closestBrick = spawner.GetClosestBrick(transform.position, characterColor);
        if (closestBrick == null)
        {
            StopAgentAtCurrentPosition();
            return;
        }

        agent.isStopped = false;
        agent.SetDestination(closestBrick.transform.position);
    }

    public bool ShouldMoveToGoal()
    {
        return CurrentStage != null && !CurrentStage.HasBrickSpawners && FindFirstObjectByType<Goal>() != null;
    }

    public void RefreshGoalTarget()
    {
        Goal goal = FindFirstObjectByType<Goal>();
        if (goal == null)
        {
            return;
        }

        if (agent != null && agent.enabled && ShouldRefreshDestination())
        {
            agent.isStopped = false;
            agent.SetDestination(goal.transform.position);
            ResetRefreshCooldown();
        }
    }

    public bool TryPrepareBuild()
    {
        targetBridgeWall?.Bridge?.ReleaseEnemy(this);

        targetBridgeWall = CurrentStage.GetBestBridgeWallForEnemy(transform.position, this);
        if (targetBridgeWall == null)
        {
            StopAgentAtCurrentPosition();
            return false;
        }

        Bridge targetBridge = targetBridgeWall.Bridge;
        if (targetBridge == null) return false;
        if (!targetBridge.TryReserveEnemy(this)) return false;

        buildPoint = targetBridge.GetBridgeEntryPosition();
        if (!CanReachBuildPoint(buildPoint))
        {
            targetBridge.ReleaseEnemy(this);
            targetBridgeWall = null;
            StopAgentAtCurrentPosition();
            return false;
        }

        if (NavMesh.SamplePosition(buildPoint, out NavMeshHit hit, 0.2f, NavMesh.AllAreas))
        {
            buildPoint = hit.position;
        }
        buildMoveDirection = targetBridge.GetBuildMoveDirection();

        agent.isStopped = false;
        agent.SetDestination(buildPoint);
        return true;
    }

    public bool CanReachBridge(Bridge bridge)
    {
        if (bridge == null) return false;
        return CanReachBuildPoint(bridge.GetBridgeEntryPosition());
    }

    public bool IsAtBuildPoint()
    {
        return Vector3.Distance(transform.position, buildPoint) <= destinationReachedDistance;
    }

    public void ReturnToBuildPoint()
    {
        if (agent != null && agent.enabled)
        {
            agent.isStopped = false;
        }

        agent.SetDestination(buildPoint);
    }

    public void StopAgentAtCurrentPosition()
    {
        if (agent == null || !agent.enabled)
        {
            return;
        }

        agent.ResetPath();
        agent.isStopped = true;
        agent.nextPosition = transform.position;
    }

    public void PauseMovement()
    {
        StopAgentAtCurrentPosition();
        didManualMoveThisFrame = false;

        if (enemyRigidbody != null && !enemyRigidbody.isKinematic)
        {
            enemyRigidbody.linearVelocity = Vector3.zero;
            enemyRigidbody.angularVelocity = Vector3.zero;
        }

        UpdateAnimation();
    }

    public void ResumeMovement()
    {
        if (HasReachedGoal || stateManager == null)
        {
            return;
        }

        if (agent != null && agent.enabled && !isCrossingBridge)
        {
            agent.nextPosition = transform.position;
            agent.isStopped = false;
        }
    }

    private bool CanReachBuildPoint(Vector3 point)
    {
        if (agent == null || !agent.enabled)
        {
            return false;
        }

        if (!NavMesh.SamplePosition(point, out NavMeshHit hit, 0.2f, NavMesh.AllAreas))
        {
            return false;
        }

        NavMeshPath path = new NavMeshPath();
        if (!agent.CalculatePath(hit.position, path))
        {
            return false;
        }

        return path.status == NavMeshPathStatus.PathComplete;
    }

    public bool IsNearDestination()
    {
        if (agent.pathPending) return false;

        return agent.remainingDistance <= Mathf.Max(agent.stoppingDistance, destinationReachedDistance);
    }

    public void ChangeState(IEnemyState newState)
    {
        stateManager.ChangeState(newState);
    }

    public void DisableAgentMovement()
    {
        if (agent == null || !agent.enabled) return;

        agent.isStopped = true;
        agent.updatePosition = false;
        agent.updateRotation = false;
        agent.nextPosition = transform.position;
    }

    public void EnableAgentMovement()
    {
        if (agent == null) return;

        if (!agent.enabled)
        {
            agent.enabled = true;
        }

        agent.Warp(transform.position);
        agent.updatePosition = true;
        agent.updateRotation = false;
        agent.isStopped = false;
        isTransformDrivenMovement = false;
    }

    public void SetTransformDrivenMovement(bool isActive)
    {
        isTransformDrivenMovement = isActive;
    }

    public void MoveForwardOnBridge()
    {
        MoveManually(buildMoveDirection);
    }

    public void MoveBackFromBridge()
    {
        MoveManually(-buildMoveDirection);
    }

    public void CrossBridge(Bridge bridge, StageController nextStage, BrickSpawner targetSpawner = null)
    {
        if (bridge == null || nextStage == null || isCrossingBridge) return;

        targetBridgeWall?.Bridge?.ReleaseEnemy(this);
        StartCoroutine(CrossBridgeRoutine(bridge, nextStage, targetSpawner));
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
            if (GameManager.Instance != null && GameManager.Instance.IsPaused)
            {
                if (agent != null && agent.enabled)
                {
                    agent.nextPosition = transform.position;
                }

                yield return null;
                continue;
            }

            Vector3 direction = targetPosition - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }

            transform.position = Vector3.MoveTowards(transform.position, targetPosition, 3.5f * Time.deltaTime);
            didManualMoveThisFrame = true;
            if (agent != null && agent.enabled)
            {
                agent.nextPosition = transform.position;
            }
            yield return null;
        }

        SetCurrentStage(nextStage, targetSpawner);

        EnableAgentMovement();

        isCrossingBridge = false;
        isTransformDrivenMovement = false;
        ChangeState(new FindBrickState(this));
    }

    private void UpdateRotation()
    {
        if (didManualMoveThisFrame || isCrossingBridge || !agent.updatePosition || agent.isStopped)
        {
            return;
        }

        Vector3 velocity = agent.velocity;
        velocity.y = 0f;

        if (velocity.sqrMagnitude <= 0.001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(velocity.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    private void UpdateAnimation()
    {
        if (animator == null) return;

        if (IsStunned)
        {
            isRunning = false;
            return;
        }

        bool isMoving = isCrossingBridge || didManualMoveThisFrame || agent.velocity.sqrMagnitude > 0.05f;

        if (isMoving && !isRunning)
        {
            animator.SetTrigger("Run");
            isRunning = true;
        }
        else if (!isMoving && isRunning)
        {
            animator.SetTrigger("Idle");
            isRunning = false;
        }
    }

    private void MoveManually(Vector3 direction)
    {
        if (direction.sqrMagnitude <= 0.001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        transform.position += direction.normalized * manualBridgeMoveSpeed * Time.deltaTime;
        didManualMoveThisFrame = true;
        if (agent != null && agent.enabled)
        {
            agent.nextPosition = transform.position;
        }
    }

    public bool TrySnapToNavMesh()
    {
        return TrySnapToNavMeshAt(transform.position, 0.35f);
    }

    public bool TryReturnFromBridgeInstantly(Bridge bridge)
    {
        if (bridge != null && TrySnapToNavMeshAt(bridge.GetBridgeEntryPosition(), 1.5f))
        {
            return true;
        }

        return TrySnapToNavMeshAt(transform.position, 1.5f);
    }

    private bool TrySnapToNavMeshAt(Vector3 samplePosition, float maxDistance)
    {
        if (NavMesh.SamplePosition(samplePosition, out NavMeshHit hit, maxDistance, NavMesh.AllAreas))
        {
            transform.position = hit.position;
            if (agent != null && agent.enabled)
            {
                agent.nextPosition = transform.position;
            }
            return true;
        }

        return false;
    }

    public void StopAllMovement()
    {
        StopForGoal();
    }

    protected override void OnKnockedDown()
    {
        targetBridgeWall?.Bridge?.ReleaseEnemy(this);
        targetBridgeWall = null;

        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.nextPosition = transform.position;
        }
    }

    protected override void StopForGoal()
    {
        targetBridgeWall?.Bridge?.ReleaseEnemy(this);
        targetBridgeWall = null;

        refreshTimer = 0f;
        buildPoint = transform.position;
        buildMoveDirection = Vector3.zero;

        if (agent != null)
        {
            if (agent.enabled)
            {
                agent.ResetPath();
                agent.isStopped = true;
                agent.updatePosition = false;
                agent.updateRotation = false;
                agent.nextPosition = transform.position;
                agent.enabled = false;
            }
        }

        if (enemyRigidbody != null)
        {
            if (!enemyRigidbody.isKinematic)
            {
                enemyRigidbody.linearVelocity = Vector3.zero;
                enemyRigidbody.angularVelocity = Vector3.zero;
            }
        }

        StopAllCoroutines();
        stateManager = null;
        isCrossingBridge = false;
        didManualMoveThisFrame = false;
        isTransformDrivenMovement = false;
        isRunning = false;
        SetTransformDrivenMovement(false);

        if (animator != null)
        {
            animator.applyRootMotion = false;
        }

        enabled = false;
    }
}
