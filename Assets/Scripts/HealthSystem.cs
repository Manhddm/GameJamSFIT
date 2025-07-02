using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HealthSystem : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    
    [Header("Events")]
    public UnityEvent onDie;
    public UnityEvent onTakeDamage;
    public UnityEvent onHeal;
    
    private bool isDead = false;
    
    #region Properties
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public float HealthPercentage => maxHealth > 0 ? currentHealth / maxHealth : 0f;
    public bool IsDead => isDead;
    #endregion
    
    void Awake()
    {
        // Khởi tạo máu đầy nếu chưa được set
        if (currentHealth <= 0)
        {
            currentHealth = maxHealth;
        }
    }
    
    public void TakeDamage(float damage)
    {
        // Không nhận damage khi đã chết hoặc damage <= 0
        if (isDead || damage <= 0) return;
        
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        
        // Trigger damage event
        onTakeDamage?.Invoke();
        
        // Check death
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    public void Heal(float healAmount)
    {
        // Không hồi máu khi đã chết hoặc heal <= 0
        if (isDead || healAmount <= 0) return;
        
        float previousHealth = currentHealth;
        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        
        // Chỉ trigger heal event nếu thực sự hồi được máu
        if (currentHealth > previousHealth)
        {
            onHeal?.Invoke();
        }
    }
    
    public void SetHealth(float newHealth)
    {
        currentHealth = Mathf.Clamp(newHealth, 0, maxHealth);
        
        if (currentHealth <= 0 && !isDead)
        {
            Die();
        }
    }
    
    public void Kill()
    {
        SetHealth(0);
    }
    
    private void Die()
    {
        if (isDead) return;
        
        isDead = true;
        currentHealth = 0;
        onDie?.Invoke();
    }
    
    // Optional: Hồi sinh đơn giản
    public void Revive(float reviveHealth = -1)
    {
        if (!isDead) return;
        
        isDead = false;
        currentHealth = reviveHealth > 0 ? reviveHealth : maxHealth;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }
    
    #region Debug
    void OnValidate()
    {
        maxHealth = Mathf.Max(maxHealth, 1f);
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }

    // Vẽ thanh máu debug trong Scene view khi chọn object
#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        // Vị trí vẽ phía trên object
        Vector3 pos = transform.position + Vector3.up * 2f;
        float width = 2f;
        float height = 0.2f;
        float percent = HealthPercentage;

        // Khung nền (màu xám)
        Gizmos.color = Color.gray;
        Gizmos.DrawCube(pos, new Vector3(width, height, 0.01f));

        // Thanh máu (màu xanh lá)
        Gizmos.color = Color.green;
        Gizmos.DrawCube(pos - new Vector3((1 - percent) * width / 2f, 0, 0), new Vector3(width * percent, height, 0.02f));

        // Viền (màu đen)
        Gizmos.color = Color.black;
        Gizmos.DrawWireCube(pos, new Vector3(width, height, 0.03f));
    }
#endif
    #endregion
}