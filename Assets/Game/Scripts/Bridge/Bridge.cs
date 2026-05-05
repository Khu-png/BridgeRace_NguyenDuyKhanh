using System.Collections.Generic;
using UnityEngine;

public partial class Bridge : MonoBehaviour
{
    [Header("Ramp Settings")]
    [SerializeField] private GameObject rampPrefab;
    [SerializeField] private float rampWidth = 0.4f;
    [SerializeField] private float rampThickness = 0.2f;
    [SerializeField] private float offsetUp;
    [SerializeField] private float offsetBack;
    [SerializeField] private float retireGroundDelay = 1.2f;

    [Header("Brick Settings")]
    public GameObject brickPrefab;
    public int brickCount = 20;
    public float stepLength = 1f;
    public float stepHeight = 0.2f;

    [Header("Start Point")]
    public Transform startPoint;

    [Header("Build brick")]
    public List<GameObject> bricks = new List<GameObject>();
    public int currentIndex = 0;

    [Header("Stage")]
    [SerializeField] private StageController sourceStage;
    [SerializeField] private BrickSpawner targetSpawner;
    [SerializeField] private int maxEnemyBuilders = 2;

    private BridgeWall bridgeWall;
    private GameObject generatedRamp;
    private bool isRetired;
    private Character lastProgressCharacter;
    private Character bridgeCompleter;
    private readonly HashSet<Enemy> reservedEnemies = new HashSet<Enemy>();

    public StageController SourceStage => sourceStage;
    public BrickSpawner TargetSpawner => targetSpawner;
    public StageController TargetStage => targetSpawner != null ? targetSpawner.GetComponentInParent<StageController>() : null;
    public bool IsRetired => isRetired;
    public Character LastProgressCharacter => lastProgressCharacter;
    public Character BridgeCompleter => bridgeCompleter;

    private void Awake()
    {
        CacheReferences();
        GenerateBridge();
        GenerateRamp();
    }

    private void CacheReferences()
    {
        bridgeWall = GetComponentInChildren<BridgeWall>();

        if (sourceStage == null)
        {
            sourceStage = GetComponentInParent<StageController>();
        }
    }

    public bool CanBuild() => currentIndex < bricks.Count;
    public bool IsFull() => currentIndex >= bricks.Count;

    public Vector3 GetBuildPosition()
    {
        if (startPoint == null)
        {
            return transform.position;
        }

        return startPoint.position + GetStepVector() * currentIndex;
    }

    public void RegisterBrickProgress(int index, Character builder = null)
    {
        if (isRetired) return;

        bool wasFull = IsFull();

        if (builder != null)
        {
            lastProgressCharacter = builder;
        }

        if (index >= currentIndex)
        {
            currentIndex = index + 1;
        }

        if (!wasFull && IsFull() && builder != null)
        {
            bridgeCompleter = builder;
        }
    }

    public void NextStep()
    {
        if (isRetired || currentIndex >= brickCount)
        {
            return;
        }

        currentIndex++;
    }

    public void MoveWallForward(int brickIndex)
    {
        if (isRetired || bridgeWall == null) return;
        bridgeWall.MoveWallToIndex(brickIndex);
    }

    public void MoveWallForwardIfAhead(int brickIndex)
    {
        if (isRetired || bridgeWall == null) return;
        bridgeWall.MoveWallToIndexIfAhead(brickIndex);
    }

    public void MoveWallToBlockBrick(int brickIndex)
    {
        if (isRetired || bridgeWall == null) return;
        bridgeWall.MoveWallBeforeIndex(brickIndex);
    }

    public void TryComplete(Character character)
    {
        if (character is Enemy || bridgeWall == null) return;
        bridgeWall.TryAdvance(character);
    }

    public void Retire()
    {
        isRetired = true;

        if (generatedRamp == null) return;

        StopAllCoroutines();
        StartCoroutine(DisableRampGroundAfterDelay());
    }

    public Vector3 GetBridgeEndPosition()
    {
        if (startPoint == null)
        {
            return transform.position;
        }

        return startPoint.position + GetStepVector() * brickCount;
    }

    public Vector3 GetBuildMoveDirection()
    {
        if (startPoint == null)
        {
            return transform.forward;
        }

        Vector3 direction = GetStepVector();
        return direction.sqrMagnitude > 0.001f ? direction.normalized : startPoint.forward;
    }

    public Vector3 GetBridgeEntryPosition(float backwardOffset = 0.35f)
    {
        if (startPoint == null)
        {
            return transform.position;
        }

        return startPoint.position - startPoint.forward * backwardOffset;
    }

    public bool CanAcceptEnemy(Enemy enemy)
    {
        if (enemy == null) return false;
        if (isRetired || IsFull()) return false;
        if (reservedEnemies.Contains(enemy)) return true;
        return reservedEnemies.Count < maxEnemyBuilders;
    }

    public bool TryReserveEnemy(Enemy enemy)
    {
        if (!CanAcceptEnemy(enemy)) return false;
        reservedEnemies.Add(enemy);
        return true;
    }

    public void ReleaseEnemy(Enemy enemy)
    {
        if (enemy == null) return;
        reservedEnemies.Remove(enemy);
    }
}
