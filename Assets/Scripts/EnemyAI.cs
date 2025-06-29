using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyState
{
    Idle,
    Patrol,
    Chase,
    Attack,
    Return,
    Stunned
}

public enum EnemyType
{
    Patrol,
    Guard,
    Aggressive
}

[RequireComponent(typeof(MovementSystem))]
public class EnemyAI : MonoBehaviour    
{
    [Header("AI Settings")]
    [SerializeField] private EnemyType enemyType = EnemyType.Patrol;
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float losePlayerRange = 8f;
    [SerializeField] private float fieldOfViewAngle = 60f;
    
    [Header("Patrol Settings")]
    [SerializeField] private Transform waypointContainer;
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private float waitTime = 2f;
    [SerializeField] private float patrolRange = 4f; // For auto-generated waypoints
    
    [Header("Combat Settings")]
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float stunDuration = 1f;
    
    [Header("References")]
    [SerializeField] private LayerMask playerLayer = 1;
    [SerializeField] private LayerMask obstacleLayer = 1;
    
    // Components
    private MovementSystem _movementSystem;
    private Rigidbody2D _rb;
    private Animator _animator;
    private HealthSystem _healthSystem;
    
    // AI State
    private EnemyState _currentState = EnemyState.Idle;
    private Transform _playerTarget;
    private Vector3 _originalPosition;
    private bool _facingRight = true;
    
    // Patrol System
    private List<Transform> _waypoints = new List<Transform>();
    private int _currentWaypointIndex = 0;
    private bool _isWaiting = false;
    
    // Timers
    private float _lastAttackTime;
    private float _stunEndTime;
    private float _lastPlayerSeenTime;
    
    // Properties
    public EnemyState CurrentState => _currentState;
    public bool IsPlayerInRange => _playerTarget != null && Vector2.Distance(transform.position, _playerTarget.position) <= detectionRange;
    public bool IsPlayerInAttackRange => _playerTarget != null && Vector2.Distance(transform.position, _playerTarget.position) <= attackRange;
    
    #region MonoBehaviour
    
    private void Start()
    {
        InitializeComponents();
        InitializeWaypoints();
        _originalPosition = transform.position;
        
        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _playerTarget = player.transform;
        }
        
