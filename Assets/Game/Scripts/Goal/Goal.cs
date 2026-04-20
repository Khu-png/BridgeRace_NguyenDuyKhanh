using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Goal : MonoBehaviour
{
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private Transform goalRoot;

    private Character topOne;

    private void Awake()
    {
        ResolveCinemachineCamera();
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

        ResolveCinemachineCamera();
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
        foreach (Enemy enemy in enemies)
        {
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

        ResolveCinemachineCamera();

        if (cinemachineCamera != null)
        {
            cinemachineCamera.Target.TrackingTarget = topOne.transform;
            cinemachineCamera.Target.LookAtTarget = topOne.transform;
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

    private void ResolveCinemachineCamera()
    {
        if (cinemachineCamera == null)
        {
            cinemachineCamera = FindFirstObjectByType<CinemachineCamera>();
        }
    }
}
