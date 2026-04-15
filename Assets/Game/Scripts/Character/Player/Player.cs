using UnityEngine;


[RequireComponent(typeof(Rigidbody), typeof (CapsuleCollider))]
public class Player : Character
{
    
    [Header("Movement")]
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private FixedJoystick _joystick;
    [SerializeField] private Animator _animator;

    [SerializeField] private float _moveSpeed;

    private bool isRunning = false;

    private void FixedUpdate()
    {
        RefreshKnockdownState();

        if (IsStunned)
        {
            _rigidbody.linearVelocity = Vector3.zero;
            return;
        }

        Vector3 direction = new Vector3(_joystick.Horizontal, 0, _joystick.Vertical);

        if (direction.magnitude > 1f)
            direction.Normalize();
        
        Vector3 targetVelocity = new Vector3(
            direction.x * _moveSpeed,
            _rigidbody.linearVelocity.y,
            direction.z * _moveSpeed
        );

        _rigidbody.linearVelocity = Vector3.Lerp(
            _rigidbody.linearVelocity,
            targetVelocity,
            Time.fixedDeltaTime * 10f
        );
        
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.fixedDeltaTime * 15f
            );
        }
        
        bool isMoving = direction.sqrMagnitude > 0.001f;

        if (isMoving && !isRunning)
        {
            _animator.SetTrigger("Run");
            isRunning = true;
        }
        else if (!isMoving && isRunning)
        {
            _animator.SetTrigger("Idle");
            isRunning = false;
        }
    }
}
