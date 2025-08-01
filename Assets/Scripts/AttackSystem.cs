using System;
using UnityEngine;

/// <summary>
/// Quản lý các thông số và tài nguyên cho hệ thống tấn công.
/// Script này đóng vai trò là nơi chứa dữ liệu tấn công cho nhân vật.
/// </summary>
public class AttackSystem : MonoBehaviour
{
    [SerializeField] private int attackDamage =  10;
    public Vector2 knockback = Vector2.zero;


    private void OnTriggerEnter2D(Collider2D other)
    {
        Damageable damageable = other.GetComponent<Damageable>();

        if (damageable != null)
        {
            float direction = Mathf.Sign(transform.parent.localScale.x);
            direction = direction / (Mathf.Abs(direction));
            Vector2 trueKnockback = new Vector2(Mathf.Abs(knockback.x)*direction, knockback.y);
            bool gotHit = damageable.Hit(attackDamage, trueKnockback);
            if (gotHit)
            {
                if (gameObject.name == "Player")
                {
                    Debug.Log(gameObject.name + " is attacking" + other.name);
                }
            }
        }
    }
}