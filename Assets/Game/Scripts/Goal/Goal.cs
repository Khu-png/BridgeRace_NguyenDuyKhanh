using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Goal : MonoBehaviour
{
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private Transform goalRoot;

    private Character topOne;

    private void Reset()
    {
        Collider goalCollider = GetComponent<Collider>();
        if (goalCollider != null)
        {
            goalCollider.isTrigger = true;
        }
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
        StopAllEnemies();
        FocusCameraOnTopOne();
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
        if (cinemachineCamera == null || topOne == null)
        {
            return;
        }

        cinemachineCamera.Target.TrackingTarget = topOne.transform;
    }
}
