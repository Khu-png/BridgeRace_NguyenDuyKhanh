using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Character : MonoBehaviour
{
    [Header("Brick Stack")]
    [SerializeField] protected Transform brickHolder;
    [SerializeField] protected string brickKey = "PlayerBrick";
    [SerializeField] protected float brickOffset = 0.5f;
    [SerializeField] protected float moveSpeed = 8f;
    [SerializeField] protected Vector3 stackBrickScale = new Vector3(0.8f, 0.8f, 0.8f);

    [Header("Color")]
    public Color characterColor = Color.white;

    [Header("Stage")]
    [SerializeField] private StageController currentStage;

    protected Stack<GameObject> brickStack = new Stack<GameObject>();
    private Coroutine updateRoutine;

    public int BrickCount => brickStack.Count;
    public StageController CurrentStage => currentStage;

    protected virtual void Start()
    {
        if (currentStage != null)
        {
            currentStage.RegisterCharacter(this);
        }
    }

    public void SetCurrentStage(StageController newStage)
    {
        if (currentStage == newStage) return;

        StageController previousStage = currentStage;

        if (currentStage != null)
        {
            currentStage.UnregisterCharacter(this);
        }

        currentStage = newStage;

        if (currentStage != null)
        {
            currentStage.RegisterCharacter(this);
        }

        Debug.Log($"{name} stage changed: {previousStage?.name ?? "None"} -> {currentStage?.name ?? "None"}");
    }

    // ================= COLLECT =================
    public void CollectBrick(Vector3 pickupPosition)
    {
        GameObject brick = SimplePool.Spawn(brickKey, pickupPosition, Quaternion.identity);
        brick.transform.SetParent(brickHolder, true);
        brick.transform.localRotation = Quaternion.identity;
        brick.transform.localScale = stackBrickScale;

        // set màu
        foreach (var mr in brick.GetComponentsInChildren<MeshRenderer>())
        {
            mr.material = new Material(mr.material);
            mr.material.color = characterColor;
        }

        brickStack.Push(brick);

        if (updateRoutine != null) StopCoroutine(updateRoutine);
        updateRoutine = StartCoroutine(UpdateStackSmooth());
    }

    // ================= REMOVE =================
    public GameObject RemoveBrick()
    {
        if (brickStack.Count == 0) return null;

        GameObject topBrick = brickStack.Pop();

        SimplePool.Despawn(topBrick);

        if (updateRoutine != null) StopCoroutine(updateRoutine);
        updateRoutine = StartCoroutine(UpdateStackSmooth());

        return topBrick;
    }

    // ================= BLOCK =================
    public void Block(Rigidbody rb)
    {
        rb.linearVelocity = Vector3.zero;
    }

    // ================= STACK ANIMATION =================
    protected IEnumerator UpdateStackSmooth()
    {
        GameObject[] bricks = brickStack.ToArray(); // ❗ FIX crash

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

        float t = 0;
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
}
