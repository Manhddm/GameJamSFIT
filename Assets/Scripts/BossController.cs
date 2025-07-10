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
    private Rigidbody2D _rigidbody;
    private Animator _animator;
    private HealthSystem _healthSystem;
    private MovementSystem _movementSystem;
    public DetectionZone _detectionZone;
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
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
        FlipCharacter();
        Movement();
        PlayAnimation();
        
    }
    

    #region Animaiton
    public bool isFacingRight = true;
    private void UpdateAnimation(BossState state)
    {
        _currentState = state;
        
    }

    private void PlayAnimation()
    {
        _animator.Play(_currentState.ToString());
    }

    private void FlipCharacter()
    {
        if ((_movementSystem.IsFacingRight() && !isFacingRight) || (_movementSystem.IsFacingLeft() && isFacingRight))
        {
            isFacingRight = !isFacingRight;
            transform.Rotate(0f,180f,0f);
        }
        
    }

    #endregion
    #region Attack

    private void Attack()
    {
        if (_healthSystem.CurrentHealth / _healthSystem.MaxHealth < 0.4f)
        {
            UpdateAnimation(BossState.Attack2);
        }
        else UpdateAnimation(BossState.Attack1);
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
            else
            {
                _movementSystem.Stop();
                UpdateAnimation(BossState.Idle);
            }
        }
    }
    private void AutoMoving()
    {
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
            _movementSystem.MoveTowards(_player.position);  
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
        yield return new WaitForSeconds(waitTime);
        IsMoving = true;
        _currentWaypoint = (_currentWaypoint + 1) % _waypoints.Count;
        
    }
    #endregion
}