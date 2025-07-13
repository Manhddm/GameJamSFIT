using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovementSystem : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 10f;
    [SerializeField] private bool useAcceleration = true;
    
    [Header("Components")]
    [SerializeField] private Rigidbody2D rb;
    
    private Vector2 moveDirection;
    private Vector2 currentVelocity;
    private Vector2 targetVelocity;
    
    public float MoveSpeed 
    { 
        get => moveSpeed; 
        set => moveSpeed = Mathf.Max(0, value); 
    }
    
    public Vector2 CurrentVelocity => currentVelocity;
    public bool IsMoving => moveDirection.magnitude > 0.1f;

    private void Start()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }
        
        moveDirection = Vector2.zero;
        currentVelocity = Vector2.zero;
        targetVelocity = Vector2.zero;
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        // Calculate target velocity
        targetVelocity = moveDirection * moveSpeed;
        
        if (useAcceleration)
        {
            // Smooth acceleration/deceleration
            if (moveDirection.magnitude > 0.1f)
            {
                // Accelerating
                currentVelocity = Vector2.MoveTowards(currentVelocity, targetVelocity, 
                    acceleration * Time.fixedDeltaTime);
            }
            else
            {
                // Decelerating
                currentVelocity = Vector2.MoveTowards(currentVelocity, Vector2.zero, 
                    deceleration * Time.fixedDeltaTime);
            }
        }
        else
        {
            // Instant movement
            currentVelocity = targetVelocity;
        }
        
        // Apply movement
        rb.MovePosition(rb.position + currentVelocity * Time.fixedDeltaTime);
    }

    #region Public Movement Methods
    
    /// <summary>
    /// Set movement direction (normalized vector recommended)
    /// </summary>
    /// <param name="direction">Movement direction</param>
    public void Move(Vector2 direction)
    {
        moveDirection = direction;
        
        // Ensure direction is properly normalized if it has magnitude
        if (moveDirection.magnitude > 1f)
        {
            moveDirection = moveDirection.normalized;
        }
    }
    
    /// <summary>
    /// Move in a specific direction with custom speed
    /// </summary>
    /// <param name="direction">Movement direction</param>
    /// <param name="speed">Custom speed for this movement</param>
    public void Move(Vector2 direction, float speed)
    {
        float originalSpeed = moveSpeed;
        moveSpeed = speed;
        Move(direction);
        moveSpeed = originalSpeed;
    }
    
    /// <summary>
    /// Stop all movement immediately
    /// </summary>
    public void Stop()
    {
        moveDirection = Vector2.zero;
        if (!useAcceleration)
        {
            currentVelocity = Vector2.zero;
        }
    }
    
    /// <summary>
    /// Move towards a specific world position
    /// </summary>
    /// <param name="targetPosition">World position to move towards</param>
    public void MoveTowards(Vector3 targetPosition)
    {
        Vector2 direction = (targetPosition - transform.position).normalized;
        Move(direction);
    }
    
    /// <summary>
    /// Move towards a target transform
    /// </summary>
    /// <param name="target">Target transform to move towards</param>
    public void MoveTowards(Transform target)
    {

        Vector2 direction = (target.transform.position - transform.position).normalized;
        Move(direction);
    }
    
    /// <summary>
    /// Set movement speed temporarily
    /// </summary>
    /// <param name="newSpeed">New movement speed</param>
    public void SetSpeed(float newSpeed)
    {
        moveSpeed = Mathf.Max(0, newSpeed);
    }
    
    /// <summary>
    /// Get distance to a target position
    /// </summary>
    /// <param name="targetPosition">Target position</param>
    /// <returns>Distance to target</returns>
    public float GetDistanceTo(Vector3 targetPosition)
    {
        return Vector2.Distance(transform.position, targetPosition);
    }
    
    /// <summary>
    /// Check if character has reached target position within threshold
    /// </summary>
    /// <param name="targetPosition">Target position</param>
    /// <param name="threshold">Distance threshold</param>
    /// <returns>True if within threshold</returns>
    public bool HasReachedTarget(Vector3 targetPosition, float threshold = 0.1f)
    {
        return GetDistanceTo(targetPosition) <= threshold;
    }
    
    #endregion
    
    #region Utility Methods
    
    /// <summary>
    /// Get the current movement direction
    /// </summary>
    /// <returns>Current movement direction</returns>
    public Vector2 GetMoveDirection()
    {
        return moveDirection;
    }
    
    /// <summary>
    /// Check if the character is facing right based on movement
    /// </summary>
    /// <returns>True if facing right</returns>
    public bool IsFacingRight()
    {
        return currentVelocity.x >0f;
    }
    
    /// <summary>
    /// Check if the character is facing left based on movement
    /// </summary>
    /// <returns>True if facing left</returns>
    public bool IsFacingLeft()
    {
        return currentVelocity.x < -0.1f;
    }
    
    /// <summary>
    /// Apply a force-based movement (useful for knockback, etc.)
    /// </summary>
    /// <param name="force">Force vector to apply</param>
    /// <param name="mode">Force mode</param>
    public void ApplyForce(Vector2 force, ForceMode2D mode = ForceMode2D.Impulse)
    {
        rb.AddForce(force, mode);
    }
    
    /// <summary>
    /// Instantly set the velocity (bypasses normal movement system)
    /// </summary>
    /// <param name="velocity">Velocity to set</param>
    public void SetVelocity(Vector2 velocity)
    {
        rb.velocity = velocity;
        currentVelocity = velocity;
    }
    
    #endregion
    
    #region Debug
    
    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            // Draw movement direction
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, 
                transform.position + (Vector3)moveDirection * 2f);
            
            // Draw current velocity
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, 
                transform.position + (Vector3)currentVelocity);
        }
    }
    
    #endregion
}