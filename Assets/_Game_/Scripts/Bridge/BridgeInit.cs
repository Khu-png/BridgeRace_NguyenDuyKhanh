using System.Collections;
using UnityEngine;

public partial class Bridge
{
    private const string GroundLayerName = "Ground";
    private const string DefaultLayerName = "Default";

    private void GenerateBridge()
    {
        if (brickPrefab == null || startPoint == null)
        {
            return;
        }

        Vector3 localPosition = Vector3.zero;
        Vector3 step = GetStepVector();

        for (int i = 0; i < brickCount; i++)
        {
            GameObject brick = Instantiate(brickPrefab, startPoint);
            SetupGeneratedBrick(brick, localPosition, i);
            bricks.Add(brick);
            localPosition += step;
        }
    }

    private void SetupGeneratedBrick(GameObject brick, Vector3 localPosition, int index)
    {
        brick.transform.localPosition = localPosition;
        brick.transform.localRotation = Quaternion.identity;
        brick.SetActive(true);

        if (TryGetBridgeBrick(brick, out BridgeBrick bridgeBrick))
        {
            bridgeBrick.Initialize(this, index);
        }
    }

    private void GenerateRamp()
    {
        if (rampPrefab == null || startPoint == null)
        {
            return;
        }

        Vector3 start = startPoint.position;
        Vector3 direction = GetStepVector() * brickCount;
        float length = direction.magnitude;

        GameObject ramp = Instantiate(rampPrefab, transform);
        generatedRamp = ramp;
        SetRampGroundLayer(ramp);

        ramp.transform.rotation = Quaternion.LookRotation(direction, startPoint.up);
        ramp.transform.localScale = new Vector3(rampWidth, rampThickness, length);
        ramp.transform.position = start + direction / 2f + GetRampOffset();

        MeshRenderer meshRenderer = ramp.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.enabled = false;
        }
    }

    private Vector3 GetStepVector()
    {
        if (startPoint == null)
        {
            return Vector3.zero;
        }

        return startPoint.up * stepHeight + startPoint.forward * stepLength;
    }

    private Vector3 GetRampOffset()
    {
        return -startPoint.up * (rampThickness / 2f)
            + startPoint.up * offsetUp
            - startPoint.forward * offsetBack;
    }

    private void SetRampGroundLayer(GameObject ramp)
    {
        int groundLayer = LayerMask.NameToLayer(GroundLayerName);
        if (groundLayer >= 0)
        {
            SetLayerRecursively(ramp, groundLayer);
        }
    }

    private void SetLayerRecursively(GameObject target, int layer)
    {
        if (target == null) return;

        target.layer = layer;

        foreach (Transform child in target.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
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

}
