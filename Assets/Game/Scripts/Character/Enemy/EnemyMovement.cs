using NavMesh;
using UnityEngine;

[RequireComponent(typeof(NavmeshAgent))]
public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private NavmeshAgent agent;
    [SerializeField] private float destinationReachedDistance = 0.2f;
    [SerializeField] private float rotationSpeed = 12f;
    [SerializeField] private float manualBridgeMoveSpeed = 3f;

    private Rigidbody characterRigidbody;
    private bool didManualMoveThisFrame;
    private bool isCrossingBridge;
    private bool isTransformDrivenMovement;

    public bool IsCrossingBridge => isCrossingBridge;
    public bool IsTransformDrivenMovement => isTransformDrivenMovement || isCrossingBridge;
    public bool IsAgentEnabled => agent != null && agent.IsEnabled;
    public bool IsMoving => isCrossingBridge || didManualMoveThisFrame || (agent != null && agent.Velocity.sqrMagnitude > 0.05f);

    public bool IsNearDestination =>
        agent != null
        && !agent.PathPending
        && agent.RemainingDistance <= Mathf.Max(agent.StoppingDistance, destinationReachedDistance);

    private void Awake() => ResolveAgent();
    private void OnValidate() => ResolveAgent();

    public void OnInit(Rigidbody targetRigidbody)
    {
        ResolveAgent();
        characterRigidbody = targetRigidbody;
        PrepareRigidbody();
        agent.ConfigureForEnemy();
        ResetState();
    }

    public void ResetState()
    {
        didManualMoveThisFrame = false;
        isCrossingBridge = false;
        isTransformDrivenMovement = false;
    }

    public bool IsAtPosition(Vector3 position) => Vector3.Distance(transform.position, position) <= destinationReachedDistance;
    public void BeginFrame() => didManualMoveThisFrame = false;
    public void StopAgentAtCurrentPosition() => agent?.StopAtCurrentPosition();
    public void PauseAgentAtCurrentPosition() => agent?.PauseAtCurrentPosition();
    public void ResumeAgentAtCurrentPosition() => agent?.ResumeAtCurrentPosition();
    public void SetDestination(Vector3 destination) => agent?.SetDestination(destination);
    public void DisableAgentMovement() => agent?.DisableMovement();
    public void DisableAgent() => agent?.DisableAgent();
    public void SetTransformDrivenMovement(bool isActive) => isTransformDrivenMovement = isActive;
    public void SyncNextPosition() => agent?.SyncNextPosition();
    public bool CanReach(Vector3 point, float sampleDistance) => agent != null && agent.CanReach(point, sampleDistance);
    public bool TrySnapToNavMesh(Vector3 point, float maxDistance) => agent != null && agent.TrySnapToNavMesh(point, maxDistance);

    public void EnableAgentMovement()
    {
        agent?.EnableMovement();
        isTransformDrivenMovement = false;
    }

    public void SetCrossingBridge(bool isActive)
    {
        isCrossingBridge = isActive;
        isTransformDrivenMovement = isActive || isTransformDrivenMovement;
    }

    public void StopRigidbody()
    {
        if (characterRigidbody == null || characterRigidbody.isKinematic) return;

        characterRigidbody.linearVelocity = Vector3.zero;
        characterRigidbody.angularVelocity = Vector3.zero;
    }

    public void MoveManually(Vector3 direction, float speed = -1f)
    {
        if (direction.sqrMagnitude <= 0.001f) return;

        Vector3 normalizedDirection = direction.normalized;
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(normalizedDirection),
            Time.deltaTime * rotationSpeed);

        float resolvedSpeed = speed > 0f ? speed : manualBridgeMoveSpeed;
        transform.position += normalizedDirection * resolvedSpeed * Time.deltaTime;
        didManualMoveThisFrame = true;
        SyncNextPosition();
    }

    public void UpdateRotation()
    {
        if (agent == null || didManualMoveThisFrame || isCrossingBridge || !agent.UpdatePosition || agent.IsStopped) return;

        Vector3 velocity = Flatten(agent.Velocity);
        if (velocity.sqrMagnitude <= 0.001f) return;

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(velocity.normalized),
            Time.deltaTime * rotationSpeed);
    }

    public bool TrySamplePosition(Vector3 point, float maxDistance, out Vector3 sampledPosition)
    {
        sampledPosition = point;
        return agent != null && agent.TrySamplePosition(point, maxDistance, out sampledPosition);
    }

    private void PrepareRigidbody()
    {
        StopRigidbody();
        if (characterRigidbody == null) return;

        characterRigidbody.useGravity = false;
        characterRigidbody.isKinematic = true;
    }

    private void ResolveAgent()
    {
        if (agent == null) agent = GetComponent<NavmeshAgent>();
    }

    private static Vector3 Flatten(Vector3 vector)
    {
        vector.y = 0f;
        return vector;
    }
}
