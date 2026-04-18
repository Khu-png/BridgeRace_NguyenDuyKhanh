using UnityEngine;
using System.Collections.Generic;

public class BridgeWall : MonoBehaviour
{
    [SerializeField] private Bridge bridge;
    [SerializeField] private StageController nextStage;
    [SerializeField] private int maxEnemyBuilders = 2;
    
    public float stepLength = 1f;
    public float stepHeight = 0.2f;
    
    private bool isProcessing;
    private Vector3 startPosition;
    private int filledCount;
    private readonly HashSet<Enemy> reservedEnemies = new HashSet<Enemy>();
    public Bridge Bridge => bridge;
    public StageController NextStage => nextStage;
    private void Awake()
    {
        startPosition = transform.position;
        
        if (bridge == null)
        {
            bridge = GetComponent<Bridge>() ?? GetComponentInParent<Bridge>();
        }

        if (nextStage == null)
        {
            Goal goal = FindFirstObjectByType<Goal>();
            if (goal != null)
            {
                nextStage = goal.GetComponent<StageController>() ?? goal.GetComponentInParent<StageController>();
            }
        }
    }
    
    public void OnBridgeTriggered(Bridge other, Character character)
    {
        TryAdvance(character);
    }

    public void TryAdvance(Character character)
    {
        if (bridge == null || character == null) return;
        if (!bridge.IsFull()) return;
        if (character is Enemy) return;

        bridge.Retire();

        if (nextStage != null)
        {
            character.SetCurrentStage(nextStage);
        }

        EndPoint();
    }
    
    public void MoveWallToIndex(int brickIndex)
    {
        if (bridge == null) return;

        filledCount = Mathf.Clamp(brickIndex + 1, 0, bridge.brickCount);

        if (filledCount >= bridge.brickCount)
        {
            EndPoint();
            return;
        }

        transform.position =
            startPosition +
            transform.forward * (stepLength * filledCount) +
            transform.up * (stepHeight * filledCount);
    }

    public void EndPoint()
    {
        transform.position = startPosition;
        filledCount = 0;
    }

    public void ResetProgressToStart()
    {
        transform.position = startPosition;
        filledCount = 0;
    }

    public bool CanAcceptEnemy(Enemy enemy)
    {
        if (enemy == null) return false;
        if (bridge == null || bridge.IsRetired || bridge.IsFull()) return false;
        if (reservedEnemies.Contains(enemy)) return true;
        return reservedEnemies.Count < maxEnemyBuilders;
    }

    public bool TryReserveEnemySlot(Enemy enemy)
    {
        if (!CanAcceptEnemy(enemy)) return false;
        reservedEnemies.Add(enemy);
        return true;
    }

    public void ReleaseEnemySlot(Enemy enemy)
    {
        if (enemy == null) return;
        reservedEnemies.Remove(enemy);
    }
}
