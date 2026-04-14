using UnityEngine;

public class Brick : MonoBehaviour
{
    [SerializeField] private MeshRenderer meshRenderer;
    public Color ownerColor;
    public Vector3 spawnPos;
    public BrickSpawner sourceSpawner { get; private set; }
    public StageController sourceStage { get; private set; }

    public void SetOwnerColor(Color color)
    {
        ownerColor = color;
        meshRenderer.material.color = ownerColor;
    }

    public void SetSource(BrickSpawner spawner, StageController stage)
    {
        sourceSpawner = spawner;
        sourceStage = stage;
    }
}
