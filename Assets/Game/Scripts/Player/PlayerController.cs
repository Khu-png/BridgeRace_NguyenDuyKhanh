using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof (BoxCollider))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private FixedJoystick _joystick;
    [SerializeField] private Animator _animator;

    [SerializeField] private float _moveSpeed;

    private bool isRunning = false;

    private void FixedUpdate()
    {
        _rigidbody.linearVelocity = new Vector3(
            _joystick.Horizontal * _moveSpeed,
            _rigidbody.linearVelocity.y,
            _joystick.Vertical * _moveSpeed
        );

        bool isMoving = _joystick.Horizontal != 0 || _joystick.Vertical != 0;

        if (isMoving)
        {
            transform.rotation = Quaternion.LookRotation(_rigidbody.linearVelocity);
        }

        // chỉ trigger khi bắt đầu chạy
        if (isMoving && !isRunning)
        {
            _animator.SetTrigger("Run");
            isRunning = true;
        }

        // khi dừng → reset trạng thái
        if (!isMoving && isRunning)
        {
            _animator.SetTrigger("Idle"); 
            isRunning = false;
        }
    }
}