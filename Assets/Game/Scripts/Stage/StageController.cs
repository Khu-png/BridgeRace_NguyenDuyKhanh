using UnityEngine;
using System.Collections.Generic;

public class StageController : MonoBehaviour
{
    [SerializeField] private BrickSpawner defaultBrickSpawner;
    [SerializeField] private List<BrickSpawner> brickSpawners = new List<BrickSpawner>();
    public BrickSpawner BrickSpawner => GetDefaultBrickSpawner();
    public bool HasBrickSpawners => GetDefaultBrickSpawner() != null;

    private void Awake()
    {
        if (brickSpawners.Count == 0)
        {
            brickSpawners.AddRange(GetComponentsInChildren<BrickSpawner>());
        }

        brickSpawners.RemoveAll(spawner => spawner == null);

        if (defaultBrickSpawner == null)
        {
            defaultBrickSpawner = brickSpawners.Count > 0 ? brickSpawners[0] : null;
        }
        else if (!brickSpawners.Contains(defaultBrickSpawner))
        {
            brickSpawners.Insert(0, defaultBrickSpawner);
        }
    }

    public void RegisterCharacter(Character character)
    {
        if (character == null) return;

        BrickSpawner spawner = ResolveSpawner(character);
        if (spawner != null)
        {
            spawner.RegisterCharacter(character);
        }
    }

    public void UnregisterCharacter(Character character)
    {
        if (character == null) return;

        BrickSpawner spawner = ResolveSpawner(character);
        if (spawner != null)
        {
            spawner.UnregisterCharacter(character);
        }
    }

    public BrickSpawner GetDefaultBrickSpawner()
    {
        if (defaultBrickSpawner != null)
        {
            return defaultBrickSpawner;
        }

        for (int i = 0; i < brickSpawners.Count; i++)
        {
            if (brickSpawners[i] != null)
            {
                return brickSpawners[i];
            }
        }

        return null;
    }

    private BrickSpawner ResolveSpawner(Character character)
    {
        BrickSpawner characterSpawner = character.CurrentBrickSpawner;
        if (characterSpawner != null && brickSpawners.Contains(characterSpawner))
        {
            return characterSpawner;
        }

        return GetDefaultBrickSpawner();
    }

    public BridgeWall GetClosestAvailableBridgeWall(Vector3 fromPosition)
    {
        BridgeWall[] bridgeWalls = GetComponentsInChildren<BridgeWall>();
        BridgeWall closestWall = null;
        float closestDistance = float.MaxValue;

        foreach (BridgeWall wall in bridgeWalls)
        {
            if (wall == null || !wall.enabled) continue;

            Bridge bridge = wall.GetComponent<Bridge>() ?? wall.GetComponentInParent<Bridge>();
            if (bridge == null || bridge.IsRetired || bridge.currentIndex >= bridge.brickCount) continue;

            float sqrDistance = (wall.transform.position - fromPosition).sqrMagnitude;
            if (sqrDistance < closestDistance)
            {
                closestDistance = sqrDistance;
                closestWall = wall;
            }
        }

        return closestWall;
    }

    public Bridge GetBestBridgeForEnemy(Vector3 fromPosition, Enemy enemy)
    {
        Bridge[] bridges = GetComponentsInChildren<Bridge>();
        Bridge bestBridge = null;
        int bestOwnedBrickCount = -1;
        float bestDistance = float.MaxValue;

        foreach (Bridge bridge in bridges)
        {
            if (enemy != null && (bridge == null || !bridge.CanAcceptEnemy(enemy))) continue;
            if (enemy != null && bridge != null && !enemy.CanReachBridge(bridge)) continue;
            if (bridge == null || bridge.IsRetired || bridge.currentIndex >= bridge.brickCount) continue;

            int ownedBrickCount = bridge.CountBuiltBricksByColor(enemy.characterColor);
            float sqrDistance = (bridge.GetBridgeEntryPosition() - fromPosition).sqrMagnitude;

            if (ownedBrickCount > bestOwnedBrickCount ||
                (ownedBrickCount == bestOwnedBrickCount && sqrDistance < bestDistance))
            {
                bestOwnedBrickCount = ownedBrickCount;
                bestDistance = sqrDistance;
                bestBridge = bridge;
            }
        }

        return bestBridge;
    }
}
