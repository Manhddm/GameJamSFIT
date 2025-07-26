using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour
{
    // Enum định nghĩa các trạng thái của nhân vật
    enum PlayerState
    {
        PlayerIdle,
        PlayerWalk,
        PlayerRun,
        PlayerJump,
        PlayerFall,
        PlayerRising,
        PlayerAttack01 // Gộp chung trạng thái tấn công
    }

    // Enum định nghĩa các kiểu tấn công có thể có
    enum AttackType
    {
        Sword,
        Bow
    }

    private PlayerState _currentState;
    private PlayerState _previousState;
    private AttackType _currentAttackType = AttackType.Sword; // Mặc định là dùng kiếm

    private Animator _animator;
    private Rigidbody2D _rigidbody;
    private TouchGround _touchGround;
    private AttackSystem _attackSystem;

    [FormerlySerializedAs("_currentSpeed")] [SerializeField] private float currentSpeed;
    public float speed = 5f;
    public float runMultiplier = 2f;

    private Vector2 _moveInput;
    private bool _isFacingRight = true;
    private bool _isRunning = false;
    private bool _isMoving = false;
    private bool _isAttacking = false;
    
    [Header("Movement Smoothing")]
    [Tooltip("Thời gian để làm mượt chuyển động. Giá trị càng nhỏ càng tăng tốc nhanh.")]
    [SerializeField] private float movementSmoothing = 0.05f;
    private Vector2 _velocity = Vector2.zero; // Biến tham chiếu cho hàm SmoothDamp

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;
    [SerializeField] private float coyoteTime = 0.2f;
    [SerializeField] private float jumpBufferTime = 0.2f;
    [SerializeField] private int maxAirJumps = 1;

    [Header("Attack Settings")]
    [SerializeField] private MeleeHitbox meleeHitbox;
    // **THAY ĐỔI**: Xóa biến meleeAttackDuration, thay bằng biến tự động đọc độ dài animation
    private float _meleeAttackAnimLength;

    //Jump variables
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
        currentSpeed = speed;

        if (meleeHitbox != null)
        {
            meleeHitbox.gameObject.SetActive(false);
        }

        // **THAY ĐỔI**: Tự động tìm và lưu lại độ dài của animation tấn công
        // Điều này đảm bảo thời gian tấn công luôn đồng bộ với animation.
        foreach(var clip in _animator.runtimeAnimatorController.animationClips)
        {
            if(clip.name == "PlayerAttack01")
            {
                _meleeAttackAnimLength = clip.length;
                break;
            }
        }
        if (_meleeAttackAnimLength == 0)
        {
            Debug.LogWarning("Không tìm thấy Animation Clip 'PlayerAttack01'. Sử dụng giá trị mặc định 0.5s. Tấn công có thể không đồng bộ. Hãy chắc chắn tên clip trong Animator khớp chính xác.");
            _meleeAttackAnimLength = 0.5f; // Giá trị dự phòng nếu không tìm thấy clip
        }
    }

    void Update()
    {
        UpdateSpeed();
        UpdateJumpTimer();
        UpdatePlayerState();
        UpdateAnimation();
        
        _previousState = _currentState;
    }

    void FixedUpdate()
    {
        Move();
        HandleJump();
        ApplyBetterJumpPhysics();
    }

    #endregion

    #region Movement Logic

    private void Move()
    {
        if (_isAttacking)
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
        if (_isAttacking) return;

        _isMoving = Mathf.Abs(_moveInput.x) > 0.1f;
        if (!_touchGround.IsGrounded)
        {
            if (_rigidbody.velocity.y > 0.1f)
            {
                SetState(PlayerState.PlayerJump);
            }
            else if (_rigidbody.velocity.y < -0.1f)
            {
                SetState(PlayerState.PlayerFall);
            }
        }
        else
        {
            bool isMovingIntoWall = (_moveInput.x > 0 && _touchGround.IsOnRightWall) || (_moveInput.x < 0 && _touchGround.IsOnLeftWall);

            if (!_isMoving || isMovingIntoWall)
            {
                SetState(PlayerState.PlayerIdle);
            }
            else
            {
                if (_isRunning)
                {
                    SetState(PlayerState.PlayerRun);
                }
                else
                {
                    SetState(PlayerState.PlayerWalk);
                }
            }
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
            if (_isRunning)
            {
                currentSpeed = speed * runMultiplier;
            }
            else
            {
                currentSpeed = speed;
            }
        }
    }

    private void UpdateAnimation()
    {
        if (_currentState != _previousState)
        {
            string animationToPlay = _currentState.ToString();
            _animator.Play(animationToPlay);
        }
    }

    #endregion

    #region Jump Logic
    private void UpdateJumpTimer() { if (_touchGround.IsGrounded) { _coyoteTimeCounter = coyoteTime; } else { _coyoteTimeCounter -= Time.deltaTime; } if (_jumpBufferCounter > 0) { _jumpBufferCounter -= Time.deltaTime; } }
    private void HandleJump() { if (_touchGround.IsGrounded && !_wasGround) { _currentAirJumps = maxAirJumps; } _wasGround = _touchGround.IsGrounded; if (_jumpBufferCounter > 0 && CanJump()) { PerformJump(); _jumpBufferCounter = 0; } }
    private bool CanJump() { return (_coyoteTimeCounter > 0) || (_currentAirJumps > 0); }
    private void PerformJump() { _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, 0f); _rigidbody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse); if (_coyoteTimeCounter <= 0) { _currentAirJumps--; } _coyoteTimeCounter = 0; }
    private void ApplyBetterJumpPhysics() { if (_rigidbody.velocity.y < 0) { _rigidbody.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime; } else if (_rigidbody.velocity.y > 0 && !_isJumpHeld) { _rigidbody.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime; } }
    #endregion

    #region Attack Logic
    
    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.started && CanAttack())
        {
            switch (_currentAttackType)
            {
                case AttackType.Sword:
                    StartCoroutine(MeleeAttackRoutine());
                    break;
                case AttackType.Bow:
                    Debug.Log("Chưa triển khai tấn công bằng cung!");
                    break;
            }
        }
    }

    public void OnSelectWeapon1(InputAction.CallbackContext context)
    {
        if (context.started && !_isAttacking)
        {
            _currentAttackType = AttackType.Sword;
            Debug.Log("Đã chọn: Kiếm");
        }
    }

    public void OnSelectWeapon2(InputAction.CallbackContext context)
    {
        if (context.started && !_isAttacking)
        {
            _currentAttackType = AttackType.Bow;
            Debug.Log("Đã chọn: Cung");
        }
    }

    private bool CanAttack()
    {
        return !_isAttacking && _touchGround.IsGrounded;
    }

    private IEnumerator MeleeAttackRoutine()
    {
        _isAttacking = true;
        SetState(PlayerState.PlayerAttack01);

        // **THAY ĐỔI**: Canh thời gian của coroutine theo độ dài animation đã đọc được
        // Điều này đảm bảo coroutine và animation kết thúc cùng lúc.
        yield return new WaitForSeconds(_meleeAttackAnimLength);

        _isAttacking = false;
    }

    #endregion

    #region Input Management
    public bool IsRunning { get { return _isRunning; } set { _isRunning = value; } }
    public void OnMove(InputAction.CallbackContext context) { _moveInput = context.ReadValue<Vector2>(); if (_moveInput.x != 0) { SetFacing(_moveInput); } }
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
            GUILayout.BeginArea(new Rect(10, 10, 250, 100));
            GUILayout.Label($"Current State: {_currentState}");
            GUILayout.Label($"Current Weapon: {_currentAttackType}");
            GUILayout.Label($"Grounded: {_touchGround.IsGrounded}");
            GUILayout.Label($"Velocity Y: {_rigidbody.velocity.y:F2}");
            GUILayout.EndArea();
        }
    }
    #endregion
}
