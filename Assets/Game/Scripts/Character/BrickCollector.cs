using UnityEngine;

public class PlayerCollector : MonoBehaviour
{
    [SerializeField] private Color characterColor;

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
        
        if (Vector4.Distance(brick.ownerColor, characterColor) > 0.01f) return;
        
        Vector3 pickupPosition = brick.transform.position;
        _character.CollectBrick(pickupPosition);
        
        BrickSpawner spawner = brick.sourceSpawner;
        if (spawner != null)
        {
            spawner.OnBrickCollected(brick.spawnPos, _character); 
        }
        
        SimplePool.Despawn(brick.gameObject);
    }
}
