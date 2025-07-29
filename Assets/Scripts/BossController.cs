using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TouchGround), typeof(Rigidbody2D), typeof(HealthSystem))]
public class BossController : MonoBehaviour
{
    public float walkSpeed;
    [SerializeField]
    private Rigidbody2D rb;
    [SerializeField]
    private TouchGround touchGround;
    private Animator _animator;

    enum WalkableDirection
    {
        Right,
        Left,
    }

    private WalkableDirection _walkableDirection = WalkableDirection.Right;
    public Vector2 walkDirectionVector = Vector2.right;
    private WalkableDirection WalkDirection
    {
        get{ return _walkableDirection; }
        set
        {
            if (_walkableDirection == value) return;
            gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x *-1, gameObject.transform.localScale.y, gameObject.transform.localScale.z);
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
    }

    private void FixedUpdate()
    {
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
        rb.velocity = new Vector2(walkSpeed * walkDirectionVector.x, rb.velocity.y);
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
}
