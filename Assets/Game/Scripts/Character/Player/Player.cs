using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class Player : Character
{
    public static bool CanMove { get; set; } = true;

    [Header("Movement")]
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private FixedJoystick _joystick;
    [SerializeField] private Animator _animator;
    [SerializeField] private float _moveSpeed;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float groundCheckHeight = 0.5f;
    [SerializeField] private float groundCheckDistance = 1.5f;
    [SerializeField] private float edgeStopPadding = 0.1f;

    private bool isRunning;

    protected override void Start()
    {
        base.Start();

        if (_rigidbody == null)
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        if (_animator == null)
        {
            _animator = GetComponentInChildren<Animator>();
        }

        if (_joystick == null)
        {
            _joystick = FindFirstObjectByType<FixedJoystick>();
        }
    }

    private void FixedUpdate()
    {
        if (_rigidbody == null)
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        if (_animator == null)
        {
            _animator = GetComponentInChildren<Animator>();
        }

        if (_joystick == null)
        {
            _joystick = FindFirstObjectByType<FixedJoystick>();
        }

        if (_rigidbody == null || _animator == null || _joystick == null)
        {
            return;
        }

        RefreshKnockdownState();

        if (HasReachedGoal)
        {
            _rigidbody.linearVelocity = Vector3.zero;
            return;
        }

        if (IsStunned)
        {
            _rigidbody.linearVelocity = Vector3.zero;
            isRunning = false;
            return;
        }

        if (!CanMove)
        {
            _rigidbody.linearVelocity = Vector3.zero;

            if (isRunning)
            {
                _animator.SetTrigger("Idle");
                isRunning = false;
            }

            return;
        }

        Vector3 direction = new Vector3(_joystick.Horizontal, 0f, _joystick.Vertical);

        if (direction.magnitude > 1f)
        {
            direction.Normalize();
        }

        Vector3 filteredDirection = FilterDirectionByGround(direction);

        Vector3 targetVelocity = new Vector3(
            filteredDirection.x * _moveSpeed,
            _rigidbody.linearVelocity.y,
            filteredDirection.z * _moveSpeed
        );

        _rigidbody.linearVelocity = Vector3.Lerp(
            _rigidbody.linearVelocity,
            targetVelocity,
            Time.fixedDeltaTime * 10f
        );

        if (filteredDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(filteredDirection);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.fixedDeltaTime * 15f
            );
        }

        bool isMoving = filteredDirection.sqrMagnitude > 0.001f;

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

    protected override void StopForGoal()
    {
        CanMove = false;
        ResetMovementState();
    }

    public void ResetMovementState()
    {
        if (_rigidbody == null)
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        if (_animator == null)
        {
            _animator = GetComponentInChildren<Animator>();
        }

        if (_joystick == null)
        {
            _joystick = FindFirstObjectByType<FixedJoystick>();
        }

        if (_rigidbody != null)
        {
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
        }

        _joystick?.ResetInput();

        if (isRunning && _animator != null)
        {
            _animator.SetTrigger("Idle");
        }

        isRunning = false;
    }

    private Vector3 FilterDirectionByGround(Vector3 direction)
    {
        if (direction.sqrMagnitude <= 0.001f) return Vector3.zero;
        if (groundMask.value == 0) return direction;

        float filteredX = direction.x;
        float filteredZ = direction.z;

        if (Mathf.Abs(direction.x) > 0.001f && !CanMoveAxis(new Vector3(direction.x, 0f, 0f)))
        {
            filteredX = 0f;
        }

        if (Mathf.Abs(direction.z) > 0.001f && !CanMoveAxis(new Vector3(0f, 0f, direction.z)))
        {
            filteredZ = 0f;
        }

        Vector3 filteredDirection = new Vector3(filteredX, 0f, filteredZ);
        if (filteredDirection.magnitude > 1f)
        {
            filteredDirection.Normalize();
        }

        return filteredDirection;
    }

    private bool CanMoveAxis(Vector3 axisDirection)
    {
        if (groundMask.value == 0) return true;

        Vector3 axisVelocity = axisDirection.normalized * _moveSpeed;
        Vector3 nextPosition = transform.position + axisVelocity * Time.fixedDeltaTime;
        nextPosition += axisDirection.normalized * edgeStopPadding;

        Vector3 rayOrigin = nextPosition + Vector3.up * groundCheckHeight;
        return Physics.Raycast(rayOrigin, Vector3.down, groundCheckDistance, groundMask);
    }
}
