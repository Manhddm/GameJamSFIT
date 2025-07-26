using System.Collections;
using UnityEngine;

[RequireComponent(typeof(TouchGround), typeof(Rigidbody2D), typeof(EnemyHealth))]
public class BossController : MonoBehaviour
{
    // Enum định nghĩa các trạng thái của Boss
    private enum BossState
    {
        Idle,
        Walk,
        Chase,
        Attack,
        Hurt,
        Death
    }

    [Header("Component References")]
    private Rigidbody2D _rb;
    private TouchGround _touchGround;
    private EnemyHealth _health;
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

    // Movement
    // Giá trị khởi tạo là true, giả định sprite gốc quay mặt sang phải
    private bool _isFacingRight = true;

    #region MonoBehaviour Methods

    private void Awake()
    {
        // Lấy các component cần thiết
        _rb = GetComponent<Rigidbody2D>();
        _touchGround = GetComponent<TouchGround>();
        _health = GetComponent<EnemyHealth>();
        _animator = GetComponent<Animator>();

        // Khởi tạo các điểm tuần tra trong code
        InitializePatrolPoints();

        // Tìm đối tượng người chơi bằng Tag "Player"
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("Không tìm thấy đối tượng người chơi! Hãy chắc chắn Player có tag 'Player'.");
        }
    }

    private void Start()
    {
        // Thiết lập trạng thái ban đầu và mục tiêu tuần tra
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

        // Chạy máy trạng thái
        RunStateMachine();
    }

    private void FixedUpdate()
    {
        if (_isDead || _isHurt || _isAttacking)
        {
             // Đứng yên khi chết, bị thương hoặc đang tấn công
            _rb.velocity = new Vector2(0, _rb.velocity.y);
            return;
        }

        // Xử lý di chuyển vật lý
        HandleMovement();
    }

    #endregion

    #region Initialization
    
    private void InitializePatrolPoints()
    {
        // Tạo một đối tượng cha để chứa các điểm tuần tra cho gọn gàng
        GameObject patrolPointsContainer = new GameObject("PatrolPoints_" + gameObject.name);
        // Đặt nó cùng cấp với Boss trong Hierarchy
        patrolPointsContainer.transform.SetParent(this.transform.parent, true);

        // Điểm A bên trái của vị trí bắt đầu của Boss
        _patrolPointA = new GameObject("PatrolPoint_A").transform;
        _patrolPointA.position = transform.position - new Vector3(_patrolDistance, 0, 0);
        _patrolPointA.SetParent(patrolPointsContainer.transform);

        // Điểm B bên phải của vị trí bắt đầu của Boss
        _patrolPointB = new GameObject("PatrolPoint_B").transform;
        _patrolPointB.position = transform.position + new Vector3(_patrolDistance, 0, 0);
        _patrolPointB.SetParent(patrolPointsContainer.transform);
    }

    #endregion

    #region State Machine

    private void RunStateMachine()
    {
        if (_isHurt || _isAttacking || _playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, _playerTransform.position);

        // Logic chuyển đổi trạng thái
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
        
        // Cập nhật animation tương ứng với trạng thái
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

        float direction = 0;

        if (_currentState == BossState.Walk) // Tuần tra
        {
            direction = (_currentTarget.position.x > transform.position.x) ? 1 : -1;
            if (Vector2.Distance(transform.position, _currentTarget.position) < 0.5f)
            {
                _currentTarget = (_currentTarget == _patrolPointA) ? _patrolPointB : _patrolPointA;
            }
        }
        else // Đuổi theo người chơi
        {
            direction = (_playerTransform.position.x > transform.position.x) ? 1 : -1;
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

    public void OnTakeDamage()
    {
        if (_isDead) return;

        if (_health.currentHealth <= 0)
        {
            StartCoroutine(DieRoutine());
        }
        else
        {
            StartCoroutine(HurtRoutine());
        }
    }

    #endregion

    #region Coroutines (Attack, Hurt, Death)

    private IEnumerator AttackRoutine()
    {
        _isAttacking = true;
        _lastAttackTime = Time.time;
        SwitchState(BossState.Attack);

        yield return new WaitForSeconds(0.5f); 

        if (_attackHitbox != null)
        {
            _attackHitbox.StartAttack(_attackDamage, _playerLayer);
            _attackHitbox.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(0.3f); 

        if (_attackHitbox != null)
        {
            _attackHitbox.gameObject.SetActive(false);
        }

        yield return new WaitForSeconds(0.5f);

        _isAttacking = false;
        SwitchState(BossState.Idle);
    }

    private IEnumerator HurtRoutine()
    {
        _isHurt = true;
        SwitchState(BossState.Hurt);
        yield return new WaitForSeconds(0.5f); 
        _isHurt = false;
        SwitchState(BossState.Idle);
    }

    private IEnumerator DieRoutine()
    {
        _isDead = true;
        SwitchState(BossState.Death);
        GetComponent<Collider2D>().enabled = false;
        _rb.simulated = false;
        
        yield return new WaitForSeconds(2f);
        
        Destroy(gameObject);
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
            GUILayout.EndVertical();

            GUILayout.EndArea();
        }
    }

    #endregion
}
