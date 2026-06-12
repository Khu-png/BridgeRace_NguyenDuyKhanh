using System.Collections.Generic;
using UnityEngine;

public class BridgeBrick : MonoBehaviour
{
    [SerializeField] private Transform modelRoot;
    [SerializeField] private MeshRenderer[] modelRenderers;

    private Bridge bridge;
    private int brickIndex;
    private ColorType ownerColorType;
    private bool hasOwnerColor;
    private readonly HashSet<Character> charactersInside = new HashSet<Character>();

    public bool IsRevealed => modelRoot.gameObject.activeSelf;

    public void Initialize(Bridge ownerBridge, int index)
    {
        bridge = ownerBridge;
        brickIndex = index;
        ownerColorType = ColorType.None;
        hasOwnerColor = false;

        modelRoot.gameObject.SetActive(false);
    }

    public bool IsOwnedBy(ColorType colorType)
    {
        if (!IsRevealed || !hasOwnerColor) return false;

        return ownerColorType == colorType;
    }

    public void RevealAndPaint(ColorType colorType, Color color, Material material = null)
    {
        modelRoot.gameObject.SetActive(true);
        ownerColorType = colorType;
        hasOwnerColor = true;

        foreach (MeshRenderer renderer in modelRenderers)
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

        if (character.CompareTag("Player") && !IsMovingForwardOnBridge(character))
        {
            return;
        }

        if (IsOwnedBy(character.characterColorType))
        {
            if (character.CompareTag("Player") && !bridge.IsRetired)
            {
                bridge?.MoveWallForwardIfAhead(brickIndex);
            }

            if (bridge != null && bridge.IsFull())
            {
                bridge.TryComplete(character);
            }

            return;
        }

        if (character.BrickCount <= 0)
        {
            if (character.CompareTag("Player") && !bridge.IsRetired)
            {
                bridge?.MoveWallToBlockBrick(brickIndex);
                character.Block();
            }

            return;
        }

        character.SetBridgeBuildingState(true);

        character.RemoveBrick();
        RevealAndPaint(character.characterColorType, character.characterColor, character.characterMaterial);
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

    private bool IsMovingForwardOnBridge(Character character)
    {
        Vector3 moveDirection = Flatten(character.MovementVelocity);
        if (moveDirection.sqrMagnitude <= 0.001f)
        {
            moveDirection = Flatten(character.transform.forward);
        }

        Vector3 bridgeDirection = Flatten(bridge.GetBuildMoveDirection());
        if (moveDirection.sqrMagnitude <= 0.001f || bridgeDirection.sqrMagnitude <= 0.001f)
        {
            return true;
        }

        return Vector3.Dot(moveDirection.normalized, bridgeDirection.normalized) > 0f;
    }

    private static Vector3 Flatten(Vector3 vector)
    {
        vector.y = 0f;
        return vector;
    }

}
