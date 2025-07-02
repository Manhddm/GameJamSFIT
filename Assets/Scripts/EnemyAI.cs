using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum State
{
    Walk,
    Idle,
    Hurt,
    Dead,
    Attack1,
    Attack2,
    WalkAttack
}

[RequireComponent(typeof(MovementSystem))]
public class EnemyAI : MonoBehaviour    
{
    private Transform target;
    private State state;
    private MovementSystem movementSystem;
    private Animator animator;
    private float detectionRange = 10f;
    private float attackRange = 2f;
    private float walkAttackRange = 1f;
    private List<Transform> waypoints;
    private int currentWaypointIndex = 0;
    private float waypointThreshold = 0.1f;
    private float attackCooldown = 0.5f;
    private float attackTimer = 0f;
    private float walkAttackCooldown = 0.5f;
    private float walkAttackTimer = 0f;

    // Gizmos for debugging
    private void OnDrawGizmosSelected()
    {
        // Detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Walk attack range
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, walkAttackRange);
        
        // Waypoints
        Gizmos.color = Color.green;
        foreach (Transform waypoint in waypoints)
        {
            if (waypoint != null)
            {
                Gizmos.DrawWireSphere(waypoint.position, 0.3f);
            }
        }
    }
}