        SetState(EnemyState.Patrol);
    }
    
    private void Update()
    {
        UpdateAI();
        UpdateAnimation();
        HandleFlipping();
    }
    
    #endregion
    
    #region Initialization
    
    private void InitializeComponents()
    {
        _movementSystem = GetComponent<MovementSystem>();
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _healthSystem = GetComponent<HealthSystem>();
        
        if (_healthSystem != null)
        {
            _healthSystem.onDie.AddListener(OnDeath);
        }
    }
    
    private void InitializeWaypoints()
    {
        _waypoints.Clear();
        
        if (waypointContainer != null)
        {
            // Use provided waypoints
            foreach (Transform child in waypointContainer)
            {
                _waypoints.Add(child);
            }
        }
        else
        {
            // Auto-generate waypoints
            CreateAutoWaypoints();
        }
        
        if (_waypoints.Count == 0)
        {
            // Fallback: create simple left-right patrol
            CreateSimplePatrol();
        }
    }
    
    private void CreateAutoWaypoints()
    {
        GameObject waypointParent = new GameObject($"{gameObject.name}_Waypoints");
        waypointParent.transform.SetParent(transform.parent);
        
        // Create waypoints in a line
        for (int i = 0; i < 3; i++)
        {
            GameObject waypoint = new GameObject($"Waypoint_{i}");
            waypoint.transform.SetParent(waypointParent.transform);
            
            Vector3 position = transform.position;
            position.x += (i - 1) * patrolRange; // -patrolRange, 0, +patrolRange
            waypoint.transform.position = position;
            
            _waypoints.Add(waypoint.transform);
        }
    }
    
    private void CreateSimplePatrol()
    {
        GameObject waypointParent = new GameObject($"{gameObject.name}_SimpleWaypoints");
        waypointParent.transform.SetParent(transform.parent);
        
        // Left waypoint
        GameObject leftWaypoint = new GameObject("LeftWaypoint");
        leftWaypoint.transform.SetParent(waypointParent.transform);
        leftWaypoint.transform.position = transform.position + Vector3.left * 3f;
        _waypoints.Add(leftWaypoint.transform);
        
        // Right waypoint
        GameObject rightWaypoint = new GameObject("RightWaypoint");
        rightWaypoint.transform.SetParent(waypointParent.transform);
        rightWaypoint.transform.position = transform.position + Vector3.right * 3f;
        _waypoints.Add(rightWaypoint.transform);
    }
    
    #endregion
    
    #region AI State Machine
    
    private void UpdateAI()
    {
        // Check for stun
        if (Time.time < _stunEndTime)
        {
            SetState(EnemyState.Stunned);
            return;
        }
        
        // Check for player detection
        bool playerDetected = DetectPlayer();
        
        switch (_currentState)
        {
            case EnemyState.Idle:
                HandleIdleState();
                break;
                
            case EnemyState.Patrol:
                HandlePatrolState(playerDetected);
                break;
                
            case EnemyState.Chase:
                HandleChaseState(playerDetected);
                break;
                
            case EnemyState.Attack:
                HandleAttackState();
                break;
                
            case EnemyState.Return:
                HandleReturnState(playerDetected);
                break;
                
            case EnemyState.Stunned:
                HandleStunnedState();
                break;
        }
    }
    
    private void SetState(EnemyState newState)
    {
        if (_currentState == newState) return;
        
        // Exit current state
        ExitState(_currentState);
        
        // Enter new state
        _currentState = newState;
        EnterState(_currentState);
    }
    
    private void EnterState(EnemyState state)
    {
        switch (state)
        {
            case EnemyState.Idle:
                _movementSystem.Stop();
                break;
                
            case EnemyState.Patrol:
                _movementSystem.SetRunning(false);
                break;
                
            case EnemyState.Chase:
                _movementSystem.SetRunning(true);
                _lastPlayerSeenTime = Time.time;
                break;
                
            case EnemyState.Attack:
                _movementSystem.Stop();
                break;
                
            case EnemyState.Return:
                _movementSystem.SetRunning(false);
                break;
                
            case EnemyState.Stunned:
                _movementSystem.Stop();
                break;
        }
    }
    
    private void ExitState(EnemyState state)
    {
        // Any cleanup needed when exiting states
    }
    
    #endregion
    
    #region State Handlers
    
    private void HandleIdleState()
    {
        if (enemyType != EnemyType.Guard)
        {
            SetState(EnemyState.Patrol);
        }
    }
    
    private void HandlePatrolState(bool playerDetected)
    {
        if (playerDetected && enemyType != EnemyType.Patrol)
        {
            SetState(EnemyState.Chase);
            return;
        }
        
        if (!_isWaiting)
        {
            PatrolMovement();
        }
    }
    
    private void HandleChaseState(bool playerDetected)
    {
        if (!playerDetected)
        {
            // Lost player
            if (Time.time - _lastPlayerSeenTime > 3f) // Wait 3 seconds before giving up
            {
                SetState(EnemyState.Return);
                return;
            }
        }
        else
        {
            _lastPlayerSeenTime = Time.time;
        }
        
        if (IsPlayerInAttackRange && CanAttack())
        {
            SetState(EnemyState.Attack);
            return;
        }
        
        // Chase player
        if (_playerTarget != null)
        {
            Vector2 direction = (_playerTarget.position - transform.position).normalized;
            _movementSystem.Move(direction);
        }
    }
    
    private void HandleAttackState()
    {
        if (!IsPlayerInAttackRange)
        {
            SetState(EnemyState.Chase);
            return;
        }
        
        if (CanAttack())
        {
            PerformAttack();
        }
        else
        {
            SetState(EnemyState.Chase);
        }
    }
    
    private void HandleReturnState(bool playerDetected)
    {
        if (playerDetected && enemyType == EnemyType.Aggressive)
        {
            SetState(EnemyState.Chase);
            return;
        }
        
        // Return to original position
        float distanceToHome = Vector2.Distance(transform.position, _originalPosition);
        if (distanceToHome < 0.5f)
        {
            SetState(EnemyState.Patrol);
            return;
        }
        
        Vector2 direction = (_originalPosition - transform.position).normalized;
        _movementSystem.Move(direction);
    }
    
    private void HandleStunnedState()
    {
        // Just wait for stun to end
        if (Time.time >= _stunEndTime)
        {
            SetState(EnemyState.Idle);
        }
    }
    
    #endregion
    
    #region Movement and Patrol
    
    private void PatrolMovement()
    {
        if (_waypoints.Count == 0) return;
        
        Transform targetWaypoint = _waypoints[_currentWaypointIndex];
        float distanceToWaypoint = Vector2.Distance(transform.position, targetWaypoint.position);
        
        if (distanceToWaypoint <= 0.2f)
        {
            StartCoroutine(WaitAtWaypoint());
        }
        else
        {
            Vector2 direction = (targetWaypoint.position - transform.position).normalized;
            _movementSystem.Move(direction);
        }
    }
    
    private IEnumerator WaitAtWaypoint()
    {
        _isWaiting = true;
        _movementSystem.Stop();
        
        yield return new WaitForSeconds(waitTime);
        
        // Move to next waypoint
        _currentWaypointIndex = (_currentWaypointIndex + 1) % _waypoints.Count;
        _isWaiting = false;
    }
    
    #endregion
    
    #region Player Detection
    
    private bool DetectPlayer()
    {
        if (_playerTarget == null) return false;
        
        float distanceToPlayer = Vector2.Distance(transform.position, _playerTarget.position);
        
        // Check if player is in range
        if (distanceToPlayer > detectionRange) return false;
        
        // Check if player is in field of view
        Vector2 directionToPlayer = (_playerTarget.position - transform.position).normalized;
        Vector2 facingDirection = _facingRight ? Vector2.right : Vector2.left;
        
        float angle = Vector2.Angle(facingDirection, directionToPlayer);
        if (angle > fieldOfViewAngle / 2f) return false;
        
        // Check for obstacles
        RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstacleLayer);
        if (hit.collider != null) return false;
        
        return true;
    }
    
    #endregion
    
    #region Combat
    
    private bool CanAttack()
    {
        return Time.time >= _lastAttackTime + attackCooldown;
    }
    
    private void PerformAttack()
    {
        _lastAttackTime = Time.time;
        
        // Trigger attack animation
        if (_animator != null)
        {
            _animator.SetTrigger(AnimationString.attackTrigger);
        }
        
        // Deal damage to player (this would be handled by animation events or collision detection)
        // For now, just a placeholder
        Debug.Log($"{gameObject.name} attacks!");
    }
    
    public void Stun(float duration)
    {
        _stunEndTime = Time.time + duration;
        SetState(EnemyState.Stunned);
    }
    
    #endregion
    
    #region Animation and Visuals
    
    private void UpdateAnimation()
    {
        if (_animator == null) return;
        
        // Update animation parameters
        _animator.SetBool(AnimationString.isMoving, _movementSystem.IsMoving);
        _animator.SetBool(AnimationString.isRunning, _movementSystem.IsRunning);
        
        // Set state-specific animations
        string stateName = _currentState.ToString();
        
        // Override with specific animations if needed
        if (_currentState == EnemyState.Patrol && _movementSystem.IsMoving)
        {
            stateName = "Walk";
        }
        else if (_currentState == EnemyState.Chase && _movementSystem.IsMoving)
        {
            stateName = "Run";
        }
        
        // Only change animation if it's different
        if (!_animator.GetCurrentAnimatorStateInfo(0).IsName(stateName))
        {
            _animator.Play(stateName);
        }
    }
    
    private void HandleFlipping()
    {
        Vector2 moveDirection = _movementSystem.MoveDirection;
        
        if (moveDirection.x > 0.1f && !_facingRight)
        {
            Flip();
        }
        else if (moveDirection.x < -0.1f && _facingRight)
        {
            Flip();
        }
    }
    
    private void Flip()
    {
        _facingRight = !_facingRight;
        transform.Rotate(0f, 180f, 0f);
    }
    
    #endregion
    
    #region Events
    
    private void OnDeath()
    {
        // Handle death
        _movementSystem.SetCanMove(false);
        enabled = false;
    }
    
    #endregion
    
    #region Debug
    
    private void OnDrawGizmosSelected()
    {
        // Draw detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Draw field of view
        Vector3 facingDirection = _facingRight ? Vector3.right : Vector3.left;
        Vector3 leftBoundary = Quaternion.Euler(0, 0, fieldOfViewAngle / 2f) * facingDirection;
        Vector3 rightBoundary = Quaternion.Euler(0, 0, -fieldOfViewAngle / 2f) * facingDirection;
        
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * detectionRange);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * detectionRange);
        
        // Draw waypoints
        if (_waypoints != null)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < _waypoints.Count; i++)
            {
                if (_waypoints[i] != null)
                {
                    Gizmos.DrawWireSphere(_waypoints[i].position, 0.3f);
                    
                    // Draw path
                    if (i < _waypoints.Count - 1 && _waypoints[i + 1] != null)
                    {
                        Gizmos.DrawLine(_waypoints[i].position, _waypoints[i + 1].position);
                    }
                    else if (i == _waypoints.Count - 1 && _waypoints[0] != null)
                    {
                        Gizmos.DrawLine(_waypoints[i].position, _waypoints[0].position);
                    }
                }
            }
        }
    }
    
    #endregion
}