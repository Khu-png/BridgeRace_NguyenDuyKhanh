using UnityEngine;

public class Brick : MonoBehaviour
{
    [SerializeField] private MeshRenderer meshRenderer;

    public Color ownerColor;
    public Vector3 spawnPos;
    public BrickSpawner sourceSpawner { get; private set; }
    public StageController sourceStage { get; private set; }
    public bool IsNeutral { get; private set; }
    private float collectibleAtTime;

    public void SetOwnerColor(Color color, Material material = null)
    {
        ownerColor = color;
        IsNeutral = false;
        collectibleAtTime = Time.time;
        ApplyVisual(color, material);
    }

    public void SetNeutralColor(Color color, Material material = null)
    {
        ownerColor = color;
        IsNeutral = true;
        collectibleAtTime = Time.time;
        ApplyVisual(color, material);
    }

    public void SetNeutral()
    {
        IsNeutral = true;
        collectibleAtTime = Time.time;
    }

    public void SetCollectDelay(float delay)
    {
        collectibleAtTime = Time.time + delay;
    }

    public void SetSource(BrickSpawner spawner, StageController stage)
    {
        sourceSpawner = spawner;
        sourceStage = stage;
    }

    public bool CanBeCollectedBy(Color collectorColor)
    {
        if (Time.time < collectibleAtTime)
        {
            return false;
        }

        return IsNeutral || Vector4.Distance(ownerColor, collectorColor) <= 0.01f;
    }

    private void ApplyVisual(Color color, Material material)
    {
        if (meshRenderer == null) return;

        if (material != null)
        {
            RendererColorUtility.ApplyMaterial(meshRenderer, material);
            return;
        }

        RendererColorUtility.ApplyColor(meshRenderer, color);
    }
}
