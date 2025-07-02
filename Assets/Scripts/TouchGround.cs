using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class TouchGround : MonoBehaviour
{
    public ContactFilter2D castFilter;
    private RaycastHit2D[] groundHits = new RaycastHit2D[5];
    private RaycastHit2D[] wallHits = new RaycastHit2D[5];
    private RaycastHit2D[] ceilingHits = new RaycastHit2D[5];
    private Animator animator;
    private CapsuleCollider2D touchGround;
    private bool _isGrounded;
    private float groundDistance = 0.1f;
    private float wallDistance = 0.1f;
    private float ceilingDistance = 0.2f;
    private Vector2 wallCheckDirection => gameObject.transform.rotation.y > 0 ? Vector2.right : Vector2.left; 
    public bool IsGrounded
    {
        get => _isGrounded;
        
        set
        {
            _isGrounded = value;
            animator.SetBool((AnimationString.isGrounded), _isGrounded);
        }
    }
    [FormerlySerializedAs("_isOnWall")] [SerializeField]
    private bool isOnWall;
    public bool IsOnWall
    {
        get => isOnWall;
        
        set
        {
            isOnWall = value;
            animator.SetBool((AnimationString.isOnWall), value);
        }
    }

    private bool _isOnCeilling;
    public bool IsOnCeiling
    {
        get => _isOnCeilling;
        
        set
        {
            _isOnCeilling = value;
            animator.SetBool((AnimationString.isOnCeiling), _isOnCeilling);
        }
    }
    
    #region MonoBehaviour

    private void Awake()
    {
        touchGround = GetComponent<CapsuleCollider2D>();
        animator = GetComponent<Animator>();
    }



    void FixedUpdate()
    {
        IsGrounded = touchGround.Cast(Vector2.down, castFilter, groundHits, groundDistance ) > 0;
        IsOnWall = touchGround.Cast(wallCheckDirection, castFilter, wallHits, wallDistance) > 0; 
        IsOnCeiling = touchGround.Cast(Vector2.up, castFilter,ceilingHits, ceilingDistance ) > 0;
  
    }


    #endregion
}
