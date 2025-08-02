using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TouchGround), typeof(Rigidbody2D), typeof(Damageable))]
public class BossController : MonoBehaviour
{
    // Giữ lại biến tốc độ gốc, đơn giản và rõ ràng
    public float walkSpeed;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private TouchGround touchGround;
    private Animator _animator;
    [SerializeField]
    public DetectionZone detectionZone;
    private Damageable _damageable;

    enum WalkableDirection
    {
        Right,
        Left,
    }

    private WalkableDirection _walkableDirection = WalkableDirection.Right;
    public Vector2 walkDirectionVector = Vector2.right;

    // Thuộc tính WalkDirection giờ chỉ thay đổi trạng thái và gọi hàm Flip
    private WalkableDirection WalkDirection
    {
        get { return _walkableDirection; }
        set
        {
            if (_walkableDirection == value) return;
            _walkableDirection = value;
            Flip();
        }
    }

    #region MonoBehaviour

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        touchGround = GetComponent<TouchGround>();
        detectionZone = GetComponentInChildren<DetectionZone>();
        _damageable = GetComponent<Damageable>(); // Giả sử Damageable cùng cấp
    }

    private void Update()
    {
        HasTarget = detectionZone.detectedColliders.Count > 0;
        if (AttackCooldown > 0)
            AttackCooldown -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        // Kiểm tra LockVelocity trước khi di chuyển
        if (!_damageable.LockVelocity)
            Move();
    }

    #endregion

    #region Movement

    private void Move()
    {
        // Kiểm tra va chạm tường để đổi hướng
        if (touchGround.IsGrounded && touchGround.IsOnWall)
        {
            FlipDirection();
        }

        // Tính toán tốc độ di chuyển thực tế ngay tại đây
        float currentMoveSpeed = 0f;
        if (_animator.GetBool(EnumEntity.canMove.ToString()))
        {
            currentMoveSpeed = walkSpeed;
        }

        // Áp dụng vận tốc
        rb.velocity = new Vector2(currentMoveSpeed * walkDirectionVector.x, rb.velocity.y);
    }

    // Hàm lật người, chỉ chịu trách nhiệm về hình ảnh và vector
    private void Flip()
    {
        // Lật hình ảnh
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);

        // Cập nhật vector hướng đi
        if (WalkDirection == WalkableDirection.Right)
        {
            walkDirectionVector = Vector2.right;
        }
        else
        {
            walkDirectionVector = Vector2.left;
        }
    }

    // Hàm đổi hướng logic, đã được đơn giản hóa
    private void FlipDirection()
    {
        if (WalkDirection == WalkableDirection.Right)
        {
            WalkDirection = WalkableDirection.Left;
        }
        else
        {
            WalkDirection = WalkableDirection.Right;
        }
    }

    #endregion

    #region Attack
    [SerializeField]
    private bool _hasTarget = false;

    public float AttackCooldown
    {
        get { return _animator.GetFloat(EnemyState.attackCooldown.ToString()); }
        set
        {
            _animator.SetFloat(EnemyState.attackCooldown.ToString(), Mathf.Max(0f, value));
        }
    }
    public bool HasTarget
    {
        get { return _hasTarget; }
        set
        {
            _hasTarget = value;
            _animator.SetBool(EnemyState.hasTarget.ToString(), value);
        }
    }

    #endregion
    
    // Hàm xử lý khi bị tấn công
    public void OnHit(int damage, Vector2 knockback)
    {
        rb.velocity = new Vector2(knockback.x, rb.velocity.y + knockback.y);
    }
}
