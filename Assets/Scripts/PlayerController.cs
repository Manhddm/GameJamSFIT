using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

[RequireComponent(typeof(HealthSystem))] // <-- THÊM MỚI: Yêu cầu có HealthSystem
public class PlayerController : MonoBehaviour
{
    enum PlayerState
    {
        PlayerIdle,
        PlayerWalk,
        PlayerRun,
        PlayerJump,
        PlayerFall,
        PlayerRising,
        PlayerAttack01,
        PlayerHurt, // <-- THÊM MỚI: Trạng thái bị thương
        PlayerDeath // <-- THÊM MỚI: Trạng thái chết
    }

    enum AttackType { Sword, Bow }

    private PlayerState _currentState;
    private PlayerState _previousState;
    private AttackType _currentAttackType = AttackType.Sword;

    private Animator _animator;
    private Rigidbody2D _rigidbody;
    private TouchGround _touchGround;
    private AttackSystem _attackSystem;
    private HealthSystem _health; // <-- THÊM MỚI: Tham chiếu đến HealthSystem

    [SerializeField] private float currentSpeed;
    public float speed = 5f;
    public float runMultiplier = 2f;

    private Vector2 _moveInput;
    private bool _isFacingRight = true;
    private bool _isRunning = false;
    private bool _isMoving = false;

    // **THÊM MỚI**: Các biến cờ trạng thái mới
    private bool _isAttacking = false;
    private bool _isHurt = false;
    private bool _isDead = false;
    
    [Header("Movement Smoothing")]
    [SerializeField] private float movementSmoothing = 0.05f;
    private Vector2 _velocity = Vector2.zero;

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;
    [SerializeField] private float coyoteTime = 0.2f;
    [SerializeField] private float jumpBufferTime = 0.2f;
    [SerializeField] private int maxAirJumps = 1;

    [Header("Attack Settings")]
    [SerializeField] private MeleeHitbox meleeHitbox;
    private float _meleeAttackAnimLength;
    
    [Header("Hurt & Death Settings")] // <-- THÊM MỚI
    private float _hurtAnimLength;
    private float _deathAnimLength;

    private int _currentAirJumps;
    private bool _isJumpPressed;
    private bool _wasGround;
    private float _coyoteTimeCounter;
    private float _jumpBufferCounter;
    private bool _isJumpHeld;

    #region Monobehaviour

    void Awake()
    {
        _currentState = PlayerState.PlayerIdle;
        _animator = GetComponent<Animator>();
        _rigidbody = GetComponent<Rigidbody2D>();
        _touchGround = GetComponent<TouchGround>();
        _attackSystem = GetComponent<AttackSystem>();
        _health = GetComponent<HealthSystem>(); // <-- THÊM MỚI: Lấy component HealthSystem

        // **THÊM MỚI**: Lắng nghe các sự kiện từ HealthSystem
        _health.OnHurt.AddListener(HandleHurt);
        _health.OnDie.AddListener(HandleDeath);
        
        currentSpeed = speed;

        if (meleeHitbox != null)
        {
            meleeHitbox.gameObject.SetActive(false);
        }
        
        CacheAnimationLengths();
    }
    
    // **THÊM MỚI**: Hàm gom việc lấy độ dài animation vào một chỗ
    void CacheAnimationLengths()
    {
        foreach(var clip in _animator.runtimeAnimatorController.animationClips)
        {
            switch(clip.name)
            {
                case "PlayerAttack01":
                    _meleeAttackAnimLength = clip.length;
                    break;
                case "PlayerHurt": // <-- Tên animation bị thương của Player
                    _hurtAnimLength = clip.length;
                    break;
                case "PlayerDeath": // <-- Tên animation chết của Player
                    _deathAnimLength = clip.length;
                    break;
            }
        }
        
        if (_meleeAttackAnimLength == 0) Debug.LogWarning("Không tìm thấy Animation Clip 'PlayerAttack01'.");
        if (_hurtAnimLength == 0) Debug.LogWarning("Không tìm thấy Animation Clip 'PlayerHurt'.");
        if (_deathAnimLength == 0) Debug.LogWarning("Không tìm thấy Animation Clip 'PlayerDeath'.");
    }

