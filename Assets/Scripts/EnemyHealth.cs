using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Cài đặt máu cho Enemy")]
    public float maxHealth;
    public float currentHealth;

    // Thêm tham chiếu đến BossController
    private BossController _bossController;

    void Awake()
    {
        currentHealth = maxHealth;
        // Lấy component BossController trên cùng đối tượng
        _bossController = GetComponent<BossController>();
    }
    
    #region Take Damage

    public void TakeDamage(float damage)
    {
        // Không nhận thêm sát thương nếu đã chết
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        Debug.Log($"{gameObject.name} nhận {damage} sát thương, máu còn lại {currentHealth}");

        // Nếu có BossController, gọi hàm OnTakeDamage() để nó phản ứng
        if (_bossController != null)
        {
            _bossController.OnTakeDamage();
        }
    }

    #endregion
}