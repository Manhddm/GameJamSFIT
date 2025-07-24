using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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

    [SerializeField] private float _currentSpeed;
    public float speed = 5f;
    public float runMultiplier = 2f;

    private Vector2 _moveInput;
    private bool _isFacingRight = true;
    private bool _isRunning = false;
    private bool _isMoving = false;
    private bool _isAttacking = false;

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;
    [SerializeField] private float coyoteTime = 0.2f;
    [SerializeField] private float jumpBufferTime = 0.2f;
    [SerializeField] private int maxAirJumps = 1;

    [Header("Attack Settings")]
    [SerializeField] private GameObject meleeHitboxObject;
    [SerializeField] private float meleeAttackDuration = 0.5f;

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
        _currentSpeed = speed;

        if (meleeHitboxObject != null)
        {
            meleeHitboxObject.SetActive(false);
        }
    }

    void Update()
    {
        UpdateJumpTimer();
        UpdatePlayerState();
        UpdateAnimation();
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

        float targetVelocityX = _moveInput.x * _currentSpeed;

        if ((_moveInput.x > 0f && _touchGround.IsOnRightWall) || (_moveInput.x < 0f && _touchGround.IsOnLeftWall))
        {
            targetVelocityX = 0;
        }

        _rigidbody.velocity = new Vector2(targetVelocityX, _rigidbody.velocity.y);
    }

    private void UpdatePlayerState()
    {
        if (_isAttacking) return;

        _previousState = _currentState;

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
            UpdateSpeed();
        }
    }

    private void UpdateSpeed()
    {
        switch (_currentState)
        {
            case PlayerState.PlayerIdle:
            case PlayerState.PlayerAttack01:
                _currentSpeed = 0f;
                break;
            case PlayerState.PlayerWalk:
                _currentSpeed = speed;
                break;
            case PlayerState.PlayerRun:
                _currentSpeed = speed * runMultiplier;
                break;
            case PlayerState.PlayerJump:
            case PlayerState.PlayerFall:
            case PlayerState.PlayerRising:
                break;
            default:
                _currentSpeed = speed;
                break;
        }
    }

    private void UpdateAnimation()
    {
        if (_currentState != _previousState)
        {
            string animationToPlay = _currentState.ToString();

            // if (_currentState == PlayerState.PlayerAttack1)
            // {
            //     // Tạm thời dùng Idle để không lỗi.
            //     // TODO: Khi có animation tấn công, bạn có thể đổi tên này thành "PlayerAttack"
            //     // hoặc dùng một logic phức tạp hơn để chọn animation dựa trên _currentAttackType.
            //     animationToPlay = "PlayerIdle";
            //     Debug.LogWarning($"Chưa có animation cho state 'PlayerAttack'. Sử dụng 'PlayerIdle' thay thế.");
            // }

            _animator.Play(animationToPlay);
        }
    }

    #endregion

    #region Jump Logic
    // ... (Phần code Jump không thay đổi)
    private void UpdateJumpTimer() { if (_touchGround.IsGrounded) { _coyoteTimeCounter = coyoteTime; } else { _coyoteTimeCounter -= Time.deltaTime; } if (_jumpBufferCounter > 0) { _jumpBufferCounter -= Time.deltaTime; } }
    private void HandleJump() { if (_touchGround.IsGrounded && !_wasGround) { _currentAirJumps = maxAirJumps; } _wasGround = _touchGround.IsGrounded; if (_jumpBufferCounter > 0 && CanJump()) { PerformJump(); _jumpBufferCounter = 0; } }
    private bool CanJump() { return (_coyoteTimeCounter > 0) || (_currentAirJumps > 0); }
    private void PerformJump() { _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, 0f); _rigidbody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse); if (_coyoteTimeCounter <= 0) { _currentAirJumps--; } _coyoteTimeCounter = 0; }
    private void ApplyBetterJumpPhysics() { if (_rigidbody.velocity.y < 0) { _rigidbody.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime; } else if (_rigidbody.velocity.y > 0 && !_isJumpHeld) { _rigidbody.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime; } }
    #endregion

    #region Attack Logic
    
    // Hàm này sẽ được gọi bởi nút tấn công chính (ví dụ: chuột trái)
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
                    // Tạm thời chưa có logic. Khi cần, bạn sẽ gọi coroutine bắn cung ở đây.
                    Debug.Log("Chưa triển khai tấn công bằng cung!");
                    break;
                default:
                    StartCoroutine(MeleeAttackRoutine());
                    break;
            }
        }
    }

    // Hàm này gọi khi nhấn phím 1
    public void OnSelectWeapon1(InputAction.CallbackContext context)
    {
        if (context.started && !_isAttacking)
        {
            _currentAttackType = AttackType.Sword;
            Debug.Log("Đã chọn: Kiếm");
        }
    }

    // Hàm này gọi khi nhấn phím 2
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

        if (meleeHitboxObject != null)
        {
            meleeHitboxObject.SetActive(true);
        }

        yield return new WaitForSeconds(0.2f); 

        if (meleeHitboxObject != null)
        {
            meleeHitboxObject.SetActive(false);
        }

        yield return new WaitForSeconds(meleeAttackDuration - 0.2f); 

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
            GUILayout.Label($"Current State: {_currentState}");
            GUILayout.Label($"Current Weapon: {_currentAttackType}");
            GUILayout.Label($"Grounded: {_touchGround.IsGrounded}");
            GUILayout.Label($"Velocity Y: {_rigidbody.velocity.y:F2}");
        }
    }
    #endregion
}
