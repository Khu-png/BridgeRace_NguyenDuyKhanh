using UnityEngine;

public class BridgeCheck : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        BridgeWall wall = other.GetComponent<BridgeWall>();
        if (wall == null) return;

        Bridge bridge = other.GetComponent<Bridge>() ?? other.GetComponentInParent<Bridge>();
        if (bridge == null) return;

        Character character = GetComponentInParent<Character>();
        if (character == null) return;

        wall.OnBridgeTriggered(bridge, character);
    }
}
