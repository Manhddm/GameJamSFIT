using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HealthSystem : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float currentHealth;
    [SerializeField] private float maxHealth;
    
    public float CurrentHealth { get => currentHealth; set => currentHealth = value; }
    public float MaxHealth { get => maxHealth; set => maxHealth = value; }
    
    [Header("Events")]
    [SerializeField] private UnityEvent onDie;
    [SerializeField] private UnityEvent onTakeDamage;
    

    #region Public Methods

    public void TakeDamage(int damage)
    {
        onTakeDamage?.Invoke();
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    #endregion

    #region Private Methods

    private void Die()
    {
        onDie?.Invoke();
    }

    #endregion
    
}
