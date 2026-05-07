using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 8f, -7f);
    [SerializeField] private float followSpeed = 10f;
    [SerializeField] private bool snapOnTargetChanged = true;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;

        if (snapOnTargetChanged)
        {
            SnapToTarget();
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        transform.position = Vector3.Lerp(
            transform.position,
            target.position + offset,
            Time.deltaTime * followSpeed
        );
    }

    private void SnapToTarget()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
        }
    }
}
