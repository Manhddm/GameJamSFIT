using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovementSystem : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 15f;
    
    [Header("Physics")]
    [SerializeField] private Rigidbody2D _rb;
    
    [Header("State")]
    [SerializeField] private bool isRunning = false;
    [SerializeField] private bool canMove = true;
    
    // Internal variables
    private Vector2 _currentVelocity;
    private Vector2 _targetVelocity;
    private Vector2 _moveDirection;
    private float _currentSpeed;
    
    // Properties
    public float MoveSpeed 
    { 
        get => moveSpeed; 
        set => moveSpeed = Mathf.Max(value, 0f); 
    }
    
    public float RunSpeed 
    { 
        get => runSpeed; 
        set => runSpeed = Mathf.Max(value, moveSpeed); 
    }
    
    public bool IsRunning 
    { 
        get => isRunning; 
        set => isRunning = value; 
    }
    
    public bool CanMove 
    { 
        get => canMove; 
        set => canMove = value; 
    }
    
    public Vector2 CurrentVelocity => _currentVelocity;
    public Vector2 MoveDirection => _moveDirection;
    public bool IsMoving => _moveDirection.magnitude > 0.1f && canMove;
    public float CurrentSpeed => _currentSpeed;

    #region MonoBehaviour
    
    private void Start()
    {
        if (_rb == null)
        {
            _rb = GetComponent<Rigidbody2D>();
        }
        
        _currentVelocity = Vector2.zero;
        _targetVelocity = Vector2.zero;
    }
    
    private void FixedUpdate()
    {
        UpdateMovement();
    }
    
    #endregion
    
    #region Movement Methods
    
    public void Move(Vector2 direction)
    {
        if (!canMove)
        {
            _moveDirection = Vector2.zero;
            return;
        }
        
        _moveDirection = direction.normalized;
        
        // Determine target speed based on running state
        float targetSpeed = isRunning ? runSpeed : moveSpeed;
        _currentSpeed = targetSpeed;
        
        // Calculate target velocity
        _targetVelocity = _moveDirection * targetSpeed;
    }
    
    public void Stop()
    {
        _moveDirection = Vector2.zero;
        _targetVelocity = Vector2.zero;
    }
    
    public void SetCanMove(bool value)
    {
        canMove = value;
        if (!canMove)
        {
            Stop();
        }
    }
    
    public void SetRunning(bool running)
    {
        isRunning = running;
    }
    
    private void UpdateMovement()
    {
        if (!canMove)
        {
            // Decelerate to stop when can't move
            _currentVelocity = Vector2.MoveTowards(_currentVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
        }
        else
        {
            // Smoothly transition to target velocity
            float accel = (_targetVelocity.magnitude > _currentVelocity.magnitude) ? acceleration : deceleration;
            _currentVelocity = Vector2.MoveTowards(_currentVelocity, _targetVelocity, accel * Time.fixedDeltaTime);
        }
        
        // Apply movement preserving Y velocity for gravity
        Vector2 newVelocity = new Vector2(_currentVelocity.x, _rb.velocity.y);
        _rb.velocity = newVelocity;
    }
    
    #endregion
    
    #region Utility Methods
    
    public void AddForce(Vector2 force, ForceMode2D forceMode = ForceMode2D.Impulse)
    {
        _rb.AddForce(force, forceMode);
    }
    
    public void SetVelocity(Vector2 velocity)
    {
        _rb.velocity = velocity;
        _currentVelocity = new Vector2(velocity.x, 0); // Only track horizontal velocity
    }
    
    public void Teleport(Vector3 position)
    {
        transform.position = position;
        _rb.velocity = Vector2.zero;
        _currentVelocity = Vector2.zero;
        _targetVelocity = Vector2.zero;
    }
    
    #endregion
    
    #region Debug
    
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;
        
        // Draw movement direction
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)(_moveDirection * 2f));
        
        // Draw velocity
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)(_currentVelocity * 0.5f));
    }
    
    #endregion
}