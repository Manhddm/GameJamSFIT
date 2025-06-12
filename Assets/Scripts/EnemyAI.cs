    using System;
    using System.Collections;
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

    private void Awake()
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
        float distanceWaypoint = Vector2.Distance(_playerTarget.position, target.position);
    }
}