    void Update()
    {
        // **THAY ĐỔI**: Nếu chết thì không làm gì cả
        if (_isDead) return;

        UpdateSpeed();
        UpdateJumpTimer();
        UpdatePlayerState();
        UpdateAnimation();
        
        _previousState = _currentState;
    }

    void FixedUpdate()
    {
        // **THAY ĐỔI**: Nếu chết thì không làm gì cả
        if (_isDead) return;
        
        Move();
        HandleJump();
        ApplyBetterJumpPhysics();
    }

    #endregion

    #region Movement Logic

    private void Move()
    {
        // **THAY ĐỔI**: Không cho di chuyển khi đang bị thương
        if (_isAttacking || _isHurt)
        {
            _rigidbody.velocity = new Vector2(0, _rigidbody.velocity.y);
            return;
        }

        float targetVelocityX = _moveInput.x * currentSpeed;

        if ((_moveInput.x > 0f && _touchGround.IsOnRightWall) || (_moveInput.x < 0f && _touchGround.IsOnLeftWall))
        {
            targetVelocityX = 0;
        }

        Vector2 targetVelocity = new Vector2(targetVelocityX, _rigidbody.velocity.y);
        
        _rigidbody.velocity = Vector2.SmoothDamp(_rigidbody.velocity, targetVelocity, ref _velocity, movementSmoothing);
    }

    private void UpdatePlayerState()
    {
        // **THAY ĐỔI**: Ưu tiên các trạng thái đặc biệt
        if (_isAttacking || _isHurt || _isDead) return;

        _isMoving = Mathf.Abs(_moveInput.x) > 0.1f;
        if (!_touchGround.IsGrounded)
        {
            if (_rigidbody.velocity.y > 0.1f) SetState(PlayerState.PlayerJump);
            else if (_rigidbody.velocity.y < -0.1f) SetState(PlayerState.PlayerFall);
        }
        else
        {
            bool isMovingIntoWall = (_moveInput.x > 0 && _touchGround.IsOnRightWall) || (_moveInput.x < 0 && _touchGround.IsOnLeftWall);
            if (!_isMoving || isMovingIntoWall) SetState(PlayerState.PlayerIdle);
            else SetState(_isRunning ? PlayerState.PlayerRun : PlayerState.PlayerWalk);
        }
    }

    private void SetState(PlayerState newState)
    {
        if (_currentState != newState)
        {
            _currentState = newState;
        }
    }

    private void UpdateSpeed()
    {
        if (_touchGround.IsGrounded)
        {
            currentSpeed = _isRunning ? speed * runMultiplier : speed;
        }
    }

    private void UpdateAnimation()
    {
        if (_currentState != _previousState)
        {
            _animator.Play(_currentState.ToString());
        }
    }

    #endregion

