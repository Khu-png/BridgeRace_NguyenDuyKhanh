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
}
