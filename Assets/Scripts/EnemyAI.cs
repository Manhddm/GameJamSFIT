using System.Collections.Generic;
using UnityEngine;
public enum State
{
    Idle,
    Walk,
    Run
}


[RequireComponent(typeof(MovementSystem))]
public class EnemyAI : MonoBehaviour    
{
    private Transform _playerTarget;
    private MovementSystem _movementSystem;
    [SerializeField] private Transform waypointContainer;
    private List<Transform> waypoints = new List<Transform>();
    private int currentWaypoint = 0;
    private Rigidbody2D _rb;
    private State state = State.Idle;
    private Animator _animator;
    private bool _facingLeft = true;

    private void Start()
    {
        _movementSystem = GetComponent<MovementSystem>();
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        InitializedWaypoints();
    }

    void InitializedWaypoints()
    {
        if (waypointContainer == null)
        {
            return;
        }

        int cnt = 1;
        foreach (Transform child in waypointContainer)
        {
            if (cnt == 1 )child.transform.position = new Vector3(transform.position.x + 4, transform.position.y, transform.position.z);
            if (cnt == 2) child.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
            if (cnt == 3) child.transform.position = new Vector3(transform.position.x - 4, transform.position.y, transform.position.z);
            waypoints.Add(child);
            cnt++;
        }
    }
    private void Update()
    {
        if (_playerTarget == null)
        {
            AutoMovement();
        }
        UpdateState();
       // FlipCharacter();
        UpdateAnimation();
    }

    void AutoMovement()
    {
        Transform target = waypoints[currentWaypoint];
        float distanceToWaypoint = Vector2.Distance(transform.position, target.position);
        float waypointReachedThreshold = 0.1f;
        if (distanceToWaypoint <= waypointReachedThreshold)
        {
           // Delay();
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

    
    private void UpdateState()
    {

    }

    private void FlipCharacter()
    {
        if (_rb.position.x > 0)
        {
            transform.transform.Rotate(0f, 180f, 0f);
        }
    }

    private void UpdateAnimation()
    {
        _animator.Play(state.ToString());
    }

    //private IEnumerator<> Delay()
    //{
    //    yield return new WaitForSeconds(2f);
  //  }
}
