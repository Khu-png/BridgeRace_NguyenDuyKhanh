using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterBrickStack : MonoBehaviour
{
    private const string StackBrickPoolKey = "PlayerBrick";
    private const string DroppedBrickPoolKey = "FallBrick";
    private const string FallbackDroppedBrickPoolKey = "SpawnBrick";

    [SerializeField] private Transform brickHolder;
    [SerializeField] private float brickOffset = 0.5f;
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private Vector3 stackBrickScale = new Vector3(0.9f, 0.9f, 0.9f);

    [Header("Dropped Brick")]
    [SerializeField] private float dropScatterRadius = 0.6f;
    [SerializeField] private float dropHeight = 0.1f;
    [SerializeField] private float droppedBrickFlyDuration = 0.18f;
    [SerializeField] private float droppedBrickFlyHeight = 0.35f;
    [SerializeField] private float droppedBrickCollectDelay = 0.25f;
    [SerializeField] private Vector3 droppedBrickScale = new Vector3(0.8f, 0.8f, 0.8f);

    private readonly Stack<GameObject> bricks = new Stack<GameObject>();
    private Coroutine updateRoutine;

    public int Count => bricks.Count;

    public void Collect(Vector3 pickupPosition, Color color, Material material)
    {
        GameObject brick = SpawnStackBrick(pickupPosition, color, material);
        if (brick == null) return;

        bricks.Push(brick);
        RefreshStackVisuals();
    }

    public GameObject Remove()
    {
        if (bricks.Count == 0) return null;

        GameObject topBrick = bricks.Pop();
        SetStackBrickTrailActive(topBrick, false);
        SimplePool.Despawn(topBrick);
        RefreshStackVisuals();

        return topBrick;
    }

    public void Clear()
    {
        while (bricks.Count > 0)
        {
            GameObject brick = bricks.Pop();
            SetStackBrickTrailActive(brick, false);
            SimplePool.Despawn(brick);
        }
    }

    public void DropAll(Vector3 origin, Color color)
    {
        int droppedCount = bricks.Count;
        Clear();
        RefreshStackVisuals();

        for (int i = 0; i < droppedCount; i++)
        {
            SpawnDroppedBrick(origin);
        }
    }

    private GameObject SpawnStackBrick(Vector3 pickupPosition, Color color, Material material)
    {
        GameObject brick = SimplePool.Spawn(StackBrickPoolKey, pickupPosition, Quaternion.identity);
        if (brick == null) return null;

        brick.transform.SetParent(brickHolder, true);
        brick.transform.localRotation = Quaternion.identity;
        brick.transform.localScale = stackBrickScale;
        PaintBrick(brick, color, material);
        ConfigureStackBrickTrail(brick, color);

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

    private IEnumerator UpdateStackSmooth()
    {
        int brickCount = bricks.Count;
        int brickIndex = 0;

        foreach (GameObject brick in bricks)
        {
            StartCoroutine(MoveToStackPosition(brick, brickCount - 1 - brickIndex));
            brickIndex++;
        }

        yield break;
    }

    private IEnumerator MoveToStackPosition(GameObject brick, int index)
    {
        if (brick == null) yield break;

        Vector3 start = brick.transform.localPosition;
        Vector3 target = new Vector3(0, index * brickOffset, -0.5f);
        Quaternion startRotation = brick.transform.localRotation;

        float t = 0f;
        while (t < 1f)
        {
            if (brick == null) yield break;

            t += Time.deltaTime * moveSpeed;
            brick.transform.localPosition = Vector3.Lerp(start, target, t);
            brick.transform.localRotation = Quaternion.Slerp(startRotation, Quaternion.identity, t);
            yield return null;
        }

        brick.transform.localPosition = target;
        brick.transform.localRotation = Quaternion.identity;
        SetStackBrickTrailActive(brick, false);
    }

    private void SpawnDroppedBrick(Vector3 origin)
    {
        Vector2 randomCircle = Random.insideUnitCircle * dropScatterRadius;
        Vector3 startPosition = origin + Vector3.up * dropHeight;
        Vector3 targetPosition = origin + new Vector3(randomCircle.x, dropHeight, randomCircle.y);

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

    private static void PaintBrick(GameObject brick, Color color, Material material)
    {
        foreach (MeshRenderer renderer in brick.GetComponentsInChildren<MeshRenderer>())
        {
            if (material != null)
            {
                RendererColorUtility.ApplyMaterial(renderer, material);
            }
            else
            {
                RendererColorUtility.ApplyColor(renderer, color);
            }
        }
    }

    private static void ConfigureStackBrickTrail(GameObject brickObject, Color color)
    {
        PlayerBrickTrail trail = brickObject.GetComponentInChildren<PlayerBrickTrail>(true);
        if (trail == null) return;

        trail.SetColor(color);
        trail.SetActive(true);
    }

    private static void SetStackBrickTrailActive(GameObject brickObject, bool isActive)
    {
        if (brickObject == null) return;

        PlayerBrickTrail trail = brickObject.GetComponentInChildren<PlayerBrickTrail>(true);
        if (trail == null) return;

        trail.SetActive(isActive);
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
