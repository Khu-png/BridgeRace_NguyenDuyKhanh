using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
    [SerializeField] private List<Character> characters; 

    private List<Vector3> positions = new List<Vector3>();
    private HashSet<Vector3> emptyPositions = new HashSet<Vector3>();
    
    private Dictionary<Character, int> characterBrickCount = new Dictionary<Character, int>();
    private int maxBricksPerCharacter;

    private void Start()
    {
        GenerateGrid();
        positions = positions.OrderBy(x => Random.value).ToList();

        SpawnFull();
        StartCoroutine(FillRoutine());
    }

    void GenerateGrid()
    {
        positions.Clear();
        emptyPositions.Clear();

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Vector3 pos = root.position + new Vector3(
                    x * spacing,
                    0.07f,
                    z * spacing
                );

                positions.Add(pos);
                emptyPositions.Add(pos);
            }
        }
    }

    void SpawnFull()
    {
        positions = positions.OrderBy(x => Random.value).ToList();
        
        List<Character> validCharacters = characters.Where(c => c != null).ToList();
        int validCount = validCharacters.Count;
        int totalCharacters = characters.Count;
        int totalPositions = positions.Count;

        if (validCount == 0) return;

        // Tổng số gạch cho tất cả character hợp lệ
        int bricksToSpawn = Mathf.RoundToInt(totalPositions * ((float)validCount / totalCharacters));
        maxBricksPerCharacter = bricksToSpawn / validCount;

        int posIndex = 0;

        foreach (Character character in validCharacters)
        {
            characterBrickCount[character] = 0; // Khởi tạo count

            int numBricks = maxBricksPerCharacter;
            for (int i = 0; i < numBricks; i++)
            {
                if (posIndex >= positions.Count) break;

                Vector3 pos = positions[posIndex];
                SpawnBrick(pos, character);
                posIndex++;
            }
        }
    }

    // Đổi tham số từ Player thành Character
    void SpawnBrick(Vector3 pos, Character character)
    {
        if (character == null || !emptyPositions.Contains(pos)) return;

        if (!characterBrickCount.ContainsKey(character))
            characterBrickCount[character] = 0;

        if (characterBrickCount[character] >= maxBricksPerCharacter)
            return; 

        GameObject brick = SimplePool.Spawn(brickKey, pos, Quaternion.identity);
        brick.transform.SetParent(root);

        Brick brickScript = brick.GetComponent<Brick>();
        if (brickScript != null)
        {
            brickScript.spawnPos = pos;
            brickScript.SetOwnerColor(character.characterColor);
        }

        emptyPositions.Remove(pos);
        characterBrickCount[character]++;
    }

    public void OnBrickCollected(Vector3 pos, Character character)
    {
        if (!emptyPositions.Contains(pos))
        {
            emptyPositions.Add(pos);
            
            if (characterBrickCount.ContainsKey(character))
            {
                characterBrickCount[character]--;
            }
        }
    }

    IEnumerator FillRoutine()
    {
        List<Character> validCharacters = characters.Where(c => c != null).ToList();

        while (true)
        {
            foreach (Character character in validCharacters)
            {
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
        foreach (var pos in emptyPositions)
        {
            if (i == index) return pos;
            i++;
        }
        return Vector3.zero;
    }
}