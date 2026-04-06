using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof (CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private FixedJoystick _joystick;
    [SerializeField] private Animator _animator;

    [SerializeField] private float _moveSpeed;

    private bool isRunning = false;

    private void FixedUpdate()
    {
        Vector3 direction = new Vector3(_joystick.Horizontal, 0, _joystick.Vertical);
        
        _rigidbody.linearVelocity = new Vector3(
            direction.x * _moveSpeed,
            _rigidbody.linearVelocity.y,
            direction.z * _moveSpeed
        );
        
        if (direction.magnitude > 0.01f) 
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 10f);
        }
        
        bool isMoving = direction.magnitude > 0.01f;
        if (isMoving && !isRunning)
        {
            _animator.SetTrigger("Run");
            isRunning = true;
        }
        if (!isMoving && isRunning)
        {
            _animator.SetTrigger("Idle"); 
            isRunning = false;
        }
    }
}