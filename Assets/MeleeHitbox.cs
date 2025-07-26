using UnityEngine;

/// <summary>
/// Script này được gắn vào GameObject chứa collider của vùng tấn công (hitbox).
/// Nó chịu trách nhiệm phát hiện va chạm và gây sát thương.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class MeleeHitbox : MonoBehaviour
{
    public AttackSystem _attackSystem;

    private void Awake()
    {
        // Tìm AttackSystem ở object cha (Player)
        if(_attackSystem == null)
        _attackSystem = GetComponentInParent<AttackSystem>();
        if (_attackSystem == null)
        {
            Debug.LogError("Không tìm thấy AttackSystem ở object cha!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Kiểm tra xem có va chạm với layer của kẻ thù không
        if ((_attackSystem.enemyLayers.value & (1 << other.gameObject.layer)) > 0)
        {
            Debug.Log($"Vùng chém đã va chạm với: {other.transform.parent.name} + {other.name}");
            
            // Lấy component máu của kẻ thù và gây sát thương
            other.GetComponent<EnemyHealth>()?.TakeDamage(_attackSystem.meleeDamage);
        }
    }
}