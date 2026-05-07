using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Goal : MonoBehaviour
{
    [SerializeField] private CameraFollow cameraFollow;
    [SerializeField] private Transform goalRoot;

    private Character topOne;

    private void Awake()
    {
        ResolveCameraFollow();
    }

    private void Reset()
    {
        Collider goalCollider = GetComponent<Collider>();
        if (goalCollider != null)
        {
            if (goalCollider is MeshCollider meshCollider)
            {
                if (meshCollider.convex)
                {
                    meshCollider.isTrigger = true;
                }
            }
            else
            {
                goalCollider.isTrigger = true;
            }
        }

        ResolveCameraFollow();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (topOne != null)
        {
            return;
        }

        Character character = other.GetComponentInParent<Character>();
        if (character == null)
        {
            return;
        }

        topOne = character;
        topOne.ReachGoal(goalRoot != null ? goalRoot : transform);

        if (topOne is Enemy topEnemy)
        {
            topEnemy.StopAllMovement();
        }

        StopAllEnemies();
        FocusCameraOnTopOne();
        ResolveGameResult(topOne);
    }

    private void StopAllEnemies()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        for (int i = 0; i < enemies.Length; i++)
        {
            Enemy enemy = enemies[i];
            if (enemy == null || enemy == topOne)
            {
                continue;
            }

            enemy.StopAllMovement();
        }
    }

    private void FocusCameraOnTopOne()
    {
        if (topOne == null)
        {
            return;
        }

        ResolveCameraFollow();

        if (cameraFollow != null)
        {
            cameraFollow.SetTarget(topOne.transform);
        }
    }

    private void ResolveGameResult(Character character)
    {
        if (character == null || LevelManager.Instance == null)
        {
            return;
        }

        if (character.CompareTag("Player"))
        {
            LevelManager.Instance.OnWin();
            return;
        }

        if (character.CompareTag("Enemy"))
        {
            LevelManager.Instance.OnLose();
        }
    }

    private void ResolveCameraFollow()
    {
        if (cameraFollow == null)
        {
            if (Camera.main != null)
            {
                cameraFollow = Camera.main.GetComponent<CameraFollow>();
            }

            if (cameraFollow == null)
            {
                cameraFollow = FindFirstObjectByType<CameraFollow>();
            }
        }
    }
}
