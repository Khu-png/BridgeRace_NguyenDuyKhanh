using UnityEngine;

public class Bridge : MonoBehaviour
{
    [SerializeField] private string brickKey = "BridgeBrick";
    [SerializeField] private int bridgeLength = 5;
    [SerializeField] private float brickSpacing = 1f;
    [SerializeField] private Vector3 direction = Vector3.forward; 

    private bool isPlaced;

    private void OnTriggerEnter(Collider other)
    {   
        if (!other.CompareTag("Player")) return;
        
        Player player = other.GetComponent<Player>();
        if (player == null) return;

        if (!isPlaced)
        {
            PlaceBridge(transform.position);
            isPlaced = true;
        }   
    }

    private void PlaceBridge(Vector3 startPos)
    {
        for (int i = 0; i < bridgeLength; i++)
        {
            Vector3 spawnPos = startPos + direction * i * brickSpacing;

            GameObject brick = SimplePool.Spawn(brickKey, spawnPos, Quaternion.identity);
            brick.SetActive(true);
        }
    }
}