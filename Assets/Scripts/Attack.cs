using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private Vector2 knockback = new Vector2(5f, 2f);
    [SerializeField] private LayerMask targetLayers;
    private Collider2D attackCollider;
    private bool isAttacking = false;
    
    private void Awake()
    {
        attackCollider = GetComponent<Collider2D>();
        attackCollider.enabled = false; // Tắt collider mặc định
    }
    
    // Được gọi từ Animation Event
    public void EnableAttack()
    {
        if (!isAttacking)
        {
            attackCollider.enabled = true;
            isAttacking = true;
        }
    }
    
    // Được gọi từ Animation Event
    public void DisableAttack()
    {
        attackCollider.enabled = false;
        isAttacking = false;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Kiểm tra layer
        if (((1 << other.gameObject.layer) & targetLayers) == 0) return;
        
        // Kiểm tra component Damageable
        Damageable damageable = other.GetComponent<Damageable>();
        if (damageable != null)
        {
            // Tính toán hướng knockback
            Vector2 direction = (other.transform.position - transform.position).normalized;
            Vector2 finalKnockback = new Vector2(
                knockback.x * (transform.parent.localScale.x > 0 ? 1 : -1),
                knockback.y
            );
            
            // Gây damage
            damageable.Hit(attackDamage, finalKnockback);
        }
    }
}
