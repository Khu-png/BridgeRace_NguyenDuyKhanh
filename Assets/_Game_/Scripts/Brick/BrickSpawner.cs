using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrickSpawner : MonoBehaviour
{
    [Header("Pool")]
    [SerializeField] private string brickKey = "SpawnBrick";
    [SerializeField] private float delay = 0.1f;

    [Header("Grid")]
    [SerializeField] private Transform root;
    [SerializeField] private int width = 10;
    [SerializeField] private int height = 10;
    [SerializeField] private float spacing = 1.2f;

    [Header("Characters (Players & Enemies)")]
    [SerializeField] private List<Character> characters = new List<Character>();
    [SerializeField] private StageController stageController;

    private readonly List<Vector3> positions = new List<Vector3>();
    private readonly HashSet<Vector3> emptyPositions = new HashSet<Vector3>();
    private readonly Dictionary<Character, int> characterBrickCount = new Dictionary<Character, int>();
    private readonly Dictionary<Character, HashSet<Brick>> spawnedBricksByCharacter = new Dictionary<Character, HashSet<Brick>>();
    private readonly List<Brick> remainingBricksBuffer = new List<Brick>();

    private int maxBricksPerCharacter;
    private bool isInitialized;

    private bool IsGameplayActive()
    {
        return GameManager.Instance != null && GameManager.Instance.IsPlaying;
    }

    private void Awake()
    {
        if (stageController == null)
        {
            stageController = GetComponentInParent<StageController>();
        }
    }

    private void Start()
    {
        GenerateGrid();
        RecalculateMaxBricksPerCharacter();
        StartCoroutine(FillRoutine());
        isInitialized = true;

        int initialCharacterCount = characters.Count;
        for (int i = 0; i < initialCharacterCount; i++)
        {
            RegisterCharacter(characters[i]);
        }
    }

    void GenerateGrid()
    {
        positions.Clear();
        emptyPositions.Clear();

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Vector3 pos = root.position + new Vector3(x * spacing, 0.07f, z * spacing);
                positions.Add(pos);
                emptyPositions.Add(pos);
            }
        }
    }

    void SpawnInitialBricks(Character character)
    {
        if (character == null || maxBricksPerCharacter <= 0) return;

        ShufflePositions();

        for (int i = characterBrickCount[character]; i < maxBricksPerCharacter; i++)
        {
            if (emptyPositions.Count == 0) break;

            int randIndex = Random.Range(0, emptyPositions.Count);
            Vector3 pos = GetRandomEmptyPosition(randIndex);
            SpawnBrick(pos, character);
        }
    }

    void SpawnBrick(Vector3 pos, Character character)
    {
        if (character == null || !emptyPositions.Contains(pos)) return;

        if (!characterBrickCount.ContainsKey(character))
        {
            characterBrickCount[character] = 0;
        }

        if (!spawnedBricksByCharacter.ContainsKey(character))
        {
            spawnedBricksByCharacter[character] = new HashSet<Brick>();
        }

        if (characterBrickCount[character] >= maxBricksPerCharacter) return;

        Brick brick = SimplePool.Get<Brick>(brickKey, pos, Quaternion.identity);
        if (brick == null) return;

        brick.transform.SetParent(root);

        Brick brickScript = brick.GetComponent<Brick>();
        if (brickScript != null)
        {
            brickScript.spawnPos = pos;
            brickScript.SetOwnerColor(character.characterColorType, character.characterColor, character.characterMaterial);
            brickScript.SetSource(this, stageController);
            spawnedBricksByCharacter[character].Add(brickScript);
        }

        emptyPositions.Remove(pos);
        characterBrickCount[character]++;
    }

    public void OnBrickCollected(Vector3 pos, Character character)
    {
        if (!emptyPositions.Contains(pos))
        {
            emptyPositions.Add(pos);

            if (character != null && characterBrickCount.ContainsKey(character))
            {
                characterBrickCount[character]--;
            }
        }

        Brick brick = FindTrackedBrick(pos, character);
        if (brick != null && spawnedBricksByCharacter.ContainsKey(character))
        {
            spawnedBricksByCharacter[character].Remove(brick);
        }
    }

    public void RegisterCharacter(Character character)
    {
        if (character == null) return;

        if (!characters.Contains(character))
        {
            int emptySlotIndex = characters.FindIndex(c => c == null);

            if (emptySlotIndex >= 0)
            {
                characters[emptySlotIndex] = character;
            }
            else
            {
                characters.Add(character);
            }
        }

        if (!characterBrickCount.ContainsKey(character))
        {
            characterBrickCount[character] = 0;
        }

        if (!spawnedBricksByCharacter.ContainsKey(character))
        {
            spawnedBricksByCharacter[character] = new HashSet<Brick>();
        }

        RecalculateMaxBricksPerCharacter();

        if (isInitialized && IsGameplayActive())
        {
            SpawnInitialBricks(character);
        }
    }

    public void UnregisterCharacter(Character character)
    {
        if (character == null) return;

        int characterIndex = characters.FindIndex(c => c == character);
        if (characterIndex >= 0)
        {
            characters[characterIndex] = null;
        }

        DespawnRemainingBricks(character);
        RecalculateMaxBricksPerCharacter();
    }

    IEnumerator FillRoutine()
    {
        while (true)
        {
            if (!IsGameplayActive())
            {
                yield return null;
                continue;
            }

            for (int i = 0; i < characters.Count; i++)
            {
                Character character = characters[i];
                if (character == null)
                {
                    continue;
                }

                if (!characterBrickCount.ContainsKey(character))
                {
                    characterBrickCount[character] = 0;
                }

                if (characterBrickCount[character] < maxBricksPerCharacter && emptyPositions.Count > 0)
                {
                    int randIndex = Random.Range(0, emptyPositions.Count);
                    Vector3 pos = GetRandomEmptyPosition(randIndex);
                    SpawnBrick(pos, character);
                }
            }

            yield return new WaitForSeconds(delay);
        }
    }

    Vector3 GetRandomEmptyPosition(int index)
    {
        int i = 0;
        foreach (Vector3 pos in emptyPositions)
        {
            if (i == index) return pos;
            i++;
        }

        return Vector3.zero;
    }

    void RecalculateMaxBricksPerCharacter()
    {
        int totalSlots = characters.Count;
        maxBricksPerCharacter = totalSlots > 0 ? Mathf.Max(1, positions.Count / totalSlots) : 0;
    }

    void ShufflePositions()
    {
        for (int i = positions.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            Vector3 temp = positions[i];
            positions[i] = positions[randomIndex];
            positions[randomIndex] = temp;
        }
    }

    void DespawnRemainingBricks(Character character)
    {
        if (character == null || !spawnedBricksByCharacter.TryGetValue(character, out HashSet<Brick> bricks)) return;

        remainingBricksBuffer.Clear();
        foreach (Brick brick in bricks)
        {
            if (brick != null && brick.gameObject.activeInHierarchy)
            {
                remainingBricksBuffer.Add(brick);
            }
        }

        for (int i = 0; i < remainingBricksBuffer.Count; i++)
        {
            Brick brick = remainingBricksBuffer[i];
            emptyPositions.Add(brick.spawnPos);
            SimplePool.Return(brick.gameObject);
        }

        remainingBricksBuffer.Clear();
        bricks.Clear();
        characterBrickCount[character] = 0;
    }

    public Brick GetClosestBrick(Vector3 fromPosition, ColorType colorType)
    {
        Brick closestBrick = null;
        float closestDistance = float.MaxValue;

        foreach (HashSet<Brick> brickSet in spawnedBricksByCharacter.Values)
        {
            foreach (Brick brick in brickSet)
            {
                if (brick == null || !brick.gameObject.activeInHierarchy) continue;
                if (brick.ownerColorType != colorType) continue;

                float sqrDistance = (brick.transform.position - fromPosition).sqrMagnitude;
                if (sqrDistance < closestDistance)
                {
                    closestDistance = sqrDistance;
                    closestBrick = brick;
                }
            }
        }

        return closestBrick;
    }

    Brick FindTrackedBrick(Vector3 pos, Character character)
    {
        if (character == null || !spawnedBricksByCharacter.TryGetValue(character, out HashSet<Brick> bricks)) return null;

        foreach (Brick brick in bricks)
        {
            if (brick != null && brick.spawnPos == pos)
            {
                return brick;
            }
        }

        return null;
    }
}
