using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TouchGround), typeof(Rigidbody2D), typeof(Damageable))]
public class BossController : MonoBehaviour
{
    public float walkSpeed;
    public float WalkSpeed{
        get
        {
            if (_animator.GetBool(EnumEntity.canMove.ToString())) return walkSpeed;
            return 0;
        }
        set
        {
            
        }
    }
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

    private WalkableDirection WalkDirection
    {
        get { return _walkableDirection; }
        set
        {
            if (_walkableDirection == value) return;
            gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x * -1,
                gameObject.transform.localScale.y, gameObject.transform.localScale.z);
            if (gameObject.transform.localScale.x > 0)
            {
                walkDirectionVector = Vector2.right;
            }
            else
            {
                walkDirectionVector = Vector2.left;
            }
        }
    }

    #region MonoBehaviour

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        touchGround = GetComponent<TouchGround>();
        detectionZone = GetComponentInChildren<DetectionZone>();
        _damageable = GetComponentInChildren<Damageable>();
    }

    private void Update()
    {
        HasTarget = detectionZone.detectedColliders.Count > 0;
        if (AttackCooldown > 0)
            AttackCooldown -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        if (!_damageable.LockVelocity)
            Move();
    }

    #endregion

    #region Movement

    private void Move()
    {
        if (touchGround.IsGrounded && touchGround.IsOnWall)
        {
            FlipDirection();
        }

        rb.velocity = new Vector2(WalkSpeed * walkDirectionVector.x, rb.velocity.y);
    }

    private void FlipDirection()
    {
        if (WalkDirection == WalkableDirection.Right)
        {
            WalkDirection = WalkableDirection.Left;
        }
        else if (WalkDirection == WalkableDirection.Left)
        {
            WalkDirection = WalkableDirection.Right;
        }
        else
        {
            Debug.Log("Error");
        }
    }

    #endregion

    #region Attack
    [SerializeField]
    private bool _hasTarget = false;

    public float AttackCooldown
    {
        get { return _animator.GetFloat(EnemyState.attackCooldown.ToString());}
        set
        {
            _animator.SetFloat(EnemyState.attackCooldown.ToString(), Mathf.Max(0f, value));
        }
    }
    public bool HasTarget
    {
        get{return _hasTarget;}
        set
        {
            _hasTarget = value;
            _animator.SetBool(EnemyState.hasTarget.ToString(),value);
        }
    }

    #endregion
    
    public void OnHit(int damage, Vector2 knockback)
    {
        rb.velocity = new Vector2(knockback.x, knockback.y + rb.velocity.y);
    }
}