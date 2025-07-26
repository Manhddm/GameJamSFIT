using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(TouchGround), typeof(Rigidbody2D), typeof(EnemyHealth))]
public class BossController : MonoBehaviour
{
    enum BossState
    {
        Walk,
        Idle,
        Death,
        Hurt,
        Attack1,
        Attack2
    }

    [Header("Setting")] 
    public Rigidbody2D _rb;
    public TouchGround _touchGround;
    [SerializeField] private float _moveSpeed;
    
    private float _currentSpeed;
    private float _previousSpeed;
    private bool _isMoving;
    
    private EnemyHealth health;
    private bool _isDead;
    public bool IsDead => _isDead;
    private BossState _currentState;
    private BossState _previousState;

    [Header("Attack setting")] [SerializeField]
    private bool _isAttacking;
    private void Awake()
    {
        health = GetComponent<EnemyHealth>();
        _rb = GetComponent<Rigidbody2D>();
        _touchGround = GetComponent<TouchGround>();
        _currentState = BossState.Idle;
    }

    private void Update()
    {
        _isDead = health.currentHealth <= 0;
    }

    private void FixedUpdate()
    {
        Move();
    }

    #region Movement

    private void Move()
    {
        if (_isAttacking)
        {
            _rb.velocity = new Vector2(0, _rb.velocity.y);
            return;
        }
        // Hoan thien move
        
    }
    

    #endregion    
    #region State

    private void UpdateState()
    {
        if (_isAttacking) return;
        _previousState = _currentState;
        _isMoving = Math.Abs(_rb.velocity.x) > 0.1f;
        bool isMovingInToWall = (_rb.velocity.x > 0f && _touchGround.IsOnRightWall) || (_rb.velocity.x < 0 && _touchGround.IsOnLeftWall);
        if (!_isMoving && isMovingInToWall)
        {
            SetState(BossState.Idle);
        }else if (_isMoving && isMovingInToWall)
        {
            if (_previousSpeed != _currentSpeed && _currentSpeed !=0)
            {
                SetSpeed(0);
            }
            SetState(BossState.Walk);
        }
        else
        {
            if (_previousSpeed != _currentSpeed && _currentSpeed == 0)
            {
                SetSpeed(_moveSpeed);
            }
            SetState(BossState.Walk);
        }
    }

    private void SetState(BossState newState)
    {
        if (_currentState != newState)
        {
            _currentState = newState;
        }
        
    }

    private void SetSpeed(float speed)
    {
        _currentSpeed = speed;
    }

    

    #endregion
    #region Die
    

    #endregion
    
}
