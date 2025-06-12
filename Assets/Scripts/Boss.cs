using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BossState
{
    IdleFL,
    IdleFR,
    Walk,
    Attack
}
public class Boss : MonoBehaviour
{
    // Start is called before the first frame update
    private Rigidbody2D rb;
    private Animator animator;
    private BossState currentState = BossState.IdleFL;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateState();
        UpdateAnimation();
    }

    private float pos;
    private void UpdateState()
    {
        if (rb.position.x < pos)
        {
            currentState = BossState.IdleFL;
        }
        else if (rb.position.x > pos)
        {
            currentState = BossState.IdleFR;
        }

        pos = rb.position.x;
    }

    private void UpdateAnimation()
    {
        animator.Play(currentState.ToString());
    }
}
