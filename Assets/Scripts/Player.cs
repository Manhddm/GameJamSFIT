using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private PlayerState _playerState = PlayerState.Idle;
    private Animator _animator;
    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        PlayerMovement();
        UpdateAnimation();
    }

    private void PlayerMovement()
    {
        if (Input.GetKey(KeyCode.A))
        {
            _playerState = PlayerState.Running;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            _playerState = PlayerState.Running;
        }
        else
        {
            _playerState = PlayerState.Idle;
        }
        
    }
    
    private void UpdateAnimation()
    {
        _animator.SetInteger("State", (int)_playerState);
    }
}
