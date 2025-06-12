using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Player : MonoBehaviour
{
    [Header("Components")]
    private Animator _animator;
    [SerializeField] private Rigidbody2D _rigidbody;

    [Header("Movement Settings")]
    [SerializeField] public float _speed = 5f;
    [SerializeField] private float _jumpForce = 10f;
    private float _horizontalInput;
    private bool _facingRight = true;

    [Header("State & Status")]
    private PlayerState _currentState = PlayerState.Idle;
    private bool _isGrounded = true;

    #region MonoBehaviour

    // Start được gọi một lần trước frame đầu tiên
    void Start()
    {
        _animator = GetComponent<Animator>();
        
        // Rigidbody nên được gán sẵn từ Inspector để đảm bảo không bị null
        if (_rigidbody == null)
        {
            _rigidbody = GetComponent<Rigidbody2D>();
        }
    }

    // Update được gọi mỗi frame, lý tưởng cho việc xử lý input
    void Update()
    {
        HandleInput();
        UpdateState();
        UpdateAnimation();
        FlipCharacter();
    }

    // FixedUpdate được gọi theo một chu kỳ thời gian cố định, lý tưởng cho các tính toán vật lý
    void FixedUpdate()
    {
        HandleMovement();
    }

    #endregion

    /// <summary>
    /// Đọc và lưu trữ tất cả các input từ người dùng.
    /// </summary>
    private void HandleInput()
    {
        // Lấy giá trị input trục ngang (-1 cho A, 1 cho D, 0 nếu không nhấn)
        _horizontalInput = Input.GetAxisRaw("Horizontal");

        // Xử lý input tấn công
        if (Input.GetMouseButtonDown(0) && _isGrounded)
        {
            _currentState = PlayerState.Attack;
            
        }

        // Xử lý input nhảy (chỉ thực hiện khi đang trên mặt đất)
        if (Input.GetKeyDown(KeyCode.Space) && _isGrounded)
        {
            Jump();
        }
    }

    /// <summary>
    /// Cập nhật trạng thái của người chơi dựa trên các điều kiện hiện tại.
    /// Ưu tiên: Tấn công > Nhảy > Chạy > Đứng yên.
    /// </summary>
    private void UpdateState()
    {
        // Nếu đang tấn công, không cho phép chuyển sang trạng thái khác
        // (Bạn có thể reset trạng thái này bằng Animation Event khi animation tấn công kết thúc)
        if (_currentState == PlayerState.Attack) return;
        // Xác định trạng thái dựa trên việc có đang ở trên không hay không
        if (!_isGrounded)
        {
            _currentState = PlayerState.JumpUp;
            
        }
        else
        {
            // Nếu trên mặt đất, kiểm tra xem có đang di chuyển không
            if (_horizontalInput != 0)
            {
                _currentState = PlayerState.Run;
              
            }
            else
            {
                _currentState = PlayerState.Idle;
                
            }
        }

    }

    /// <summary>
    /// Cập nhật animation dựa trên trạng thái hiện tại.
    /// </summary>
    private void UpdateAnimation()
    {
        _animator.Play(_currentState.ToString());
    }
    
    /// <summary>
    /// Xử lý di chuyển vật lý của nhân vật.
    /// </summary>
    private void HandleMovement()
    {
        if (_currentState != PlayerState.Attack)
        {
            _rigidbody.velocity = new Vector2(_horizontalInput * _speed, _rigidbody.velocity.y);
        }
        else
        {
            _rigidbody.velocity = new Vector2(0, _rigidbody.velocity.y);
        }
    }
    
    /// <summary>
    /// Áp dụng một lực tức thời để nhân vật nhảy lên.
    /// </summary>
    private void Jump()
    {
        _rigidbody.AddForce(new Vector2(0, _jumpForce), ForceMode2D.Impulse);
        _isGrounded = false; 
    }

    /// <summary>
    /// Lật hướng nhìn của nhân vật.
    /// </summary>
    private void FlipCharacter()
    {
        if ((_horizontalInput > 0 && !_facingRight) || (_horizontalInput < 0 && _facingRight))
        {
            _facingRight = !_facingRight;
            transform.Rotate(0f, 180f, 0f);
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            _isGrounded = true;
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            _isGrounded = false;
        }
    }
    
    public void FinishAttack()
    {
        _currentState = PlayerState.Idle;
    }
}