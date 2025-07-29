// MeleeHitbox.cs
using System.Collections.Generic;
using UnityEngine;

public class MeleeHitbox : MonoBehaviour
{
    private List<Collider2D> _hitTargets = new List<Collider2D>();
    private float _damage;
    private LayerMask _targetLayers;

    public void StartAttack(float damage, LayerMask targetLayers)
    {
        _hitTargets.Clear();
        _damage = damage;
        _targetLayers = targetLayers;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Kiểm tra layer của đối tượng va chạm
        if ((_targetLayers.value & (1 << other.gameObject.layer)) == 0)
        {
            return; // Bỏ qua nếu không phải layer mục tiêu
        }

        // Kiểm tra xem mục tiêu này đã bị đánh trong lần tấn công này chưa
        if (_hitTargets.Contains(other))
        {
            return; // Đã đánh rồi, bỏ qua
        }

        // **THAY ĐỔI**: Lấy component HealthSystem và gây sát thương
        HealthSystem healthSystem = other.GetComponentInParent<HealthSystem>();
        if (healthSystem != null)
        {
            Debug.Log($"Hitbox va chạm với: {other.gameObject.name}. Gây {_damage} sát thương.");
            healthSystem.TakeDamage(_damage);

            // Thêm mục tiêu vào danh sách đã đánh
            _hitTargets.Add(other);
        }
    }
}