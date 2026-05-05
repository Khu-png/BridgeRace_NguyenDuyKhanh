using UnityEngine;

public class BridgeWall : MonoBehaviour
{
    [SerializeField] private Bridge bridge;
    [SerializeField] private StageController nextStage;
    
    public float stepLength = 1f;
    public float stepHeight = 0.2f;
    
    private Vector3 startPosition;
    private int filledCount;
    public Bridge Bridge => bridge;
    public StageController NextStage => nextStage;
    private void Awake()
    {
        startPosition = transform.position;
        
        if (bridge == null)
        {
            bridge = GetComponent<Bridge>() ?? GetComponentInParent<Bridge>();
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
            character.SetCurrentStage(nextStage, bridge.TargetSpawner);
        }

        EndPoint();
    }
    
    public void MoveWallToIndex(int brickIndex)
    {
        if (bridge == null) return;

        filledCount = Mathf.Clamp(brickIndex + 1, 0, bridge.brickCount);
        ApplyFilledCountPosition();
    }

    public void MoveWallToIndexIfAhead(int brickIndex)
    {
        if (bridge == null) return;

        int targetFilledCount = Mathf.Clamp(brickIndex + 1, 0, bridge.brickCount);
        if (targetFilledCount <= filledCount) return;

        filledCount = targetFilledCount;
        ApplyFilledCountPosition();
    }

    public void MoveWallBeforeIndex(int brickIndex)
    {
        if (bridge == null) return;

        filledCount = Mathf.Clamp(brickIndex, 0, bridge.brickCount);
        ApplyFilledCountPosition();
    }

    private void ApplyFilledCountPosition()
    {
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
}
