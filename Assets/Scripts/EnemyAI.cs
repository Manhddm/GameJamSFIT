using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(MovementSystem))]
public class EnemyAI : MonoBehaviour    
{
    private Transform _playerTarget;
    private MovementSystem _movementSystem;
    [SerializeField] private Transform waypointContainer;
    private List<Transform> waypoints = new List<Transform>();
    private int currentWaypoint = 0;

    private void Start()
    {
        _movementSystem = GetComponent<MovementSystem>();
        InitializedWaypoints();
    }

    void InitializedWaypoints()
    {
        if (waypointContainer == null)
        {
            return;
        }

        foreach (Transform child in waypointContainer)
        {
            waypoints.Add(child);
        }
    }
    private void Update()
    {
        if (_playerTarget == null)
        {
            AutoMovement();
        }
    }

    void AutoMovement()
    {
        Transform target = waypoints[currentWaypoint];
        float distanceToWaypoint = Vector2.Distance(transform.position, target.position);
        float waypointReachedThreshold = 0.1f;
        if (distanceToWaypoint <= waypointReachedThreshold)
        {
            // Nếu đã đến, chuyển sang waypoint tiếp theo
            currentWaypoint++;

            // 4. Nếu đã đi hết vòng, quay lại điểm đầu tiên
            if (currentWaypoint >= waypoints.Count)
            {
                currentWaypoint = 0;
            }
            
            // Cập nhật lại mục tiêu sau khi đã đổi index
            target = waypoints[currentWaypoint];
        }

        // 5. Tính toán hướng và ra lệnh di chuyển
        Vector2 direction = (target.position - transform.position).normalized;
        _movementSystem.Move(direction);
    }
}
