using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TouchGround), typeof(Rigidbody2D), typeof(HealthSystem))]
public class BossController : MonoBehaviour
{
    // Enum định nghĩa các trạng thái của Boss
    private enum BossState
    {
        Idle,
        Walk,
        Chase,
        Attack1,
        Hurt,
        Death
    }

    [Header("Component References")]
    private Rigidbody2D _rb;
    private TouchGround _touchGround;
    private HealthSystem _health; // <-- THAY ĐỔI: Sử dụng HealthSystem
    private Animator _animator;
    private Transform _playerTransform;

    [Header("AI Behavior")]
    [SerializeField] private float _moveSpeed = 2f;
    [SerializeField] private float _sightRange = 12f;
    [SerializeField] private float _attackRange = 2f;
    [Tooltip("Khoảng cách từ vị trí bắt đầu của Boss đến mỗi điểm tuần tra.")]
    [SerializeField] private float _patrolDistance = 7f;
    private Transform _patrolPointA;
    private Transform _patrolPointB;
    private Transform _currentTarget;

    [Header("Attack Settings")]
    [SerializeField] private MeleeHitbox _attackHitbox;
    [SerializeField] private float _attackDamage = 25f;
    [SerializeField] private float _attackCooldown = 2.5f;
    [SerializeField] private LayerMask _playerLayer;

    // State Machine
    private BossState _currentState;
    private bool _isAttacking = false;
    private bool _isHurt = false;
    private bool _isDead = false;
    private float _lastAttackTime;
    
    // Animation Lengths
    private float _attackAnimLength;
    private float _hurtAnimLength;
    private float _deathAnimLength;

    // Movement
    private bool _isFacingRight = true;

    #region MonoBehaviour Methods

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _touchGround = GetComponent<TouchGround>();
        _health = GetComponent<HealthSystem>(); // <-- THAY ĐỔI: Lấy component HealthSystem
        _animator = GetComponent<Animator>();

