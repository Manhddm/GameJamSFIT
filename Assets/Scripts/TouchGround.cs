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

    [Header("Debug")]
    [SerializeField] private bool showDebugRays = true;

    // Biến lưu trữ trạng thái va chạm
    private bool _isGrounded;
    private bool _isOnWall;
    private bool _isOnCeiling;
    private bool _isOnRightWall;
    private bool _isOnLeftWall;

    // Mảng để nhận kết quả từ Cast, cấp phát 1 lần để tối ưu
    private readonly RaycastHit2D[] _groundHits = new RaycastHit2D[3];
    private readonly RaycastHit2D[] _wallHits = new RaycastHit2D[3];
    private readonly RaycastHit2D[] _ceilingHits = new RaycastHit2D[3];

    [FormerlySerializedAs("_collider2D")] public CapsuleCollider2D collider2D;

    // Properties để các script khác có thể truy cập (chỉ đọc)
    public bool IsGrounded => _isGrounded;
    public bool IsOnWall => _isOnWall;
    public bool IsOnCeiling => _isOnCeiling;
    public bool IsOnRightWall => _isOnRightWall;
    public bool IsOnLeftWall => _isOnLeftWall;
    public float WallDirection => _isOnLeftWall ? -1 : (_isOnRightWall ? 1 : 0);

    // Thông tin chi tiết về va chạm
    public RaycastHit2D GroundHit { get; private set; }
    public RaycastHit2D WallHit { get; private set; }


    private void Awake()
    {
        if (collider2D == null)
            collider2D = GetComponent<CapsuleCollider2D>();
    }

    private void FixedUpdate()
    {
        // Reset trạng thái trước mỗi lần kiểm tra
        _isGrounded = false;
        _isOnWall = false;
        _isOnRightWall = false;
        _isOnLeftWall = false;
        _isOnCeiling = false;
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
            _isGrounded = true;
            GroundHit = _groundHits[0];
        }
    }

    private void CheckWallCollision()
    {
        // Kiểm tra tường bên phải
        int rightHitCount = collider2D.Cast(Vector2.right, castFilter, _wallHits, wallDistance);
        if (rightHitCount > 0)
        {
            _isOnRightWall = true;
            _isOnWall = true;
            WallHit = _wallHits[0];
        }

        // Kiểm tra tường bên trái
        int leftHitCount = collider2D.Cast(Vector2.left, castFilter, _wallHits, wallDistance);
        if (leftHitCount > 0)
        {
            _isOnLeftWall = true;
            _isOnWall = true; // Ghi đè WallHit nếu tường bên trái gần hơn hoặc cũng được phát hiện
            WallHit = _wallHits[0];
        }
    }

    private void CheckCeilingCollision()
    {
        int hitCount = collider2D.Cast(Vector2.up, castFilter, _ceilingHits, ceilingDistance);
        _isOnCeiling = hitCount > 0;
    }

    private void OnDrawGizmosSelected()
    {
        if (collider2D == null || !showDebugRays) return;

        // Vẽ các đường ray để debug
        Gizmos.color = Color.green;
        Gizmos.DrawLine(collider2D.bounds.center, collider2D.bounds.center + Vector3.down * groundDistance);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(collider2D.bounds.center, collider2D.bounds.center + Vector3.right * wallDistance);
        Gizmos.DrawLine(collider2D.bounds.center, collider2D.bounds.center + Vector3.left * wallDistance);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(collider2D.bounds.center, collider2D.bounds.center + Vector3.up * ceilingDistance);
    }
}