    #region Jump Logic
    private void UpdateJumpTimer() { if (_touchGround.IsGrounded) { _coyoteTimeCounter = coyoteTime; } else { _coyoteTimeCounter -= Time.deltaTime; } if (_jumpBufferCounter > 0) { _jumpBufferCounter -= Time.deltaTime; } }
    private void HandleJump() { if (_touchGround.IsGrounded && !_wasGround) { _currentAirJumps = maxAirJumps; } _wasGround = _touchGround.IsGrounded; if (_jumpBufferCounter > 0 && CanJump()) { PerformJump(); _jumpBufferCounter = 0; } }
    private bool CanJump() { return !_isHurt && !_isAttacking && ((_coyoteTimeCounter > 0) || (_currentAirJumps > 0)); } // **THAY ĐỔI**
    private void PerformJump() { _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, 0f); _rigidbody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse); if (_coyoteTimeCounter <= 0) { _currentAirJumps--; } _coyoteTimeCounter = 0; }
    private void ApplyBetterJumpPhysics() { if (_rigidbody.velocity.y < 0) { _rigidbody.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime; } else if (_rigidbody.velocity.y > 0 && !_isJumpHeld) { _rigidbody.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime; } }
    #endregion

    #region Attack Logic
    
    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.started && CanAttack())
        {
            if (_currentAttackType == AttackType.Sword) StartCoroutine(MeleeAttackRoutine());
            else Debug.Log("Chưa triển khai tấn công bằng cung!");
        }
    }

    public void OnSelectWeapon1(InputAction.CallbackContext context) { if (context.started && !_isAttacking) { _currentAttackType = AttackType.Sword; Debug.Log("Đã chọn: Kiếm"); } }
    public void OnSelectWeapon2(InputAction.CallbackContext context) { if (context.started && !_isAttacking) { _currentAttackType = AttackType.Bow; Debug.Log("Đã chọn: Cung"); } }

    private bool CanAttack()
    {
        // **THAY ĐỔI**: Không cho tấn công khi đang bị thương
        return !_isAttacking && !_isHurt && _touchGround.IsGrounded;
    }

    private IEnumerator MeleeAttackRoutine()
    {
        _isAttacking = true;
        SetState(PlayerState.PlayerAttack01);
        
        yield return new WaitForSeconds(_meleeAttackAnimLength);

        _isAttacking = false;
    }

    public void EnableAttackHitbox() { if (meleeHitbox != null) { meleeHitbox.StartAttack(_attackSystem.meleeDamage, _attackSystem.enemyLayers); meleeHitbox.gameObject.SetActive(true); } }
    public void DisableAttackHitbox() { if (meleeHitbox != null) { meleeHitbox.gameObject.SetActive(false); } }

    #endregion
    
    #region Hurt & Death Logic

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
        StartCoroutine(DeathRoutine());
    }

    private IEnumerator HurtRoutine()
    {
        _isHurt = true;
        SetState(PlayerState.PlayerHurt);
        yield return new WaitForSeconds(_hurtAnimLength);
        _isHurt = false;
    }

    private IEnumerator DeathRoutine()
    {
        _isDead = true;
        SetState(PlayerState.PlayerDeath);
        _rigidbody.simulated = false; // Tắt vật lý
        this.enabled = false; // Tắt script controller này
        yield return new WaitForSeconds(_deathAnimLength);
        // Ở đây bạn có thể thêm logic hiển thị màn hình Game Over
        Debug.Log("GAME OVER");
    }

    #endregion

    #region Input Management
    public bool IsRunning { get { return _isRunning; } set { _isRunning = value; } }
    public void OnMove(InputAction.CallbackContext context) { if(!_isHurt && !_isDead) { _moveInput = context.ReadValue<Vector2>(); if (_moveInput.x != 0) { SetFacing(_moveInput); } } }
    private void SetFacing(Vector2 moveInput) { bool shouldFaceRight = moveInput.x > 0; if (shouldFaceRight != _isFacingRight) { _isFacingRight = shouldFaceRight; transform.Rotate(0f, 180f, 0f); } }
    public void OnRun(InputAction.CallbackContext context) { if (context.started) { _isRunning = !_isRunning; } }
    public void OnJump(InputAction.CallbackContext context) { if (context.started) { _isJumpPressed = true; _isJumpHeld = true; _jumpBufferCounter = jumpBufferTime; } else if (context.canceled) { _isJumpPressed = false; _isJumpHeld = false; } }
    #endregion

    #region Debug Info
    void OnGUI()
    {
        if (Application.isEditor)
        {
            GUI.color = Color.black;
            GUILayout.BeginArea(new Rect(10, 10, 250, 150));
            GUILayout.Label($"Máu: {_health.currentHealth} / {_health.maxHealth}");
            GUILayout.Label($"Current State: {_currentState}");
            GUILayout.Label($"Current Weapon: {_currentAttackType}");
            GUILayout.Label($"Grounded: {_touchGround.IsGrounded}");
            GUILayout.Label($"Bất tử: {_health.isInvincible}");
            GUILayout.EndArea();
        }
    }
    #endregion
}