        InitializePatrolPoints();
        CacheAnimationLengths();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("Không tìm thấy đối tượng người chơi! Hãy chắc chắn Player có tag 'Player'.");
        }
        
        // **THÊM MỚI**: Lắng nghe các sự kiện từ HealthSystem
        _health.OnHurt.AddListener(HandleHurt);
        _health.OnDie.AddListener(HandleDeath);
    }

    private void Start()
    {
        _currentTarget = _patrolPointB;
        SwitchState(BossState.Walk);
        if (_attackHitbox != null)
        {
            _attackHitbox.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (_isDead) return;
        RunStateMachine();
    }

    private void FixedUpdate()
    {
        if (_isDead || _isHurt || _isAttacking)
        {
            _rb.velocity = new Vector2(0, _rb.velocity.y);
            return;
        }
        HandleMovement();
    }

    #endregion

    #region Initialization
    
    private void InitializePatrolPoints()
    {
        GameObject patrolPointsContainer = new GameObject("PatrolPoints_" + gameObject.name);
        patrolPointsContainer.transform.SetParent(this.transform.parent, true);

        _patrolPointA = new GameObject("PatrolPoint_A").transform;
        _patrolPointA.position = transform.position - new Vector3(_patrolDistance, 0, 0);
        _patrolPointA.SetParent(patrolPointsContainer.transform);

        _patrolPointB = new GameObject("PatrolPoint_B").transform;
        _patrolPointB.position = transform.position + new Vector3(_patrolDistance, 0, 0);
        _patrolPointB.SetParent(patrolPointsContainer.transform);
    }
    
    private void CacheAnimationLengths()
    {
        var clips = _animator.runtimeAnimatorController.animationClips;
        foreach(var clip in clips)
        {
            switch(clip.name)
            {
                case "Attack":
                    _attackAnimLength = clip.length;
                    break;
                case "Hurt":
                    _hurtAnimLength = clip.length;
                    break;
                case "Death":
                    _deathAnimLength = clip.length;
                    break;
            }
        }
    }

    #endregion

    #region State Machine

    private void RunStateMachine()
    {
        // **THAY ĐỔI**: Không cho phép chạy state machine khi đang bị thương
        if (_isHurt || _isAttacking || _playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, _playerTransform.position);

        if (distanceToPlayer <= _attackRange && Time.time >= _lastAttackTime + _attackCooldown)
        {
            StartCoroutine(AttackRoutine());
        }
        else if (distanceToPlayer <= _sightRange)
        {
            SwitchState(BossState.Chase);
        }
        else
        {
            SwitchState(BossState.Walk);
        }
    }

    private void SwitchState(BossState newState)
    {
        if (_currentState == newState) return;
        _currentState = newState;
        _animator.Play(_currentState.ToString());
    }

    #endregion

    #region Actions & Movement

    private void HandleMovement()
    {
        if (_currentState != BossState.Walk && _currentState != BossState.Chase)
        {
            _rb.velocity = new Vector2(0, _rb.velocity.y);
            return;
        }

        float direction = (_currentState == BossState.Walk) 
            ? (_currentTarget.position.x > transform.position.x ? 1 : -1) 
            : (_playerTransform.position.x > transform.position.x ? 1 : -1);

        if (_currentState == BossState.Walk && Vector2.Distance(transform.position, _currentTarget.position) < 0.5f)
        {
            _currentTarget = (_currentTarget == _patrolPointA) ? _patrolPointB : _patrolPointA;
        }
        
        Flip(direction);
        _rb.velocity = new Vector2(direction * _moveSpeed, _rb.velocity.y);
    }

    private void Flip(float direction)
    {
        if (direction > 0 && !_isFacingRight)
        {
            transform.Rotate(0f, 180f, 0f);
            _isFacingRight = true;
        }
        else if (direction < 0 && _isFacingRight)
        {
            transform.Rotate(0f, 180f, 0f);
            _isFacingRight = false;
        }
    }

    // **THÊM MỚI**: Hàm xử lý khi nhận sự kiện OnHurt
    private void HandleHurt()
    {
        if (_isDead) return;
        StartCoroutine(HurtRoutine());
    }

    // **THÊM MỚI**: Hàm xử lý khi nhận sự kiện OnDie
    private void HandleDeath()
    {
        if (_isDead) return;
        StartCoroutine(DieRoutine());
    }

    #endregion

    #region Coroutines (Attack, Hurt, Death)

    private IEnumerator AttackRoutine()
    {
        _isAttacking = true;
        _lastAttackTime = Time.time;
        SwitchState(BossState.Attack1);
        
        yield return new WaitForSeconds(_attackAnimLength);

        _isAttacking = false;
        // Chỉ chuyển về Idle nếu không bị ngắt bởi trạng thái khác (như Hurt)
        if (!_isHurt)
        {
            SwitchState(BossState.Idle);
        }
    }

    private IEnumerator HurtRoutine()
    {
        _isHurt = true;
        SwitchState(BossState.Hurt);
        
        // Đợi cho đến khi animation bị thương kết thúc
        yield return new WaitForSeconds(_hurtAnimLength); 
        
        _isHurt = false;
        // Chỉ chuyển về Idle nếu không bị ngắt bởi trạng thái khác
        if (!_isAttacking && !_isDead)
        {
            SwitchState(BossState.Idle);
        }
    }


    private IEnumerator DieRoutine()
    {
        _isDead = true;
        SwitchState(BossState.Death);
        GetComponent<Collider2D>().enabled = false;
        _rb.simulated = false;
        
        yield return new WaitForSeconds(_deathAnimLength);
        
        Destroy(gameObject);
    }

    #endregion
    
    #region Animation Events

    public void EnableAttackHitbox()
    {
        if (_attackHitbox != null)
        {
            // **THAY ĐỔI**: Cần đảm bảo MeleeHitbox tìm được HealthSystem trên Player
            _attackHitbox.StartAttack(_attackDamage, _playerLayer);
            _attackHitbox.gameObject.SetActive(true);
        }
    }

    public void DisableAttackHitbox()
    {
        if (_attackHitbox != null)
        {
            _attackHitbox.gameObject.SetActive(false);
        }
    }

    #endregion

    #region Gizmos for Debug

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _sightRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _attackRange);
    }

    #endregion

    #region DeBug GUI

    private void OnGUI()
    {
        if (Application.isPlaying)
        {
            Rect area = new Rect(Screen.width - 260, 10, 250, 100);
            GUILayout.BeginArea(area);
            GUI.Box(new Rect(0, 0, area.width, area.height), "Boss Info");
            
            GUILayout.BeginVertical();
            GUILayout.Space(20);
            GUILayout.Label($"  Máu: {_health.currentHealth} / {_health.maxHealth}");
            GUILayout.Label($"  Trạng thái: {_currentState}");
            GUILayout.Label($"  Bất tử: {_health.isInvincible}"); // Thêm debug
            GUILayout.EndVertical();

            GUILayout.EndArea();
        }
    }

    #endregion
}
