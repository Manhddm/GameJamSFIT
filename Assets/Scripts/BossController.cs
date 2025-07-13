using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BossState
{
    Idle,
    Phase1,
    Phase2,
    Phase3,
    Walk,
    Attack1,
    Attack2,
    WalkAttack,
    ChasePlayer,
    Hurt,
    Dead
}
[RequireComponent(typeof(HealthSystem))]
public class BossController : MonoBehaviour
{
    private BossState _currentState = BossState.Walk;
    private BossState _previousState = BossState.Idle;
    private Rigidbody2D _rigidbody;
    private Animator _animator;
    private HealthSystem _healthSystem;
    private MovementSystem _movementSystem;
    public DetectionZone _detectionZone;
    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _healthSystem = GetComponent<HealthSystem>();
        _movementSystem = GetComponent<MovementSystem>();
        if (_detectionZone != null)
        {
            _detectionZone.OnPlayerEnter += OnPlayerDetected;
            _detectionZone.OnPlayerExit += OnPlayerLost;
        }
        InitWaypoints();
    }
    
    void Update()
    {
        HandleState();
        FlipCharacter();
        PlayAnimation();
        
    }

    private void FixedUpdate()
    {
        
    }

    #region State Machine

    private void HandleState()
    {
        switch (_currentState)
        {
            case BossState.Idle:
            {
                HandleStateIdle();
                break;
            }
            case BossState.Walk:
            {
                HandleStateWalk();
                break;
            }
            case BossState.ChasePlayer:
                HandleChaseState();
                break;
            case BossState.Attack1:
                HandleAttack();
                break;
        }
    }

    private void HandleChaseState()
    {
        if (!_detectionZone.HasPlayer() || _player == null)
        {
            ChangeState(BossState.Walk);
            return;
        }

     
        if (_canAttack && CanAttackPlayer())
        {
            StartAttack();
            return;
        }
        if (_isMoving)
        {
            FlPlayer();
        }
    }

    private void HandleStateIdle()
    {
        
    }

    private void HandleStateWalk()
    {
        if (_detectionZone.HasPlayer() && _player != null)
        {
            ChangeState(BossState.ChasePlayer);
            return;
        }

        if (_isMoving)
        {
            Movement();
        }
        
    }

    private void ChangeState(BossState newState)
    {
        if (_currentState == newState) return;
        _previousState = _currentState;
        _currentState = newState;
        OnStateEnter(newState);
        
    }

    private void OnStateEnter(BossState state)
    {
        switch (state)
        {
            case BossState.Idle:
                _isMoving = false;
                break;
            case BossState.Walk:
                _isMoving = true;
                _autoMoving = true;
                break;
            case BossState.ChasePlayer:
                _isMoving = true;
                _autoMoving = false;
                break;
            case BossState.Hurt:
                break;
            case BossState.Dead:
                _isMoving = false;
                break;
            case BossState.Attack1:
                _isMoving = false;
                _isAttacking = true;
                _lastAttackTime = Time.time;
                break;
        }
    }
    #endregion
    #region Animaiton
    public bool isFacingRight = true;
    private void UpdateAnimation(BossState state)
    {
        _currentState = state;
        
    }

    private void PlayAnimation()
    {
        _animator.Play(_currentState.ToString());
        if (_currentState == BossState.Idle) _movementSystem.Stop();
    }
    private bool _lastFacingDirection = true;
    private void FlipCharacter()
    {
        
        if (!_movementSystem.IsMoving || _isAttacking) return;
        
        bool shouldFaceRight = _movementSystem.IsFacingRight();
        

        if (shouldFaceRight != _lastFacingDirection)
        {
            _lastFacingDirection = shouldFaceRight;
            isFacingRight = shouldFaceRight;
            transform.Rotate(0f, 180f, 0f);
        }
        
    }

    #endregion
    #region Attack
    private bool _isAttacking = false;
    private float _lastAttackTime;
    private bool _canAttack;

    public bool CanAttackPlayer()
    {
        return !_isAttacking;
    }

    private void HandleAttack()
    {
        if (_isAttacking) return;
        if (_detectionZone.HasPlayer() && _player != null)
        {
            //float distanceToPlayer = Vector2.Distance(transform.position, _player.transform.position);
            StartAttack();
        }
    }

    private void StartAttack()
    {
        if (_isAttacking) return;
        _isAttacking = true;
        _isMoving = false;
        BossState attackType = ChooseAttackState();
        UpdateAnimation(attackType);
        
    }

    private BossState ChooseAttackState()
    {
        float healthPercentage = _healthSystem.CurrentHealth / _healthSystem.MaxHealth;
        if (healthPercentage < 0.5f) return BossState.Attack2;
        return BossState.Attack1;
    }
    #endregion

    #region Events

    private void OnPlayerDetected( Transform player )
    {
        _player = player;
        _autoMoving = false;
        
    }

    private void OnPlayerLost(Transform player)
    {
        _player = null;
        _autoMoving = true;
    }

    public bool IsDie()
    {
        return _healthSystem.IsDead;
    }

    public bool IsHurt()
    {
        return true;
    }
    //Die
    //Hurt
    #endregion

    #region Movement
    private bool _autoMoving = true;

    private List<Transform> _waypoints = new List<Transform>();
    private int _currentWaypoint = 0;
    private float waypointDistance = 0.2f;
    public float holdTime = 2f;
    [SerializeField]private bool _isMoving = true;
    private Transform _player;
    public bool IsMoving
    {
        get { return _isMoving; }
        set
        {
            _isMoving = value;
            if (_isMoving)
            {
                UpdateAnimation(BossState.Walk);
            }
        }
    }
    private void AutoMoving()
    {
        if (_detectionZone.HasPlayer()) return;
        Transform target = _waypoints[_currentWaypoint];
        float distance = Vector2.Distance(transform.position, target.position);
        if (distance > waypointDistance)
        {
            _movementSystem.MoveTowards(target.position);
        }
        else
        {
            StartCoroutine(WaitAtWaypoint(holdTime));
            
        }
        
    }

    private void FlPlayer()
    {
        if (_player != null)
        {
            _movementSystem.MoveTowards(_player);  
        }
    }

    private void Movement()
    {
        if (!IsMoving) return;
        if (_autoMoving)
        {
            AutoMoving();
        }
        else
        {
            FlPlayer();
        }
    }

    private void InitWaypoints()
    {
        GameObject waypointObject = new GameObject($"{gameObject.name}_waypoints");
        GameObject waypoint01 = new GameObject("Waypoint01");
        GameObject waypoint02 = new GameObject("Waypoint02");
        waypoint01.transform.SetParent(waypointObject.transform);
        waypoint02.transform.SetParent(waypointObject.transform);
        waypoint01.transform.position = new Vector3(transform.position.x + 5, transform.position.y, 0);
        waypoint02.transform.position = new Vector3(transform.position.x - 5, transform.position.y, 0);
        _waypoints.Add(waypoint01.transform);
        _waypoints.Add(waypoint02.transform);
    }

    private IEnumerator WaitAtWaypoint(float waitTime)
    {
        IsMoving = false;
        UpdateAnimation(BossState.Idle);
        yield return new WaitForSeconds(waitTime);
        IsMoving = true;
        _currentWaypoint = (_currentWaypoint + 1) % _waypoints.Count;
        
    }
    #endregion
}