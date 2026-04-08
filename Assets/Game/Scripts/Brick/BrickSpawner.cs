using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrickSpawner : MonoBehaviour
{
    [Header("Pool")]
    [SerializeField] private string poolKey = "SpawnBrick";
    [SerializeField] private float delay = 0.1f;

    [Header("Grid")]
    [SerializeField] private Transform root;
    [SerializeField] private int width = 10;
    [SerializeField] private int height = 10;
    [SerializeField] private float spacing = 1.2f;

    private List<Vector3> positions = new List<Vector3>();
    private HashSet<Vector3> emptyPositions = new HashSet<Vector3>();

    private void Start()
    {
        GenerateGrid();
        SpawnFull();

        StartCoroutine(FillRoutine());
    }

    // Tạo toàn bộ vị trí grid
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
                emptyPositions.Add(pos); // ban đầu tất cả đều trống
            }
        }
    }

    // Spawn full map
    void SpawnFull()
    {
        foreach (var pos in positions)
        {
            SpawnBrick(pos);
        }
    }

    // Spawn 1 viên tại vị trí cụ thể
    void SpawnBrick(Vector3 pos)
    {
        if (!emptyPositions.Contains(pos)) return;

        GameObject brick = SimplePool.Spawn(poolKey, pos, Quaternion.identity);
        brick.transform.SetParent(root);

        // Gắn lại vị trí cho brick
        Brick brickScript = brick.GetComponent<Brick>();
        if (brickScript != null)
        {
            brickScript.Init(this, pos);
        }

        emptyPositions.Remove(pos);
    }

    // Khi brick bị nhặt
    public void OnBrickCollected(Vector3 pos)
    {
        if (!emptyPositions.Contains(pos))
        {
            emptyPositions.Add(pos);
        }
    }

    // Tự động fill lại gạch
    IEnumerator FillRoutine()
    {
        while (true)
        {
            if (emptyPositions.Count > 0)
            {
                // Lấy random 1 vị trí trống
                int randIndex = Random.Range(0, emptyPositions.Count);

                Vector3 pos = GetRandomEmptyPosition(randIndex);
                SpawnBrick(pos);
            }

            yield return new WaitForSeconds(delay);
        }
    }

    // Lấy phần tử random trong HashSet
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