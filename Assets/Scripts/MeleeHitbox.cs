// MeleeHitbox.cs
using System.Collections.Generic;
using UnityEngine;

public class MeleeHitbox : MonoBehaviour
{
    // Danh sách để theo dõi các đối tượng đã va chạm trong một lần tấn công
    private List<Collider2D> _hitEnemies = new List<Collider2D>();
    private float _damage;
    private LayerMask _enemyLayers;

    /// <summary>
    /// Bắt đầu một chu kỳ tấn công mới, xóa danh sách đã đánh và thiết lập thông số
    /// </summary>
    public void StartAttack(float damage, LayerMask enemyLayers)
    {
        _hitEnemies.Clear(); // <-- Đây là bước quan trọng nhất
        _damage = damage;
        _enemyLayers = enemyLayers;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Kiểm tra layer của đối tượng va chạm
        if ((_enemyLayers.value & (1 << other.gameObject.layer)) == 0)
        {
            return; // Bỏ qua nếu không phải layer của kẻ thù
        }

        // Kiểm tra xem kẻ thù này đã bị đánh trong lần tấn công này chưa
        if (_hitEnemies.Contains(other))
        {
            return; // Đã đánh rồi, bỏ qua
        }

        // Lấy component máu và gây sát thương
        EnemyHealth enemyHealth = other.GetComponentInParent<EnemyHealth>();
        if (enemyHealth != null)
        {
            Debug.Log($"Hitbox va chạm với: {other.gameObject.name}. Gây {_damage} sát thương.");
            enemyHealth.TakeDamage(_damage);

            // Thêm kẻ thù vào danh sách đã đánh
            _hitEnemies.Add(other);
        }
    }
}