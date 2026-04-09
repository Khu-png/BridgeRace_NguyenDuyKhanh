using UnityEngine;

public class PlayerCollector : MonoBehaviour
{
    [SerializeField] private Color characterColor;

    private Character _character;

    private void Awake()
    {
        _character = GetComponentInParent<Character>();
        if (_character == null)
            Debug.LogError("No Character component found in parent!");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Brick")) return;

        Brick brick = other.GetComponent<Brick>();
        if (brick == null) return;
        
        if (Vector4.Distance(brick.ownerColor, characterColor) > 0.01f) return;
        
        _character.CollectBrick();
        
        BrickSpawner spawner = FindFirstObjectByType<BrickSpawner>();
        if (spawner != null)
        {
            spawner.OnBrickCollected(brick.spawnPos, _character); 
        }
        
        SimplePool.Despawn(brick.gameObject);
    }
}