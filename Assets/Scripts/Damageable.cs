using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Damageable : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    
    [Header("Invincibility Settings")]
    [SerializeField] private bool isInvincible = false;
    [SerializeField] private float invincibleTime = 0.5f;
    private float timeSinceHit = 0f;
    
    [Header("Events")]
    public UnityEvent<float, Vector2> OnHit;
    public UnityEvent OnDeath;
    
    private Rigidbody2D rb;
    private Animator animator;
    
    #region Properties
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsAlive => currentHealth > 0;
    public bool IsInvincible => isInvincible;
    #endregion
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        
        if (currentHealth <= 0)
            currentHealth = maxHealth;
    }
    
    private void Update()
    {
        if (isInvincible)
        {
            timeSinceHit += Time.deltaTime;
            if (timeSinceHit >= invincibleTime)
            {
                isInvincible = false;
                timeSinceHit = 0f;
            }
        }
    }
    
    public void Hit(float damage, Vector2 knockback = default)
    {
        if (!IsAlive || isInvincible) return;
        
        // Giảm máu
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        
        // Set invincible
        isInvincible = true;
        timeSinceHit = 0f;
        
        // Apply knockback
        if (rb != null && knockback != Vector2.zero)
        {
            rb.AddForce(knockback, ForceMode2D.Impulse);
        }
        
        // Trigger hit animation
        if (animator != null)
        {
            animator.SetTrigger("Hit");
        }
        
        // Invoke events
        OnHit?.Invoke(damage, knockback);
        
        // Check death
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    private void Die()
    {
        if (animator != null)
        {
            animator.SetBool("IsAlive", false);
        }
        
        OnDeath?.Invoke();
    }
    
    public void Heal(float amount)
    {
        if (!IsAlive) return;
        
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
    }
}
