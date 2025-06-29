using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HealthSystem : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float currentHealth;
    [SerializeField] private float maxHealth = 100f;
    
    [Header("Settings")]
    [SerializeField] private bool canTakeDamageWhenDead = false; // Có thể nhận damage khi đã chết không
    [SerializeField] private bool regenerateHealth = false; // Có tự hồi máu không
    [SerializeField] private float regenRate = 1f; // Tốc độ hồi máu per second
    [SerializeField] private float regenDelay = 3f; // Delay sau khi nhận damage mới bắt đầu hồi
    
    [Header("Events")]
    [SerializeField] public UnityEvent onDie;
    [SerializeField] private UnityEvent onTakeDamage;
    [SerializeField] private UnityEvent onHeal;
    [SerializeField] private UnityEvent onHealthChanged; // Event khi health thay đổi (cho UI)
    
    // Tracking states
    private bool _isDead = false;
    private float _lastDamageTime;
    private Coroutine _regenCoroutine;

    #region MonoBehaviour

    void Awake()
    {
        // Đảm bảo currentHealth không vượt quá maxHealth
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        if (currentHealth <= 0)
        {
            currentHealth = maxHealth; // Set full health nếu không được gán
        }
    }

    void Start()
    {
        // Bắt đầu regen nếu được enable
        if (regenerateHealth && !_isDead)
        {
            StartRegeneration();
        }
    }

    #endregion

    #region Properties
    public float CurrentHealth 
    { 
        get => currentHealth; 
        private set 
        {
            float oldHealth = currentHealth;
            currentHealth = Mathf.Clamp(value, 0f, maxHealth);
            
            // Trigger event nếu health thay đổi
            if (!Mathf.Approximately(oldHealth, currentHealth))
            {
                onHealthChanged?.Invoke();
            }
        } 
    }
    
    public float MaxHealth 
    { 
        get => maxHealth; 
        set 
        {
            maxHealth = Mathf.Max(value, 1f); // Đảm bảo maxHealth ít nhất là 1
            // Adjust current health nếu vượt quá maxHealth mới
            if (currentHealth > maxHealth)
            {
                CurrentHealth = maxHealth;
            }
        } 
    }
    
    public bool IsDead => _isDead;
    public float HealthPercentage => maxHealth > 0 ? currentHealth / maxHealth : 0f;
    public bool IsFullHealth => Mathf.Approximately(currentHealth, maxHealth);

    #endregion

    #region Public Methods

    public void TakeDamage(float damage)
    {
        // LOGIC ĐÚNG: Kiểm tra điều kiện trước khi xử lý
        if (_isDead && !canTakeDamageWhenDead)
        {
            return; // Không thể nhận damage khi đã chết
        }
        
        if (damage <= 0)
        {
            return; // Damage phải > 0
        }

        // Trừ máu TRƯỚC khi check điều kiện
        CurrentHealth -= damage;
        _lastDamageTime = Time.time;
        
        // Trigger damage event
        onTakeDamage?.Invoke();
        
        // Restart regen timer
        if (regenerateHealth)
        {
            RestartRegeneration();
        }
        
        // Check death AFTER taking damage
        if (currentHealth <= 0 && !_isDead)
        {
            Die();
        }
    }

    public void Heal(float healAmount)
    {
        if (_isDead || healAmount <= 0)
        {
            return;
        }
        
        float oldHealth = currentHealth;
        CurrentHealth += healAmount;
        
        // Trigger heal event nếu thực sự hồi được máu
        if (currentHealth > oldHealth)
        {
            onHeal?.Invoke();
        }
    }
    
    public void SetHealth(float newHealth)
    {
        float oldHealth = currentHealth;
        CurrentHealth = newHealth;
        
        // Check death nếu health = 0
        if (currentHealth <= 0 && !_isDead)
        {
            Die();
        }
        // Hoặc revive nếu từ chết trở về sống
        else if (currentHealth > 0 && _isDead)
        {
            Revive();
        }
    }
    
    public void RestoreToFull()
    {
        SetHealth(maxHealth);
    }
    
    public void Kill()
    {
        SetHealth(0);
    }

    public void Revive(float reviveHealth = -1f)
    {
        if (!_isDead) return;
        
        _isDead = false;
        
        // Set health to full or specified amount
        if (reviveHealth < 0)
        {
            CurrentHealth = maxHealth;
        }
        else
        {
            CurrentHealth = reviveHealth;
        }
        
        // Restart regen if enabled
        if (regenerateHealth)
        {
            StartRegeneration();
        }
    }

    #endregion

    #region Private Methods

    private void Die()
    {
        if (_isDead) return; // Đã chết rồi thì không die nữa
        
        _isDead = true;
        CurrentHealth = 0; // Đảm bảo health = 0
        
        // Stop regeneration
        if (_regenCoroutine != null)
        {
            StopCoroutine(_regenCoroutine);
            _regenCoroutine = null;
        }
        
        onDie?.Invoke();
    }
    
    private void StartRegeneration()
    {
        if (_regenCoroutine != null)
        {
            StopCoroutine(_regenCoroutine);
        }
        _regenCoroutine = StartCoroutine(RegenerateHealth());
    }
    
    private void RestartRegeneration()
    {
        if (_regenCoroutine != null)
        {
            StopCoroutine(_regenCoroutine);
        }
        _regenCoroutine = StartCoroutine(RegenerateHealthWithDelay());
    }
    
    private IEnumerator RegenerateHealth()
    {
        while (!_isDead && regenerateHealth)
        {
            if (currentHealth < maxHealth)
            {
                float oldHealth = currentHealth;
                CurrentHealth += regenRate * Time.deltaTime;
                
                if (currentHealth > oldHealth)
                {
                    onHeal?.Invoke();
                }
            }
            yield return null;
        }
    }
    
    private IEnumerator RegenerateHealthWithDelay()
    {
        // Chờ delay sau damage
        yield return new WaitForSeconds(regenDelay);
        
        // Bắt đầu regen
        yield return StartCoroutine(RegenerateHealth());
    }

    #endregion
    
    #region Debug
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private void OnValidate()
    {
        // Đảm bảo giá trị hợp lệ trong Editor
        maxHealth = Mathf.Max(maxHealth, 1f);
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        regenRate = Mathf.Max(regenRate, 0f);
        regenDelay = Mathf.Max(regenDelay, 0f);
    }
    #endregion
}