using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HealthSystem : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float currentHealth;
    [SerializeField] private float maxHealth;
    [Header("Events")]
    [SerializeField] private UnityEvent onDie;
    [SerializeField] private UnityEvent onTakeDamage;


    #region MonoBehaviour

    void Awake()
    {
        currentHealth = maxHealth;
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    #endregion

    #region Getter_Setter
    public float CurrentHealth { get => currentHealth; set => currentHealth = value; }
    public float MaxHealth { get => maxHealth; set => maxHealth = value; }
    

    #endregion
   
    

    

    #region Public Methods

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0)
        {
            return;
        }

        if (currentHealth < 0)
        {
            currentHealth = 0;
        }
        onTakeDamage?.Invoke();
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    #endregion

    #region Events  

    private void Die()
    {
        onDie?.Invoke();
    }

    #endregion
    
}
