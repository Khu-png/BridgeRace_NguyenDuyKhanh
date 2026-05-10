using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Goal : MonoBehaviour
{
    [SerializeField] private CameraFollow cameraFollow;
    [SerializeField] private Transform goalRoot;

    private Character topOne;

    private void Reset()
    {
        Collider goalCollider = GetComponent<Collider>();
        if (goalCollider is MeshCollider meshCollider)
        {
            if (meshCollider.convex)
            {
                meshCollider.isTrigger = true;
            }
            return;
        }

        goalCollider.isTrigger = true;
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
        topOne.ReachGoal(goalRoot);

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
        if (cameraFollow == null)
        {
            cameraFollow = Camera.main != null ? Camera.main.GetComponent<CameraFollow>() : null;
        }

        cameraFollow?.SetTarget(topOne.transform);
    }

    private void ResolveGameResult(Character character)
    {
        if (character == null)
        {
            return;
        }

        if (character.CompareTag("Player"))
        {
            GameManager.Instance.GameWin();
            return;
        }

        if (character.CompareTag("Enemy"))
        {
            GameManager.Instance.GameLose();
        }
    }
}
