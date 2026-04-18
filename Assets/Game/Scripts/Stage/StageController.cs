using UnityEngine;

public class StageController : MonoBehaviour
{
    [SerializeField] private BrickSpawner brickSpawner;
    public BrickSpawner BrickSpawner => brickSpawner;

    private void Awake()
    {
        if (brickSpawner == null)
        {
            brickSpawner = GetComponentInChildren<BrickSpawner>();
        }
    }

    public void RegisterCharacter(Character character)
    {
        if (character == null) return;

        if (brickSpawner != null)
        {
            brickSpawner.RegisterCharacter(character);
        }
    }

    public void UnregisterCharacter(Character character)
    {
        if (character == null) return;

        if (brickSpawner != null)
        {
            brickSpawner.UnregisterCharacter(character);
        }
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

    public BridgeWall GetBestBridgeWallForEnemy(Vector3 fromPosition, Enemy enemy)
    {
        BridgeWall[] bridgeWalls = GetComponentsInChildren<BridgeWall>();
        BridgeWall bestWall = null;
        int bestOwnedBrickCount = -1;
        float bestDistance = float.MaxValue;

        foreach (BridgeWall wall in bridgeWalls)
        {
            if (wall == null || !wall.enabled) continue;
            if (enemy != null && !wall.CanAcceptEnemy(enemy)) continue;

            Bridge bridge = wall.GetComponent<Bridge>() ?? wall.GetComponentInParent<Bridge>();
            if (bridge == null || bridge.IsRetired || bridge.currentIndex >= bridge.brickCount) continue;

            int ownedBrickCount = bridge.CountBuiltBricksByColor(enemy.characterColor);
            float sqrDistance = (wall.transform.position - fromPosition).sqrMagnitude;

            if (ownedBrickCount > bestOwnedBrickCount ||
                (ownedBrickCount == bestOwnedBrickCount && sqrDistance < bestDistance))
            {
                bestOwnedBrickCount = ownedBrickCount;
                bestDistance = sqrDistance;
                bestWall = wall;
            }
        }

        return bestWall;
    }
}
