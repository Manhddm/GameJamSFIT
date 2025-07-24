using UnityEngine;

/// <summary>
/// Quản lý các thông số và tài nguyên cho hệ thống tấn công.
/// Script này đóng vai trò là nơi chứa dữ liệu tấn công cho nhân vật.
/// </summary>
public class AttackSystem : MonoBehaviour
{
    [Header("Melee Attack Settings")]
    [Tooltip("Sát thương của đòn tấn công cận chiến.")]
    public float meleeDamage = 20f;

    [Header("Common Settings")]
    [Tooltip("Layer của các đối tượng được coi là kẻ thù.")]
    public LayerMask enemyLayers;

    // Logic tấn công tầm xa đã được tạm thời loại bỏ để tập trung vào tấn công cận chiến.
}