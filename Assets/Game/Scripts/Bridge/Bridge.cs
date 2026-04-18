using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class Bridge : MonoBehaviour
{
    private const string GroundLayerName = "Ground";
    private const string DefaultLayerName = "Default";

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

    private BridgeWall bridgeWall;
    private GameObject generatedRamp;
    private bool isRetired;
    public StageController SourceStage => sourceStage;
    public bool IsRetired => isRetired;
    
    
    void Awake()
    {
        bridgeWall = GetComponentInChildren<BridgeWall>();

        if (sourceStage == null)
        {
            sourceStage = GetComponentInParent<StageController>();
        }

        GenerateBridge();
        GenerateRamp();
    }
    
    void GenerateBridge()
    {
        Vector3 localPos = Vector3.zero;
        
        Vector3 step = startPoint.up * stepHeight + startPoint.forward * stepLength;

        for (int i = 0; i < brickCount; i++)
        {
            GameObject brick = Instantiate(brickPrefab, startPoint);

            brick.transform.localPosition = localPos;
            brick.transform.localRotation = Quaternion.identity;
            brick.SetActive(true);

            BridgeBrick bridgeBrick = brick.GetComponent<BridgeBrick>();
            if (bridgeBrick != null)
            {
                bridgeBrick.Initialize(this, i);
            }

            bricks.Add(brick);

            localPos += step;
        }
    }
    
    void GenerateRamp()
    {
        Vector3 step = startPoint.up * stepHeight + startPoint.forward * stepLength;

        Vector3 start = startPoint.position;
        Vector3 end = start + step * brickCount;

        Vector3 direction = end - start;
        float length = direction.magnitude;

        GameObject ramp = Instantiate(rampPrefab, transform);
        generatedRamp = ramp;
        int groundLayer = LayerMask.NameToLayer(GroundLayerName);
        if (groundLayer >= 0)
        {
            SetLayerRecursively(ramp, groundLayer);
        }
        
        ramp.transform.rotation = Quaternion.LookRotation(direction, startPoint.up);
        
        ramp.transform.localScale = new Vector3(rampWidth, rampThickness, length);
        
        Vector3 center = start + direction / 2f;
        
        Vector3 offset =
            (-startPoint.up * (rampThickness / 2f)) + 
            (startPoint.up * offsetUp) +              
            (-startPoint.forward * offsetBack);       

        ramp.transform.position = center + offset;
        
        MeshRenderer mr = ramp.GetComponent<MeshRenderer>();
        if (mr != null) mr.enabled = false;
    }

    private void SetLayerRecursively(GameObject target, int layer)
    {
        if (target == null) return;

        target.layer = layer;

        foreach (Transform child in target.transform)
        {
            if (child == null) continue;
            SetLayerRecursively(child.gameObject, layer);
        }
    }
    
    public bool CanBuild() => currentIndex < bricks.Count;
    public bool IsFull() => currentIndex >= bricks.Count;
    public GameObject GetCurrentBrick()
    {
        return GetBrickAtIndex(currentIndex);
    }

    public GameObject GetBrickAtIndex(int index)
    {
        if (index < 0 || index >= bricks.Count)
        {
            return null;
        }

        return bricks[index];
    }

    public bool IsCurrentBrickActive()
    {
        GameObject currentBrick = GetCurrentBrick();
        if (currentBrick == null) return false;

        BridgeBrick bridgeBrick = currentBrick.GetComponent<BridgeBrick>();
        return bridgeBrick != null ? bridgeBrick.IsRevealed : currentBrick.activeSelf;
    }

    public bool IsBrickActiveAtIndex(int index)
    {
        GameObject brick = GetBrickAtIndex(index);
        if (brick == null) return false;

        BridgeBrick bridgeBrick = brick.GetComponent<BridgeBrick>();
        return bridgeBrick != null ? bridgeBrick.IsRevealed : brick.activeSelf;
    }

    public bool IsCurrentBrickOwnedBy(Color color)
    {
        return IsBrickOwnedBy(index: currentIndex, color);
    }

    public bool IsBrickOwnedBy(int index, Color color)
    {
        GameObject currentBrick = GetBrickAtIndex(index);
        if (currentBrick == null)
        {
            return false;
        }

        BridgeBrick bridgeBrick = currentBrick.GetComponent<BridgeBrick>();
        if (bridgeBrick != null)
        {
            return bridgeBrick.IsOwnedBy(color);
        }

        if (!currentBrick.activeSelf)
        {
            return false;
        }

        MeshRenderer renderer = currentBrick.GetComponentInChildren<MeshRenderer>();
        if (renderer == null)
        {
            return false;
        }

        return Vector4.Distance(renderer.material.color, color) <= 0.01f;
    }

    public void PaintCurrentBrick(Color color)
    {
        PaintBrickAtIndex(currentIndex, color);
    }

    public void PaintBrickAtIndex(int index, Color color)
    {
        GameObject currentBrick = GetBrickAtIndex(index);
        if (currentBrick == null) return;

        BridgeBrick bridgeBrick = currentBrick.GetComponent<BridgeBrick>();
        if (bridgeBrick != null)
        {
            bridgeBrick.RevealAndPaint(color);
        }
        else
        {
            if (!currentBrick.activeSelf)
            {
                currentBrick.SetActive(true);
            }

            foreach (MeshRenderer renderer in currentBrick.GetComponentsInChildren<MeshRenderer>())
            {
                renderer.material = new Material(renderer.material);
                renderer.material.color = color;
            }
        }

        if (index >= currentIndex)
        {
            currentIndex = index + 1;
        }
    }

    public int CountBuiltBricksByColor(Color color)
    {
        int count = 0;

        foreach (GameObject brick in bricks)
        {
            if (brick == null) continue;

            BridgeBrick bridgeBrick = brick.GetComponent<BridgeBrick>();
            if (bridgeBrick != null && !bridgeBrick.IsRevealed) continue;
            if (bridgeBrick == null && !brick.activeSelf) continue;

            MeshRenderer renderer = brick.GetComponentInChildren<MeshRenderer>();
            if (renderer == null) continue;

            if (Vector4.Distance(renderer.material.color, color) <= 0.01f)
            {
                count++;
            }
        }

        return count;
    }

    public int CountBuiltBricks()
    {
        int count = 0;

        foreach (GameObject brick in bricks)
        {
            if (brick == null) continue;

            BridgeBrick bridgeBrick = brick.GetComponent<BridgeBrick>();
            if (bridgeBrick != null)
            {
                if (bridgeBrick.IsRevealed)
                {
                    count++;
                }

                continue;
            }

            if (brick.activeSelf)
            {
                count++;
            }
        }

        return count;
    }

    public Vector3 GetBuildPosition()
    {
        if (startPoint == null)
        {
            return transform.position;
        }

        Vector3 step = startPoint.up * stepHeight + startPoint.forward * stepLength;
        return startPoint.position + step * currentIndex;
    }

    public void RegisterBrickProgress(int index)
    {
        if (isRetired) return;

        if (index >= currentIndex)
        {
            currentIndex = index + 1;
        }
    }

    public void MoveWallForward(int brickIndex)
    {
        if (isRetired) return;

        if (bridgeWall != null)
        {
            bridgeWall.MoveWallToIndex(brickIndex);
        }
    }

    public void TryComplete(Character character)
    {
        if (character is Enemy)
        {
            return;
        }

        if (bridgeWall != null)
        {
            bridgeWall.TryAdvance(character);
        }
    }

    public void Retire()
    {
        isRetired = true;

        if (generatedRamp != null)
        {
            StopAllCoroutines();
            StartCoroutine(DisableRampGroundAfterDelay());
        }
    }

    private IEnumerator DisableRampGroundAfterDelay()
    {
        yield return new WaitForSeconds(retireGroundDelay);

        if (generatedRamp == null) yield break;

        int defaultLayer = LayerMask.NameToLayer(DefaultLayerName);
        if (defaultLayer >= 0)
        {
            SetLayerRecursively(generatedRamp, defaultLayer);
        }
    }

    public Vector3 GetBridgeEndPosition()
    {
        if (startPoint == null)
        {
            return transform.position;
        }

        Vector3 step = startPoint.up * stepHeight + startPoint.forward * stepLength;
        return startPoint.position + step * brickCount;
    }

    public Vector3 GetBuildMoveDirection()
    {
        if (startPoint == null)
        {
            return transform.forward;
        }

        Vector3 direction = startPoint.up * stepHeight + startPoint.forward * stepLength;
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
}
