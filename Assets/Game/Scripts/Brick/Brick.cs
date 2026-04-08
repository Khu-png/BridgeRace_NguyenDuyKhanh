using UnityEngine;

public class Brick : MonoBehaviour
{
    private bool isCollected;

    private BrickSpawner spawner;
    private Vector3 spawnPos;

    public void Init(BrickSpawner spawner, Vector3 pos)
    {
        this.spawner = spawner;
        this.spawnPos = pos;
    }

    private void OnEnable()
    {
        isCollected = false;
    }

    private void OnTriggerEnter(Collider other)
    {   
        Debug.Log("Brick collected");

        if (isCollected) return;
        if (!other.CompareTag("Player")) return;
        
        Player player = other.GetComponent<Player>();
        if (player == null) return;

        isCollected = true;

        player.CollectBrick();
        
        if (spawner != null)
        {
            spawner.OnBrickCollected(spawnPos);
        }

        SimplePool.Despawn(gameObject);
    }
}