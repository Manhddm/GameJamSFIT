using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damageable : MonoBehaviour
{
    private HealthSystem healthSystem;
    [SerializeField]
    private bool isInvincible = false;
    private float invincibleTime = 0.5f;
    private float timeSinceHit = 0f;
    // Start is called before the first frame update
    void Start()
    {
        healthSystem = GetComponent<HealthSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isInvincible)
        {
            timeSinceHit += Time.deltaTime;
            if (timeSinceHit >= invincibleTime) {
                isInvincible = false;
                timeSinceHit = 0f;
            }
        }
    }
    #region Methods
    public void Hit(float damage)
    {
        if (healthSystem.IsDead || isInvincible) return;
        healthSystem.TakeDamage(damage);
        isInvincible = true;
        
    }

    #endregion
}
