using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private PlayerState _playerState = PlayerState.Idle;
    private Animator _animator;
    private Rigidbody2D _rigidbody;
    [SerializeField] private float _speed = 1000f;
    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
       // _rigidbody = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        PlayerMovement();
        PlayerAction();
        UpdateAnimation();
    }

    private void PlayerAction()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            _playerState = PlayerState.Attacking;
        }
    }

    private void PlayerMovement()
    {
        float actuallySpeed = Input.GetAxis("Horizontal") * _speed * Time.deltaTime; ;
        if (Input.GetKey(KeyCode.A))
        {
            _playerState = PlayerState.Running;
            transform.localScale = new Vector3(-1*Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            _playerState = PlayerState.Running;
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else
        {
            _playerState = PlayerState.Idle;
        }
       // _rigidbody.velocity =  new Vector2(actuallySpeed, _rigidbody.velocity.y);
        transform.position = new Vector3(actuallySpeed + transform.position.x, transform.position.y, transform.position.z);
    }
    
    private void UpdateAnimation()
    {
        _animator.SetInteger("State", (int)_playerState);
    }
}
