using UnityEngine;

public class Brick : PoolObject
{
    [SerializeField] private MeshRenderer meshRenderer;

    public ColorType ownerColorType { get; private set; }
    public Color ownerColor;
    public Vector3 spawnPos;
    public BrickSpawner sourceSpawner { get; private set; }
    public StageController sourceStage { get; private set; }
    public bool IsNeutral { get; private set; }
    private float collectibleAtTime;

    public void SetOwnerColor(ColorType colorType, Color color, Material material = null)
    {
        ownerColorType = colorType;
        ownerColor = color;
        IsNeutral = false;
        collectibleAtTime = Time.time;
        ApplyVisual(color, material);
    }

    public void SetNeutralColor(ColorType colorType, Color color, Material material = null)
    {
        ownerColorType = colorType;
        ownerColor = color;
        IsNeutral = true;
        collectibleAtTime = Time.time;
        ApplyVisual(color, material);
    }

    public void SetNeutral()
    {
        ownerColorType = ColorType.None;
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

    public bool CanBeCollectedBy(ColorType collectorColorType)
    {
        if (Time.time < collectibleAtTime)
        {
            return false;
        }

        return IsNeutral || ownerColorType == collectorColorType;
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
