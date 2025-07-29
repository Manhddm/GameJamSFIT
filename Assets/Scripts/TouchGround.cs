using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Script này chịu trách nhiệm phát hiện va chạm với môi trường (đất, tường, trần nhà)
/// bằng cách sử dụng CapsuleCollider2D.Cast.
/// Nó chỉ cung cấp thông tin, không chứa logic hành vi của nhân vật.
/// </summary>

public class TouchGround : MonoBehaviour
{
    [Header("Bộ lọc va chạm")]
    public ContactFilter2D castFilter;

    [Header("Khoảng cách phát hiện")]
    [SerializeField] private float groundDistance = 0.1f;
    [SerializeField] private float wallDistance = 0.1f;
    [SerializeField] private float ceilingDistance = 0.1f;
    private Vector2 wallCheckDirection => gameObject.transform.localScale.x < 0 ? Vector2.left : Vector2.right;
    
    private Animator _animator;

    // Biến lưu trữ trạng thái va chạm
    private bool _isGrounded;
    private bool _isOnWall;
    private bool _isOnCeiling;
    private bool _isOnRightWall;
    private bool _isOnLeftWall;

    // Mảng để nhận kết quả từ Cast, cấp phát 1 lần để tối ưu
    private readonly RaycastHit2D[] _groundHits = new RaycastHit2D[3];
    private readonly RaycastHit2D[] _wallHits = new RaycastHit2D[3];
    private readonly RaycastHit2D[] _ceilingHits = new RaycastHit2D[1];

    [FormerlySerializedAs("_collider2D")] public CapsuleCollider2D collider2D;

    // Properties để các script khác có thể truy cập (chỉ đọc)
    public bool IsGrounded
    {
        get
        {
            return _isGrounded;
        }
        set
        {
            _isGrounded = value;
            _animator.SetBool(EnumEntity.isGrounded.ToString(), value);
        }
    }

    public bool IsOnWall
    {
        get
        {
            return _isOnWall;
        }
        set
        {
            _isOnWall = value;
            _animator.SetBool(EnumEntity.isOnWall.ToString(), value);
        }
    }

    public bool IsOnCeiling
    {
        get
        {
            return _isOnCeiling;
        }
        set
        {
            _isOnCeiling = value;
            _animator.SetBool(EnumEntity.isOnCeiling.ToString(),value);
        }
    }
    public bool IsOnRightWall => _isOnRightWall;
    public bool IsOnLeftWall => _isOnLeftWall;
    public float WallDirection => _isOnLeftWall ? -1 : (_isOnRightWall ? 1 : 0);

    // Thông tin chi tiết về va chạm
    public RaycastHit2D GroundHit { get; private set; }
    public RaycastHit2D WallHit { get; private set; }


    private void Awake()
    {
        _animator  = GetComponent<Animator>();
        if (collider2D == null)
            collider2D = GetComponent<CapsuleCollider2D>();
    }

    private void FixedUpdate()
    {
        // Reset trạng thái trước mỗi lần kiểm tra

      
        _isOnRightWall = false;
        _isOnLeftWall = false;
        GroundHit = default;
        WallHit = default;

        // Thực hiện kiểm tra
        CheckGroundCollision();
        CheckWallCollision(); // Hàm kiểm tra tường đã được đơn giản hóa
        CheckCeilingCollision();
    }

    private void CheckGroundCollision()
    {
        int hitCount = collider2D.Cast(Vector2.down, castFilter, _groundHits, groundDistance);
        if (hitCount > 0)
        {
            IsGrounded = true;
            GroundHit = _groundHits[0];
        }
        else IsGrounded = false;
    }

    private void CheckWallCollision()
    {

        IsOnWall = collider2D.Cast(wallCheckDirection,castFilter, _wallHits, wallDistance) > 0;
    }

    private void CheckCeilingCollision()
    {
        int hitCount = collider2D.Cast(Vector2.up, castFilter, _ceilingHits, ceilingDistance);
        IsOnCeiling = hitCount > 0;
    }


}