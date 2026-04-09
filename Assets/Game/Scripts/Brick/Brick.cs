using UnityEngine;

public class Brick : MonoBehaviour
{
    [SerializeField] private MeshRenderer meshRenderer;
    public Color ownerColor;
    public Vector3 spawnPos;

    public void SetOwnerColor(Color color)
    {
        ownerColor = color;
        meshRenderer.material.color = ownerColor;
    }
}