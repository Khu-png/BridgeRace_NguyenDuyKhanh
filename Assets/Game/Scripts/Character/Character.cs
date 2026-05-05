using UnityEngine;

public class Character : MonoBehaviour
{
    private const string InitialStageName = "Stage1";
    private const string FallStateName = "Fall";
    private const string IdleStateName = "Idle";

    [Header("References")]
    [SerializeField] protected Rigidbody characterRigidbody;
    [SerializeField] protected CharacterAnimation characterAnimation;
    [SerializeField] private CharacterVisual visual;
    [SerializeField] private CharacterBrickStack brickStack;

    [Header("Stage")]
    [SerializeField] private StageController currentStage;
    [SerializeField] private BrickSpawner currentBrickSpawner;

    [Header("Combat")]
    [SerializeField] protected float fallDuration = 0.75f;
    [SerializeField] protected float knockdownCooldown = 0.5f;
    [SerializeField] protected float standUpRecoverDelay = 0.15f;

    private float stunEndTime;
    private float lastKnockdownTime = -999f;
    private bool isKnockedDown;
    private bool hasEnteredFallState;
    private bool isBuildingBridge;
    private bool hasReachedGoal;
    private bool hasDespawned;

    public int BrickCount => brickStack.Count;
    public StageController CurrentStage => currentStage;
    public BrickSpawner CurrentBrickSpawner => currentBrickSpawner;
    public bool IsStunned => isKnockedDown || Time.time < stunEndTime;
    public bool IsBuildingBridge => isBuildingBridge;
    public bool HasReachedGoal => hasReachedGoal;
    public Vector3 MovementVelocity => characterRigidbody != null ? characterRigidbody.linearVelocity : Vector3.zero;
    public Color characterColor => visual.CharacterColor;

    protected virtual void Start()
    {
        OnInit();
    }

    protected virtual void OnDestroy()
    {
        OnDespawn();
    }

    public virtual void OnInit()
    {
        LevelManager.Instance?.RegisterCharacter(this);

        hasDespawned = false;
        stunEndTime = 0f;
        lastKnockdownTime = -999f;
        isKnockedDown = false;
        hasEnteredFallState = false;
        isBuildingBridge = false;
        hasReachedGoal = false;

        visual.RandomizeColor();
        characterAnimation.SetTimeScaleMode(AnimatorUpdateMode.Normal);
        characterAnimation.SetRootMotion(false);
        ChangeAnimation(CharacterAnimation.IdleTriggerName);
        ResolveInitialStage();
        currentStage?.RegisterCharacter(this);
    }

    public virtual void OnDespawn()
    {
        if (hasDespawned)
        {
            return;
        }

        hasDespawned = true;
        LevelManager.Instance?.UnregisterCharacter(this);
        currentStage?.UnregisterCharacter(this);
    }

    public void SetCurrentStage(StageController newStage, BrickSpawner targetSpawner = null)
    {
        BrickSpawner resolvedSpawner = newStage != null
            ? (targetSpawner != null ? targetSpawner : newStage.BrickSpawner)
            : null;

        if (currentStage == newStage && currentBrickSpawner == resolvedSpawner) return;

        isBuildingBridge = false;
        currentStage?.UnregisterCharacter(this);
        currentStage = newStage;
        currentBrickSpawner = resolvedSpawner;
        currentStage?.RegisterCharacter(this);
    }

    public void CollectBrick(Vector3 pickupPosition)
    {
        brickStack.Collect(pickupPosition, characterColor);
    }

    public GameObject RemoveBrick()
    {
        return brickStack.Remove();
    }

    public void SetBridgeBuildingState(bool isBuilding)
    {
        isBuildingBridge = isBuilding;
    }

    public bool TryKnockDown()
    {
        if (!CanBeKnockedDown()) return false;

        lastKnockdownTime = Time.time;
        stunEndTime = Time.time + fallDuration;
        isKnockedDown = true;
        hasEnteredFallState = false;

        ChangeAnimation(CharacterAnimation.FallTriggerName);
        brickStack.DropAll(transform.position, characterColor);
        OnKnockedDown();
        return true;
    }

    public void RefreshKnockdownState()
    {
        if (!isKnockedDown) return;

        AnimatorStateInfo stateInfo = characterAnimation.CurrentState;
        bool isInTransition = characterAnimation.IsInTransition;

        if (!hasEnteredFallState && stateInfo.IsName(FallStateName))
        {
            hasEnteredFallState = true;
        }

        if (hasEnteredFallState && !isInTransition && stateInfo.IsName(IdleStateName))
        {
            stunEndTime = Time.time + standUpRecoverDelay;
            isKnockedDown = false;
        }
    }

    public void Block()
    {
        characterRigidbody.linearVelocity = Vector3.zero;
    }

    protected void ChangeAnimation(string animationName)
    {
        characterAnimation.ChangeAnimation(animationName);
    }

    public virtual void ReachGoal(Transform goalTransform)
    {
        SetBridgeBuildingState(false);
        hasReachedGoal = true;
        StopForGoal();
        brickStack.Clear();

        transform.SetParent(null, true);
        transform.SetPositionAndRotation(
            goalTransform.position,
            goalTransform.rotation * Quaternion.Euler(0f, 180f, 0f));

        characterAnimation.SetTimeScaleMode(AnimatorUpdateMode.UnscaledTime);
        characterAnimation.SetRootMotion(false);
        ChangeAnimation(CharacterAnimation.DanceTriggerName);
    }

    protected virtual void OnKnockedDown()
    {
        characterRigidbody.linearVelocity = Vector3.zero;
        characterRigidbody.angularVelocity = Vector3.zero;
    }

    protected virtual void StopForGoal()
    {
        characterRigidbody.linearVelocity = Vector3.zero;
        characterRigidbody.angularVelocity = Vector3.zero;
    }

    private bool CanBeKnockedDown()
    {
        return Time.time >= lastKnockdownTime + knockdownCooldown;
    }

    private void ResolveInitialStage()
    {
        StageController initialStage = FindInitialStageController();
        if (initialStage != null)
        {
            currentStage = initialStage;
        }
        else if (currentStage == null)
        {
            currentStage = FindClosestStageController();
        }

        if (currentBrickSpawner == null && currentStage != null)
        {
            currentBrickSpawner = currentStage.BrickSpawner;
        }
    }

    private StageController FindClosestStageController()
    {
        StageController[] stages = FindObjectsByType<StageController>(FindObjectsSortMode.None);
        StageController closestStage = null;
        float closestDistance = float.MaxValue;

        foreach (StageController stage in stages)
        {
            if (stage == null) continue;

            float sqrDistance = (stage.transform.position - transform.position).sqrMagnitude;
            if (sqrDistance >= closestDistance) continue;

            closestDistance = sqrDistance;
            closestStage = stage;
        }

        return closestStage;
    }

    private StageController FindInitialStageController()
    {
        StageController[] stages = FindObjectsByType<StageController>(FindObjectsSortMode.None);

        foreach (StageController stage in stages)
        {
            if (stage != null && stage.name == InitialStageName)
            {
                return stage;
            }
        }

        return null;
    }
}
