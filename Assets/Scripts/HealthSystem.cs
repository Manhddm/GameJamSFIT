// HealthSystem.cs
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Hệ thống quản lý máu và trạng thái bất tử.
/// Có thể sử dụng cho cả Player, Boss và các loại kẻ thù khác.
/// </summary>
public class HealthSystem : MonoBehaviour
{
    [Header("Cài đặt máu")]
    [SerializeField] private float _maxHealth = 100f;
    public float maxHealth => _maxHealth;
    public float currentHealth { get; private set; }

    [Header("Trạng thái bất tử")]
    [Tooltip("Thời gian bất tử (tính bằng giây) sau khi nhận sát thương.")]
    [SerializeField] private float invincibilityDuration = 1f;

    // Biến cờ để kiểm tra trạng thái bất tử
    public bool isInvincible { get; private set; }
    private float _invincibilityTimer;

    // Events để thông báo cho các script khác khi có sự kiện xảy ra
    public UnityEvent OnHurt;
    public UnityEvent OnDie;
    public UnityEvent OnHeal;

    private void Awake()
    {
        currentHealth = _maxHealth;
    }

    private void Update()
    {
        // Đếm ngược thời gian bất tử
        if (isInvincible)
        {
            _invincibilityTimer -= Time.deltaTime;
            if (_invincibilityTimer <= 0)
            {
                isInvincible = false;
            }
        }
    }

    /// <summary>
    /// Hàm nhận sát thương.
    /// </summary>
    /// <param name="damage">Lượng sát thương nhận vào.</param>
    public void TakeDamage(float damage)
    {
        // Nếu đang bất tử hoặc đã chết, không nhận thêm sát thương
        if (isInvincible || currentHealth <= 0)
        {
            return;
        }

        currentHealth -= damage;
        Debug.Log($"{gameObject.name} nhận {damage} sát thương, máu còn lại {currentHealth}");

        if (currentHealth > 0)
        {
            // Kích hoạt trạng thái bất tử và sự kiện "Bị thương"
            StartInvincibility();
            OnHurt?.Invoke();
        }
        else
        {
            currentHealth = 0;
            // Kích hoạt sự kiện "Chết"
            OnDie?.Invoke();
        }
    }
    
    /// <summary>
    /// Hồi một lượng máu.
    /// </summary>
    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, _maxHealth);
        OnHeal?.Invoke();
    }

    /// <summary>
    /// Kích hoạt trạng thái bất tử trong một khoảng thời gian.
    /// </summary>
    private void StartInvincibility()
    {
        isInvincible = true;
        _invincibilityTimer = invincibilityDuration;
    }
}
