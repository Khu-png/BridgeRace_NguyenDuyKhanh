using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class Player : Character
{
    public static bool CanMove { get; set; } = true;

    [Header("Movement")]
    [SerializeField] private Joystick _joystick;
    [SerializeField] private float _moveSpeed;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float groundCheckHeight = 0.5f;
    [SerializeField] private float groundCheckDistance = 1.5f;
    [SerializeField] private float edgeStopPadding = 0.1f;
    [SerializeField] private float groundStickSpeed = 1f;
    [SerializeField] private float edgeSlideAngle = 35f;

    private bool isRunning;

    public override void OnInit()
    {
        base.OnInit();

        if (_joystick == null)
        {
            _joystick = FindFirstObjectByType<Joystick>();
        }

        CanMove = true;
        ResetMovementState();
    }

    private void FixedUpdate()
    {
        if (_joystick == null)
        {
            return;
        }

        RefreshKnockdownState();

        if (HasReachedGoal)
        {
            characterRigidbody.linearVelocity = Vector3.zero;
            return;
        }

        if (IsStunned)
        {
            characterRigidbody.linearVelocity = Vector3.zero;
            isRunning = false;
            return;
        }

        if (!CanMove)
        {
            characterRigidbody.linearVelocity = Vector3.zero;

            if (isRunning)
            {
                characterAnimation.SetMoving(false);
                isRunning = false;
            }

            return;
        }

        Vector3 direction = new Vector3(_joystick.Horizontal, 0f, _joystick.Vertical);

        if (direction.magnitude > 1f)
        {
            direction.Normalize();
        }

        Vector3 moveDirection = FilterDirectionByGround(direction);

        bool hasMoveInput = moveDirection.sqrMagnitude > 0.001f;
        bool isGrounded = TryGetGroundNormal(out Vector3 groundNormal);

        Vector3 targetVelocity = hasMoveInput
            ? GetGroundAdjustedVelocity(moveDirection, groundNormal, isGrounded)
            : GetIdleVelocity(isGrounded, groundNormal);

        characterRigidbody.linearVelocity = targetVelocity;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.fixedDeltaTime * 15f
            );
        }

        bool isMoving = hasMoveInput;

        if (isMoving && !isRunning)
        {
            characterAnimation.SetMoving(true);
            isRunning = true;
        }
        else if (!isMoving && isRunning)
        {
            characterAnimation.SetMoving(false);
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
        if (_joystick == null)
        {
            _joystick = FindFirstObjectByType<Joystick>();
        }

        characterRigidbody.linearVelocity = Vector3.zero;
        characterRigidbody.angularVelocity = Vector3.zero;

        _joystick?.ResetInput();

        if (isRunning)
        {
            characterAnimation.SetMoving(false);
        }

        isRunning = false;
    }

    private Vector3 FilterDirectionByGround(Vector3 direction)
    {
        if (direction.sqrMagnitude <= 0.001f) return Vector3.zero;
        if (groundMask.value == 0) return direction;

        if (HasGroundAhead(direction))
        {
            return direction;
        }

        Vector3 rightSlideDirection = Quaternion.AngleAxis(edgeSlideAngle, Vector3.up) * direction;
        if (HasGroundAhead(rightSlideDirection))
        {
            return rightSlideDirection.normalized;
        }

        Vector3 leftSlideDirection = Quaternion.AngleAxis(-edgeSlideAngle, Vector3.up) * direction;
        if (HasGroundAhead(leftSlideDirection))
        {
            return leftSlideDirection.normalized;
        }

        return Vector3.zero;
    }

    private bool HasGroundAhead(Vector3 direction)
    {
        if (groundMask.value == 0) return true;

        Vector3 normalizedDirection = direction.normalized;
        Vector3 nextPosition = transform.position + normalizedDirection * (_moveSpeed * Time.fixedDeltaTime + edgeStopPadding);

        Vector3 rayOrigin = nextPosition + Vector3.up * groundCheckHeight;
        return Physics.Raycast(rayOrigin, Vector3.down, groundCheckDistance, groundMask);
    }

    private Vector3 GetGroundAdjustedVelocity(Vector3 direction, Vector3 groundNormal, bool isGrounded)
    {
        if (groundMask.value == 0 || !isGrounded)
        {
            return new Vector3(direction.x * _moveSpeed, characterRigidbody.linearVelocity.y, direction.z * _moveSpeed);
        }

        Vector3 projectedDirection = Vector3.ProjectOnPlane(direction, groundNormal);
        Vector3 moveDirection = projectedDirection.sqrMagnitude > 0.001f ? projectedDirection.normalized : direction;
        return moveDirection * _moveSpeed + GetGroundStickVelocity(groundNormal);
    }

    private Vector3 GetIdleVelocity(bool isGrounded, Vector3 groundNormal)
    {
        if (isGrounded)
        {
            return GetGroundStickVelocity(groundNormal);
        }

        Vector3 currentVelocity = characterRigidbody.linearVelocity;
        return new Vector3(0f, currentVelocity.y, 0f);
    }

    private Vector3 GetGroundStickVelocity(Vector3 groundNormal)
    {
        return -groundNormal * groundStickSpeed;
    }

    private bool TryGetGroundNormal(out Vector3 groundNormal)
    {
        Vector3 rayOrigin = transform.position + Vector3.up * groundCheckHeight;

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, groundCheckDistance, groundMask))
        {
            groundNormal = hit.normal;
            return true;
        }

        groundNormal = Vector3.up;
        return false;
    }
}
