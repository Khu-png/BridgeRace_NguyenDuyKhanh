using System.Collections.Generic;
using UnityEngine;

public class BridgeBrick : MonoBehaviour
{
    [SerializeField] private Transform modelRoot;
    [SerializeField] private MeshRenderer[] modelRenderers;

    private Bridge bridge;
    private int brickIndex;
    private readonly HashSet<Character> charactersInside = new HashSet<Character>();

    public bool IsRevealed => modelRoot.gameObject.activeSelf;

    public void Initialize(Bridge ownerBridge, int index)
    {
        bridge = ownerBridge;
        brickIndex = index;

        modelRoot.gameObject.SetActive(false);
    }

    public bool IsOwnedBy(Color color)
    {
        if (!IsRevealed) return false;

        return Vector4.Distance(modelRenderers[0].material.color, color) <= 0.01f;
    }

    public void RevealAndPaint(Color color)
    {
        modelRoot.gameObject.SetActive(true);

        foreach (MeshRenderer renderer in modelRenderers)
        {
            renderer.material = new Material(renderer.material);
            renderer.material.color = color;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (bridge == null) return;
        if (bridge.IsRetired) return;

        Character character = other.GetComponentInParent<Character>();
        if (character == null) return;

        if (!character.CompareTag("Player") && !character.CompareTag("Enemy")) return;
        if (bridge.SourceStage != null && character.CurrentStage != bridge.SourceStage) return;
        if (!charactersInside.Add(character)) return;

        bool canBuildNewBrick = brickIndex == bridge.currentIndex;
        bool canOverwriteExistingBrick = IsRevealed;

        if (!canBuildNewBrick && !canOverwriteExistingBrick) return;

        character.SetBridgeBuildingState(true);

        if (IsOwnedBy(character.characterColor))
        {
            bridge?.RegisterBrickProgress(brickIndex, character);
            if (character.CompareTag("Player") && !bridge.IsRetired)
            {
                bridge?.MoveWallForward(brickIndex);
            }
            if (bridge != null && bridge.IsFull())
            {
                bridge.TryComplete(character);
            }
            return;
        }

        if (character.BrickCount <= 0) return;

        character.RemoveBrick();
        RevealAndPaint(character.characterColor);
        bridge?.RegisterBrickProgress(brickIndex, character);
        if (character.CompareTag("Player") && !bridge.IsRetired)
        {
            bridge?.MoveWallForward(brickIndex);
        }
        if (bridge != null && bridge.IsFull())
        {
            bridge.TryComplete(character);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Character character = other.GetComponentInParent<Character>();
        if (character == null) return;

        charactersInside.Remove(character);
        character.SetBridgeBuildingState(false);
    }
}
