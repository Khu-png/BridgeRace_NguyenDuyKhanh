using System.Collections.Generic;
using UnityEngine;

public partial class Bridge
{
    private readonly Dictionary<GameObject, ColorType> legacyBrickColorTypes = new Dictionary<GameObject, ColorType>();

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
        return IsBrickActiveAtIndex(currentIndex);
    }

    public bool IsBrickActiveAtIndex(int index)
    {
        return IsBrickVisible(GetBrickAtIndex(index));
    }

    public bool IsCurrentBrickOwnedBy(ColorType colorType)
    {
        return IsBrickOwnedBy(currentIndex, colorType);
    }

    public bool IsBrickOwnedBy(int index, ColorType colorType)
    {
        return IsBrickPaintedColor(GetBrickAtIndex(index), colorType);
    }

    public void PaintCurrentBrick(ColorType colorType, Color color, Material material = null)
    {
        PaintBrickAtIndex(currentIndex, colorType, color, material);
    }

    public void PaintBrickAtIndex(int index, ColorType colorType, Color color, Material material = null)
    {
        GameObject brick = GetBrickAtIndex(index);
        if (brick == null) return;

        if (TryGetBridgeBrick(brick, out BridgeBrick bridgeBrick))
        {
            bridgeBrick.RevealAndPaint(colorType, color, material);
        }
        else
        {
            RevealAndPaintLegacyBrick(brick, colorType, color, material);
        }

        if (index >= currentIndex)
        {
            currentIndex = index + 1;
        }
    }

    public int CountBuiltBricksByColor(ColorType colorType)
    {
        int count = 0;

        foreach (GameObject brick in bricks)
        {
            if (IsBrickPaintedColor(brick, colorType))
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
            if (IsBrickVisible(brick))
            {
                count++;
            }
        }

        return count;
    }

    private bool IsBrickVisible(GameObject brick)
    {
        if (brick == null) return false;

        if (TryGetBridgeBrick(brick, out BridgeBrick bridgeBrick))
        {
            return bridgeBrick.IsRevealed;
        }

        return brick.activeSelf;
    }

    private bool IsBrickPaintedColor(GameObject brick, ColorType colorType)
    {
        if (!IsBrickVisible(brick))
        {
            return false;
        }

        if (TryGetBridgeBrick(brick, out BridgeBrick bridgeBrick))
        {
            return bridgeBrick.IsOwnedBy(colorType);
        }

        MeshRenderer renderer = brick.GetComponentInChildren<MeshRenderer>();
        return renderer != null
            && renderer.sharedMaterial != null
            && legacyBrickColorTypes.TryGetValue(brick, out ColorType ownerColorType)
            && ownerColorType == colorType;
    }

    private void RevealAndPaintLegacyBrick(GameObject brick, ColorType colorType, Color color, Material material)
    {
        if (!brick.activeSelf)
        {
            brick.SetActive(true);
        }

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

        legacyBrickColorTypes[brick] = colorType;
    }

    private bool TryGetBridgeBrick(GameObject brick, out BridgeBrick bridgeBrick)
    {
        bridgeBrick = brick != null ? brick.GetComponent<BridgeBrick>() : null;
        return bridgeBrick != null;
    }

}
