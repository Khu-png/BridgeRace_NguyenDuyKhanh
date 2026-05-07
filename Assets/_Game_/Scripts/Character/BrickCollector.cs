using UnityEngine;

public class PlayerCollector : MonoBehaviour
{
    [SerializeField] private Character _character;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Brick")) return;

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
