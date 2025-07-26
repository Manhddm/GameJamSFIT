using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Cài đặt máu cho Enemy")]
    public float maxHealth;
    public float currentHealth;

    void Awake()
    {
        currentHealth = maxHealth;
    }
    
    #region Take Damage

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
    }

    #endregion
}
