using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Damageable : MonoBehaviour
{
    [SerializeField]
    private Animator animator;

    public UnityEvent<int, Vector2> damageable;
    [SerializeField] private float maxHealth = 100f;

    public float MaxHealth
    {
        get => maxHealth;
        set => maxHealth = value;
    }

    private float health;

    public float Health
    {
        get => health;
        set
        {
            health = value;
            if (health <= 0)
            {
                IsAlive = false;
            }
        }
    }

    private bool _isAlive = true;

    public bool IsAlive
    {
        get { return _isAlive; }
        set
        {
            _isAlive = value;
            animator.SetBool(nameof(EnumEntity.isAlive),value);
        }
    }
    [SerializeField]
    private bool isInvincible = false;

    private float timeSinceHit = 0;
    public float invincibleTimer = 0.25f;

    public bool Hit(int damage, Vector2 knockback)
    {
        if (IsAlive && !isInvincible)
        {
            Health -= damage;
            isInvincible = true;
            LockVelocity = true;
            animator.SetTrigger(EnumEntity.hit.ToString());
            damageable?.Invoke(damage,knockback);
            return true;
        }

        return false;
    }

    public bool LockVelocity
    {
        get
        {
            return animator.GetBool(nameof(EnumEntity.lockVelocity));
        }
        set
        {
            animator.SetBool(nameof(EnumEntity.lockVelocity), value);
        }
    }


    #region MonoBehaviour

    void Awake()
    {
        health = maxHealth;
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (isInvincible)
        {
        
    
            if (timeSinceHit > invincibleTimer)
            {

                isInvincible = false;
                timeSinceHit = 0;
            }
            timeSinceHit += Time.deltaTime;
        }
        
    }

    #endregion
}