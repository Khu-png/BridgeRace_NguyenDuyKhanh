using UnityEngine;

public class BridgeWall : MonoBehaviour
{
    [SerializeField] private Bridge bridge;
    [SerializeField] private StageController nextStage;
    
    public float stepLength = 1f;
    public float stepHeight = 0.2f;
    
    private bool isProcessing;
    private Vector3 startPosition;
    
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
        if (bridge == null || character == null || character.BrickCount <= 0) return;

        if (bridge.BuildStep())
        {
            character.RemoveBrick();
            
            GameObject builtBrick = bridge.bricks[bridge.currentIndex - 1];
            foreach (var mr in builtBrick.GetComponentsInChildren<MeshRenderer>())
            {
                mr.material = new Material(mr.material);
                mr.material.color = character.characterColor;
            }
            
            MoveWall();
            
            if (bridge.currentIndex == bridge.brickCount)
            {
                Debug.Log($"{name} completed bridge. nextStage={(nextStage != null ? nextStage.name : "None")}, character={character.name}");
                character.SetCurrentStage(nextStage);
                EndPoint();
            }
        }
    }
    
    public void MoveWall()
    {
        transform.position += transform.forward * stepLength;
        transform.position += transform.up * stepHeight;
    }

    public void EndPoint()
    {
        transform.position = startPosition;
    }
}
