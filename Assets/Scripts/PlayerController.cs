using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour
{



    private Vector2 moveInput;
    [SerializeField] private bool _isMoving;


    public bool IsMoving
    {
        get { return _isMoving; }
        set
        {
            _isMoving = value;
            _animator.SetBool(PlayerState.isMoving.ToString(), value);
        }
    }

    [SerializeField] private bool _isRunning;

    public bool IsRunning
    {
        get { return _isRunning; }
        set
        {
            _isRunning = value;
            _animator.SetBool(PlayerState.isRunning.ToString(), value);
        }
    }
    [Header("Setting speed")]
    public float walkSpped = 5f;
    public float runSpped = 10f;

    [Header("Jump setting")]
    [Range(1f,20f)]
    [SerializeField] private float jumpForce = 10f;
    public float CurrentSpeed
    {
        get
        {
            if (CanMove)
            {
                if (IsMoving && !_touch.IsOnWall)
                {
                    if (IsRunning)
                    {
                        return runSpped;
                    }

                    return walkSpped;
                }

            }
            return 0;
        }
    }


    private Rigidbody2D _rb;
    private Animator _animator;
    private TouchGround _touch;

    public bool CanMove
    {
        get
        {
            return _animator.GetBool(EnumEntity.canMove.ToString());
        }

    }

    private bool _isFacingRight = true;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _touch = GetComponent<TouchGround>();
    }

    private void Update()
    {
    }

    private void FixedUpdate()
    {
        Move();

        _animator.SetFloat(PlayerState.yVelocity.ToString(), _rb.velocity.y);
    }

    private void Move()
    {
        _rb.velocity = new Vector2(moveInput.x * CurrentSpeed, _rb.velocity.y);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        IsMoving = moveInput!= Vector2.zero;
        if (IsMoving)
        {
            SetFacing(moveInput);
        }

        if (context.canceled)
        {
            _animator.SetBool(PlayerState.isMoving.ToString(), false);
            
        }
    }

    private void SetFacing(Vector2 moveInput)
    {
        bool shouldFaceRight = moveInput.x > 0;
        if (shouldFaceRight != _isFacingRight)
        {
            _isFacingRight = shouldFaceRight;
            transform.localScale = new Vector3(transform.localScale.x *-1, transform.localScale.y, transform.localScale.z);
        }
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            IsRunning = !_isRunning;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started && _touch.IsGrounded && CanMove)
        {
            _animator.SetTrigger(PlayerState.Jump.ToString());
            _rb.velocity = new Vector2(_rb.velocity.x, jumpForce);
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.started && _touch.IsGrounded)
        {
            _animator.SetTrigger(PlayerState.Attack.ToString());
        }
    }
}