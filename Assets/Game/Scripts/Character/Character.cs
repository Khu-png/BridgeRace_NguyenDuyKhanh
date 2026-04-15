using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Character : MonoBehaviour
{
    private const string StackBrickPoolKey = "PlayerBrick";
    private const string DroppedBrickPoolKey = "FallBrick";
    private const string FallbackDroppedBrickPoolKey = "SpawnBrick";

    [Header("Brick Stack")]
    [SerializeField] protected Transform brickHolder;
    [SerializeField] protected float brickOffset = 0.5f;
    [SerializeField] protected float moveSpeed = 8f;
    [SerializeField] protected Vector3 stackBrickScale = new Vector3(0.9f, 0.9f, 0.9f);

    [Header("Color")]
    public Color characterColor = Color.white;

    [Header("Stage")]
    [SerializeField] private StageController currentStage;

    protected Animator fallAnimator;
    [Header("Combat")]
    [SerializeField] protected float dropScatterRadius = 0.6f;
    [SerializeField] protected float dropHeight = 0.1f;
    [SerializeField] protected float fallDuration = 0.75f;
    [SerializeField] protected float knockdownCooldown = 0.5f;
    [SerializeField] protected float droppedBrickFlyDuration = 0.18f;
    [SerializeField] protected float droppedBrickFlyHeight = 0.35f;
    [SerializeField] protected float droppedBrickCollectDelay = 0.25f;
    [SerializeField] protected Vector3 droppedBrickScale = new Vector3(0.8f, 0.8f, 0.8f);
    [SerializeField] protected string fallStateName = "Fall";
    [SerializeField] protected string idleStateName = "Idle";

    protected Stack<GameObject> brickStack = new Stack<GameObject>();
    private Coroutine updateRoutine;
    private float stunEndTime;
    private float lastKnockdownTime = -999f;
    private bool isKnockedDown;
    private bool hasEnteredFallState;
    private bool isBuildingBridge;

    public int BrickCount => brickStack.Count;
    public StageController CurrentStage => currentStage;
    public bool IsStunned => isKnockedDown || Time.time < stunEndTime;
    public bool IsBuildingBridge => isBuildingBridge;

    protected virtual void Start()
    {
        CacheAnimator();

        if (currentStage != null)
        {
            currentStage.RegisterCharacter(this);
        }
    }

    public void SetCurrentStage(StageController newStage)
    {
        if (currentStage == newStage) return;

        currentStage?.UnregisterCharacter(this);

        currentStage = newStage;

        currentStage?.RegisterCharacter(this);
    }

    public void CollectBrick(Vector3 pickupPosition)
    {
        GameObject brick = SpawnStackBrick(pickupPosition);
        if (brick == null) return;

        brickStack.Push(brick);
        RefreshStackVisuals();
    }

    public GameObject RemoveBrick()
    {
        if (brickStack.Count == 0) return null;

        GameObject topBrick = brickStack.Pop();
        SimplePool.Despawn(topBrick);
        RefreshStackVisuals();

        return topBrick;
    }

    public bool CanBeKnockedDown()
    {
        return !isBuildingBridge && Time.time >= lastKnockdownTime + knockdownCooldown;
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

        TriggerFallAnimation();

        DropAllBricks();
        OnKnockedDown();
        return true;
    }

    public void RefreshKnockdownState()
    {
        if (!isKnockedDown) return;

        if (fallAnimator == null)
        {
            if (Time.time >= stunEndTime)
            {
                isKnockedDown = false;
            }

            return;
        }

        AnimatorStateInfo stateInfo = fallAnimator.GetCurrentAnimatorStateInfo(0);
        bool isInTransition = fallAnimator.IsInTransition(0);

        if (!hasEnteredFallState && stateInfo.IsName(fallStateName))
        {
            hasEnteredFallState = true;
        }

        if (hasEnteredFallState && !isInTransition && stateInfo.IsName(idleStateName))
        {
            isKnockedDown = false;
            return;
        }
    }

    protected virtual void OnKnockedDown()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    public void DropAllBricks()
    {
        int droppedCount = brickStack.Count;
        ClearStackBricks();
        RefreshStackVisuals();

        for (int i = 0; i < droppedCount; i++)
        {
            SpawnDroppedBrick();
        }
    }

    public void Block(Rigidbody rb)
    {
        rb.linearVelocity = Vector3.zero;
    }

    protected IEnumerator UpdateStackSmooth()
    {
        GameObject[] bricks = brickStack.ToArray();

        for (int i = 0; i < bricks.Length; i++)
        {
            StartCoroutine(MoveToStackPosition(bricks[i], i));
        }

        yield break;
    }

    protected IEnumerator MoveToStackPosition(GameObject brick, int index)
    {
        Vector3 start = brick.transform.localPosition;
        Vector3 target = new Vector3(0, index * brickOffset, -0.5f);
        Quaternion startRotation = brick.transform.localRotation;
        Quaternion targetRotation = Quaternion.identity;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;
            brick.transform.localPosition = Vector3.Lerp(start, target, t);
            brick.transform.localRotation = Quaternion.Slerp(startRotation, targetRotation, t);
            yield return null;
        }

        brick.transform.localPosition = target;
        brick.transform.localRotation = targetRotation;
    }

    private void CacheAnimator()
    {
        if (fallAnimator == null)
        {
            fallAnimator = GetComponentInChildren<Animator>();
        }
    }

    private void TriggerFallAnimation()
    {
        if (fallAnimator == null) return;

        fallAnimator.ResetTrigger("Idle");
        fallAnimator.ResetTrigger("Run");
        fallAnimator.SetTrigger("Fall");
    }

    private GameObject SpawnStackBrick(Vector3 pickupPosition)
    {
        GameObject brick = SimplePool.Spawn(StackBrickPoolKey, pickupPosition, Quaternion.identity);
        if (brick == null) return null;

        brick.transform.SetParent(brickHolder, true);
        brick.transform.localRotation = Quaternion.identity;
        brick.transform.localScale = stackBrickScale;

        foreach (MeshRenderer mr in brick.GetComponentsInChildren<MeshRenderer>())
        {
            mr.material = new Material(mr.material);
            mr.material.color = characterColor;
        }

        return brick;
    }

    private void RefreshStackVisuals()
    {
        if (updateRoutine != null)
        {
            StopCoroutine(updateRoutine);
        }

        updateRoutine = StartCoroutine(UpdateStackSmooth());
    }

    private void ClearStackBricks()
    {
        while (brickStack.Count > 0)
        {
            GameObject stackBrick = brickStack.Pop();
            if (stackBrick != null)
            {
                SimplePool.Despawn(stackBrick);
            }
        }

        foreach (GameObject remainingBrick in GetActiveStackBrickObjects())
        {
            SimplePool.Despawn(remainingBrick);
        }
    }

    private void SpawnDroppedBrick()
    {
        Vector2 randomCircle = Random.insideUnitCircle * dropScatterRadius;
        Vector3 startPosition = transform.position + Vector3.up * dropHeight;
        Vector3 targetPosition = transform.position + new Vector3(randomCircle.x, dropHeight, randomCircle.y);

        GameObject droppedBrick = SimplePool.Spawn(DroppedBrickPoolKey, startPosition, Quaternion.identity);
        if (droppedBrick == null)
        {
            droppedBrick = SimplePool.Spawn(FallbackDroppedBrickPoolKey, startPosition, Quaternion.identity);
        }

        if (droppedBrick == null) return;

        droppedBrick.transform.localScale = droppedBrickScale;

        Brick brick = droppedBrick.GetComponent<Brick>();
        if (brick != null)
        {
            brick.spawnPos = targetPosition;
            brick.SetNeutral();
            brick.SetCollectDelay(droppedBrickCollectDelay);
            brick.SetSource(null, null);
        }

        StartCoroutine(AnimateDroppedBrick(droppedBrick.transform, targetPosition));
    }

    private List<GameObject> GetActiveStackBrickObjects()
    {
        List<GameObject> stackBricks = new List<GameObject>();

        if (brickHolder == null)
        {
            return stackBricks;
        }

        for (int i = 0; i < brickHolder.childCount; i++)
        {
            Transform child = brickHolder.GetChild(i);
            if (child == null || !child.gameObject.activeSelf)
            {
                continue;
            }

            PoolManager.PoolObject poolObject = child.GetComponent<PoolManager.PoolObject>();
            if (poolObject != null && poolObject.key == StackBrickPoolKey)
            {
                stackBricks.Add(child.gameObject);
            }
        }

        return stackBricks;
    }

    private IEnumerator AnimateDroppedBrick(Transform brickTransform, Vector3 targetPosition)
    {
        Vector3 startPosition = brickTransform.position;
        Quaternion startRotation = brickTransform.rotation;
        Quaternion targetRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

        float elapsed = 0f;

        while (elapsed < droppedBrickFlyDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / droppedBrickFlyDuration);
            Vector3 position = Vector3.Lerp(startPosition, targetPosition, t);
            position.y += Mathf.Sin(t * Mathf.PI) * droppedBrickFlyHeight;

            brickTransform.position = position;
            brickTransform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            yield return null;
        }

        brickTransform.position = targetPosition;
        brickTransform.rotation = targetRotation;
    }
}
