using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class PLayerControllerVl : MonoBehaviour
{
   public float walkSpeed = 10f;
   public float runSpeed = 20f;


   private Vector2 _moveInput;
   private bool _isMoving = false;
   private bool _isRunning = false;
   private bool _isFacingRight = true;
   private bool _isJumping = false;
   public float highJump = 100f;
   private TouchGround _touchGround;
   #region Events

   public bool IsMoving
   {
      get => _isMoving;
      private set
      {
         _isMoving = value;
         _animator.SetBool(AnimationString.isMoving, _isMoving);
      }
   }

   public bool IsRunning
   {
      get => _isRunning;
      set
      {
         _isRunning = value;
         _animator.SetBool((AnimationString.isRunning), _isRunning);
         
      }
   }
   
   #endregion
  

   private Rigidbody2D _rb;
   private Animator _animator;
   
   #region MonoBehavior

   private void Awake()
   {
      _rb = GetComponent<Rigidbody2D>();
      _animator = GetComponent<Animator>();
      _touchGround = GetComponent<TouchGround>();
      IsRunning = false;
   }

   void Start()
   {
        
   }
    
   void Update()
   {
     
   }

   void FixedUpdate()
   {
      _rb.velocity = new Vector2(_moveInput.x*CurrentSpeed*Time.fixedDeltaTime, _rb.velocity.y ) ;
      _animator.SetFloat(AnimationString.yVelocity, _rb.velocity.y);
   }
   #endregion

   #region Player Input

   public void OnMove(InputAction.CallbackContext context)
   {
      _moveInput = context.ReadValue<Vector2>();
      IsMoving = _moveInput != Vector2.zero;
      SetFacing(_moveInput);
   }

   private void SetFacing(Vector2 moveInput)
   {
      if ((moveInput.x > 0 && !_isFacingRight) || (moveInput.x < 0 && _isFacingRight))
      {
         _isFacingRight = !_isFacingRight;
         transform.Rotate(0f, 180f, 0f);
      }
   }

   public void OnRun(InputAction.CallbackContext context)
   {
     
      if (context.started)
      {
         IsRunning = !IsRunning;
         CurrentSpeed = IsRunning ? runSpeed : walkSpeed;
      }
   }

   public void OnJump(InputAction.CallbackContext context)
   {
      if (!context.started || !_touchGround.IsGrounded) return;
      _animator.SetTrigger(AnimationString.isJumping);
      _rb.velocity = new Vector2(_rb.velocity.x, highJump);
   }
   #endregion

   #region UpdateLogic

   private float CurrentSpeed
   {
      get
      {
         if (IsMoving && !_touchGround.IsOnWall)
         {
            if (IsRunning)
            {
               return runSpeed;
            }

            return walkSpeed;
         }
         return 0f;
      }
      set{}
   }
   

   #endregion
}
