using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public enum BossState
{
    Idle,
    Phase1,
    Phase2,
    Phase3,
    Walk,
    Attack1,
    Attack2,
    WalkAttack,
    Hurt,
    Dead
}
[RequireComponent(typeof(MovementSystem))]
[RequireComponent(typeof(TouchGround))]
public class BossController : MonoBehaviour
{
    
    private BossState state;
    public DetectionZone attackZone;
    private MovementSystem movementSystem;
    private Animator animator;
    private Rigidbody2D rb;
    private List<Transform> waypoints;
    private float waypointDistance = 0.1f;
    private int currentWaypointIndex = 0;
    public float waitTimeAtWaypoint = 1f;
    private bool isFacingRight = true;
    private TouchGround touchGround;
    [SerializeField] private bool isWaitingAtWaypoint = false;
    [SerializeField] private bool invincible = false;
    [SerializeField] private bool findedPlayer = false;
    private bool canMove = true;
    private bool isMoving = false;
    private bool _hasPlayer = false;

    #region Propertie

    public bool HasPlayer
    {
        get => _hasPlayer;
        set
        {
            _hasPlayer = value;
            animator.SetBool(AnimationString.hasPlayer, value);
        }
    }
    public bool CanMove
    {
        get
        {
            return true;//animator.GetBool(AnimationString.canMove);
        }
    }

    public bool IsMoving
    {
        get
        {
            return isMoving;
        }
        set
        {
            isMoving = value;
            animator.SetBool(AnimationString.isMoving, value);
        }
    }
    #endregion
    
    #region Movement
    private void Move()
    {
        if (!touchGround.IsGrounded)
        {
            movementSystem.Stop();
            return;
        }
        if (!findedPlayer && CanMove && !isWaitingAtWaypoint)
        {
            state = BossState.Walk;
            
            IsMoving = true;
            if (!GameObject.Find($"{gameObject.name}_Waypoints"))
            {
                GameObject OJ_waypoints = new GameObject($"{gameObject.name}_Waypoints");
                GameObject waypoint1 = new GameObject($"{gameObject.name}_Waypoint1");
                waypoint1.transform.SetParent(OJ_waypoints.transform);
                waypoint1.transform.position = new Vector3(transform.position.x + 5, transform.position.y, 0);
                GameObject waypoint2 = new GameObject($"{gameObject.name}_Waypoint2");
                waypoint2.transform.SetParent(OJ_waypoints.transform);
                waypoint2.transform.position = new Vector3(transform.position.x -5, transform.position.y, 0);
                GameObject waypoint3 = new GameObject($"{gameObject.name}_Waypoint3");
                waypoint3.transform.SetParent(OJ_waypoints.transform);
                waypoint3.transform.position = new Vector3(transform.position.x, transform.position.y, 0);
                waypoints = new List<Transform> { waypoint1.transform, waypoint2.transform, waypoint3.transform };
            }
            else{
                Transform targetWaypoint = waypoints[currentWaypointIndex];
                float distance = Vector2.Distance(transform.position, targetWaypoint.position);
                if (distance > waypointDistance)
                {
                    movementSystem.MoveTowards(targetWaypoint.position);
                }
                else
                {
                    StartCoroutine(WaitAtWaypoint(waitTimeAtWaypoint));
                    isWaitingAtWaypoint = true;
                    //currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
                }
            }
        }
    }
    private IEnumerator WaitAtWaypoint(float waitTime)
    {
        isWaitingAtWaypoint = true;
        state = BossState.Idle;
        IsMoving = false;
        movementSystem.Stop();
        yield return new WaitForSeconds(waitTime);
        isWaitingAtWaypoint = false;
        int newWaypointIndex = Random.Range(0,waypoints.Count);
        while (newWaypointIndex == currentWaypointIndex)
        {
            newWaypointIndex = Random.Range(0,waypoints.Count);
        }
        currentWaypointIndex = newWaypointIndex;
    }
    #endregion

    #region Attack

    private void Attack1()
    {
        if (HasPlayer)
        {
            movementSystem.Stop();
        }
    }
    #endregion

    #region Hurt
    #endregion

    #region Dead
    #endregion

    #region Animation

    private void UpdateAnimation()
    {
        
        if (isFacingRight && movementSystem.IsFacingLeft())
        {
            Flip();
        }
        else if (!isFacingRight && movementSystem.IsFacingRight())
        {
            Flip();
        }
    }
    private void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.Rotate(0f, 180f, 0f);
        
    }
    #endregion

    #region MonoBehaviour Methods
    private void Awake()
    {
        movementSystem = GetComponent<MovementSystem>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        touchGround = GetComponent<TouchGround>();
        
    }
    private void Start()
    {
        state = BossState.Idle;
    }
    private void Update()
    {
        UpdateAnimation();
        HasPlayer = attackZone.detectedColliders.Count > 0;
    }

    private void FixedUpdate()
    {
        Move();
        Attack1();
    }

    #endregion

    #region Debug
    #endregion
}