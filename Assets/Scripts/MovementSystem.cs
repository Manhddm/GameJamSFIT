using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Rigidbody2D))]
public class MovementSystem : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    private Rigidbody2D _rb;
    private Vector2 _offset;
    public float MoveSpeed{get=>moveSpeed;set=>moveSpeed=value;}

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _offset = Vector2.zero;
    }
    private void FixedUpdate()
    {
        _rb.MovePosition(_rb.position + _offset * moveSpeed * Time.fixedDeltaTime);
    }

    #region Movement
    public void Move(Vector2 move)
    {
        _offset = move;
    }
    

    #endregion
}
