using UnityEngine;

public partial class Bridge
{
    private const float ColorTolerance = 0.01f;

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

    public bool IsCurrentBrickOwnedBy(Color color)
    {
        return IsBrickOwnedBy(currentIndex, color);
    }

    public bool IsBrickOwnedBy(int index, Color color)
    {
        return IsBrickPaintedColor(GetBrickAtIndex(index), color);
    }

    public void PaintCurrentBrick(Color color)
    {
        PaintBrickAtIndex(currentIndex, color);
    }

    public void PaintBrickAtIndex(int index, Color color)
    {
        GameObject brick = GetBrickAtIndex(index);
        if (brick == null) return;

        if (TryGetBridgeBrick(brick, out BridgeBrick bridgeBrick))
        {
            bridgeBrick.RevealAndPaint(color);
        }
        else
        {
            RevealAndPaintLegacyBrick(brick, color);
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
            if (IsBrickPaintedColor(brick, color))
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

    private bool IsBrickPaintedColor(GameObject brick, Color color)
    {
        if (!IsBrickVisible(brick))
        {
            return false;
        }

        if (TryGetBridgeBrick(brick, out BridgeBrick bridgeBrick))
        {
            return bridgeBrick.IsOwnedBy(color);
        }

        MeshRenderer renderer = brick.GetComponentInChildren<MeshRenderer>();
        return renderer != null && AreSameColor(renderer.material.color, color);
    }

    private void RevealAndPaintLegacyBrick(GameObject brick, Color color)
    {
        if (!brick.activeSelf)
        {
            brick.SetActive(true);
        }

        foreach (MeshRenderer renderer in brick.GetComponentsInChildren<MeshRenderer>())
        {
            renderer.material = new Material(renderer.material);
            renderer.material.color = color;
        }
    }

    private bool TryGetBridgeBrick(GameObject brick, out BridgeBrick bridgeBrick)
    {
        bridgeBrick = brick != null ? brick.GetComponent<BridgeBrick>() : null;
        return bridgeBrick != null;
    }

    private bool AreSameColor(Color first, Color second)
    {
        return Vector4.Distance(first, second) <= ColorTolerance;
    }
}
