using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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
        PlayerAttack1
    }
    
    private PlayerState _currentState;
    private PlayerState _previousState;
    private Animator _animator;
    private Rigidbody2D _rigidbody;
    private TouchGround _touchGround;
    
    [SerializeField] private float _currentSpeed;
    public float speed = 5f;
    public float runMultiplier = 2f;
    
    private Vector2 _moveInput;
    private bool _isFacingRight = true;
    private bool _isRunning = false;
    private bool _isMoving = false;

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;
    [SerializeField] private float coyoteTime = 0.2f; // Thời gian đệm cho phép nhảy sau khi nhấn nút
    [SerializeField] private float jumpBufferTime = 0.2f; // Thời gian đệm cho phép nhảy sau khi rời mặt đất
    [SerializeField]private int maxAirJumps = 1;

    //Jump variables
    private int _currentAirJumps;
    private bool _isJumpPressed;
    private bool _wasGround;
    private float _coyoteTimeCounter;
    private float _jumpBufferCounter;
    private bool _isJumpHeld;// Biến để kiểm tra xem nút nhảy có đang được giữ hay không

    #region Monobehaviour

    void Awake()
    {
        _currentState = PlayerState.PlayerIdle;
        _animator = GetComponent<Animator>();
        _rigidbody = GetComponent<Rigidbody2D>();
        _touchGround = GetComponent<TouchGround>();
        _currentSpeed = speed;
    }

    void Start()
    {
        
    }

    void Update()
    {
        UpdateJumpTimer();
        UpdatePlayerState();
        UpdateAnimation();
        if (!_touchGround.IsOnWall) Debug.Log(_touchGround.IsOnWall);
    
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
        float moveDirection = _moveInput.x;
        if (_touchGround.IsOnWall && ISMovingTowardsWall(moveDirection))
        {
            // Nếu đang chạm tường và di chuyển về phía tường, không cho di chuyển
            _rigidbody.velocity = new Vector2(0, _rigidbody.velocity.y);
            return;
        }
        // Di chuyển player theo horizontal input
        _rigidbody.velocity = new Vector2(_moveInput.x * _currentSpeed, _rigidbody.velocity.y);
    }
    private bool ISMovingTowardsWall(float moveDirection)
    {
        
        if (_isFacingRight && moveDirection > 0.1f)
        {
            return true; // Moving right towards wall
        }
        else if (!_isFacingRight && moveDirection < -0.1f)
        {
            return true; // Moving left towards wall
        }
        
        return false;
    }

    private void UpdatePlayerState()
    {
        _previousState = _currentState;

        // Kiểm tra xem có đang di chuyển không
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
            else SetState(PlayerState.PlayerRising);
        }
        else
        {

            bool isBlockedByWall = _touchGround.IsOnWall;
            if (!_isMoving || isBlockedByWall)
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
                // Giữ nguyên tốc độ di chuyển trong air
                _currentSpeed = _isRunning ? speed * runMultiplier : speed;
                break;
            default:
                _currentSpeed = speed;
                break;
        }
    }

    private void UpdateAnimation()
    {
        // Chỉ update animation khi state thay đổi để tránh việc gọi Play() liên tục
        if (_currentState != _previousState)
        {
            _animator.Play(_currentState.ToString());
        }
    }

    #endregion
    
    #region Jump Logic

    private void UpdateJumpTimer()
    {
        //Coyote - cho phep nhay trong thoi gian ngan sau khi roi ground
        if (_touchGround.IsGrounded)
        {
            _coyoteTimeCounter = coyoteTime;
        }
        else
        {
            _coyoteTimeCounter -= Time.deltaTime;
        }
        //Jump buffer - ghi nho input nhay trong thoi gian ngan
        if (_jumpBufferCounter > 0)
        {
            _jumpBufferCounter -= Time.deltaTime;
        }
    }

    private void HandleJump()
    {
        //Reset airJump khi cham dat
        if (_touchGround.IsGrounded && !_wasGround)
        {
            _currentAirJumps = maxAirJumps;
        }

        _wasGround = _touchGround.IsGrounded;
        if (_jumpBufferCounter > 0 && CanJump())
        {
            PerformJump();
            _jumpBufferCounter = 0; // Clear buffer
        }
    }

    private bool CanJump()
    {
        // Có thể nhảy nếu:
        // 1. Đang trên ground hoặc trong coyote time
        // 2. Hoặc còn air jumps
        return (_coyoteTimeCounter > 0) || (_currentAirJumps > 0);
    }
    private void PerformJump()
    {
        // Reset vertical velocity trước khi jump
        _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, 0f);
        
        // Apply jump force
        _rigidbody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        
        // Consume air jump nếu không còn coyote time
        if (_coyoteTimeCounter <= 0)
        {
            _currentAirJumps--;
        }
        
        // Reset coyote time để tránh double jump không mong muốn
        _coyoteTimeCounter = 0;
    }
    private void ApplyBetterJumpPhysics()
    {
        // Variable height jumping
        if (_rigidbody.velocity.y < 0)
        {
            // Falling faster
            _rigidbody.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (_rigidbody.velocity.y > 0 && !_isJumpHeld)
        {
            // Short jump khi không giữ nút jump
            _rigidbody.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
    }
    #endregion

    #region Input Management

    public bool IsRunning
    {
        get { return _isRunning; }
        set { _isRunning = value; }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
        
        // Xử lý hướng quay mặt
        if (_moveInput.x != 0)
        {
            SetFacing(_moveInput);
        }
    }

    private void SetFacing(Vector2 moveInput)
    {
        bool shouldFaceRight = moveInput.x > 0;
        
        if (shouldFaceRight != _isFacingRight)
        {
            _isFacingRight = shouldFaceRight;
            transform.Rotate(0f, 180f, 0f);
        }
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            _isRunning = !_isRunning;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            _isJumpPressed = true;
            _isJumpHeld = true;
            _jumpBufferCounter = jumpBufferTime; // Set jump buffer
        }
        else if (context.canceled)
        {
            _isJumpPressed = false;
            _isJumpHeld = false;
        }
        

    }
    #endregion
    #region Debug Info

    void OnGUI()
    {
        if (Application.isEditor)
        {
            GUILayout.Label($"Speed: {_currentSpeed:F2}");
            GUILayout.Label($"State: {_currentState}");
            GUILayout.Label($"Grounded: {_touchGround.IsGrounded}");
            GUILayout.Label($"On Wall: {_touchGround.IsOnWall}");
            GUILayout.Label($"On Ceiling: {_touchGround.IsOnCeiling}");
            GUILayout.Label($"Coyote Time: {_coyoteTimeCounter:F2}");
            GUILayout.Label($"Jump Buffer: {_jumpBufferCounter:F2}");
            GUILayout.Label($"Air Jumps: {_currentAirJumps}");
            GUILayout.Label($"Velocity Y: {_rigidbody.velocity.y:F2}");
        }
    }

    #endregion
}