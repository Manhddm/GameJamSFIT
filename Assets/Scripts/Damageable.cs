using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Damageable : MonoBehaviour
{
    [SerializeField]
    private Animator animator;

    public UnityEvent<int, Vector2> damageable;
    [SerializeField] private float maxHealth = 100;

    public float MaxHealth
    {
        get => maxHealth;
        set => maxHealth = value;
    }

    private float health;

    public float Health
    {
        get => health;
        set
        {
            health = value;
            if (health <= 0)
            {
                IsAlive = false;
            }
        }
    }

    private bool _isAlive = true;

    public bool IsAlive
    {
        get { return _isAlive; }
        set
        {
            _isAlive = value;
            animator.SetBool(nameof(EnumEntity.isAlive),value);
        }
    }
    [SerializeField]
    private bool isInvincible = false;

    private float timeSinceHit = 0;
    public float invincibleTimer = 0.25f;

    public bool Hit(int damage, Vector2 knockback)
    {
        if (IsAlive && !isInvincible)
        {
            Health -= damage;
            isInvincible = true;
            LockVelocity = true;
            animator.SetTrigger(EnumEntity.hit.ToString());
            damageable?.Invoke(damage,knockback);
            CharacterEvents.characterDamaged.Invoke(gameObject,damage);
            return true;
        }

        return false;
    }

    public bool Heal(int heal)
    {
        if (IsAlive)
        {
            float maxHeal = Mathf.Max(MaxHealth - Health, 0);
            float actualHeal = Mathf.Min(maxHeal, health);
            Health += actualHeal;
            if (actualHeal > 0) CharacterEvents.characterHealed(gameObject,(int)actualHeal);
            return actualHeal > 0;
        }
        return false;
    }

    public bool LockVelocity
    {
        get
        {
            return animator.GetBool(nameof(EnumEntity.lockVelocity));
        }
        set
        {
            animator.SetBool(nameof(EnumEntity.lockVelocity), value);
        }
    }


    #region MonoBehaviour

    void Awake()
    {
        health = maxHealth;
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (isInvincible)
        {
        
    
            if (timeSinceHit > invincibleTimer)
            {

                isInvincible = false;
                timeSinceHit = 0;
            }
            timeSinceHit += Time.deltaTime;
        }
        
    }

    #endregion

    #region DeBug

// Khai báo style bên ngoài để tối ưu hiệu năng
    private GUIStyle debugBoxStyle;

    private void OnGUI()
    {
        if (gameObject.name != "Player") return;
        // Chỉ khởi tạo style một lần
        if (debugBoxStyle == null)
        {
            debugBoxStyle = new GUIStyle(GUI.skin.box);
            debugBoxStyle.alignment = TextAnchor.UpperLeft; // Căn lề trên-trái
            debugBoxStyle.padding = new RectOffset(5, 5, 5, 5); // Thêm lề để chữ không dính vào cạnh
            debugBoxStyle.fontSize = 14; // Tăng kích thước font một chút
        }

        // 1. Xác định vị trí và kích thước của hộp debug
        Rect debugRect = new Rect(10, 10, 250, 120);

        // 2. Tạo nội dung chữ sẽ hiển thị
        // Dùng $ để dễ dàng chèn biến vào chuỗi
        // Dùng \n để xuống dòng
        string debugText = $"--- {gameObject.name} DEBUG ---\n";
        debugText += $"Health: {Health:F0} / {MaxHealth:F0}\n";
        debugText += $"Is Alive: {IsAlive}\n";
        debugText += $"Is Invincible: {isInvincible}\n";
    
        // Chỉ hiển thị timer khi đang trong trạng thái bất tử
        if (isInvincible)
        {
            debugText += $"Invincible Timer: {timeSinceHit:F2}s";
        }

        // 3. Vẽ hộp debug lên màn hình
        GUI.Box(debugRect, debugText, debugBoxStyle);
    }

    #endregion  
    
}