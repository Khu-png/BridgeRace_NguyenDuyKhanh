using UnityEngine;

public class PlayerCollector : MonoBehaviour
{
    private Character _character;

    private void Awake()
    {
        _character = GetComponentInParent<Character>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Brick")) return;
        if (_character == null) return;

        Brick brick = other.GetComponent<Brick>();
        if (brick == null) return;

        if (!brick.CanBeCollectedBy(_character.characterColor)) return;
        
        Vector3 pickupPosition = brick.transform.position;
        _character.CollectBrick(pickupPosition);

        if (_character.CompareTag("Player"))
        {
            AudioManager.Instance?.PlaySFX("Collect");
        }
        
        BrickSpawner spawner = brick.sourceSpawner;
        if (spawner != null)
        {
            spawner.OnBrickCollected(brick.spawnPos, _character); 
        }
        
        SimplePool.Despawn(brick.gameObject);
    }
}
