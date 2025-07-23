# GitHub Copilot Instructions - Unity 2D Platformer Game

## Thông tin dự án
Đây là dự án game platformer 2D sử dụng Unity. Hãy tuân theo các patterns và conventions Unity khi gợi ý code.

## Unity-Specific Guidelines

### Player Controller
- Sử dụng Rigidbody2D cho physics-based movement
- Implement GroundCheck bằng Physics2D.OverlapCircle hoặc Raycast
- Sử dụng Input System mới (không phải Input.GetKey cũ)
- Áp dụng coyote time và jump buffering cho feel tốt hơn
- Sử dụng FixedUpdate cho physics calculations

### Component Architecture
- Tạo các MonoBehaviour components nhỏ và tái sử dụng được
- Sử dụng [SerializeField] thay vì public fields
- Implement interfaces cho các behaviors (ICollectible, IDamageable)
- Sử dụng UnityEvents cho loose coupling
- Tạo abstract base classes cho Enemy, Collectible

### ScriptableObjects Usage
- Tạo ScriptableObjects cho game data (PlayerStats, LevelData)
- Sử dụng ScriptableObject cho audio management
- Tạo Events system bằng ScriptableObjects
- Store weapon data, enemy stats trong ScriptableObjects

### Physics & Collision
- Sử dụng LayerMask cho collision detection
- Setup Physics2D layers properly (Player, Ground, Enemy, etc.)
- Implement OnTriggerEnter2D cho collectibles
- Sử dụng Composite Collider 2D cho tilemap optimization
- Áp dụng Physics Materials cho bouncy/slippery surfaces

### Animation System - Code-Driven với Animator
- Sử dụng Animator Controller nhưng điều khiển hoàn toàn qua code
- Dùng animator.Play() thay vì parameters/triggers
- Không sử dụng transitions trong Animator Controller
- Tạo các animation states riêng biệt, không kết nối
- Control timing và state switching hoàn toàn trong script
- Sử dụng animator.GetCurrentAnimatorStateInfo() để check trạng thái

### Audio Implementation
- Sử dụng AudioSource components hiệu quả
- Tạo Audio Manager singleton
- Implement object pooling cho sound effects
- Sử dụng Audio Mixers cho volume control

## Code Patterns to Follow

### Player Movement Template
```csharp
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 12f;
    
    [Header("Ground Detection")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayerMask;
    
    private Rigidbody2D rb;
    private Animator animator;
    private bool isGrounded;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }
    
    private void FixedUpdate()
    {
        HandleMovement();
        CheckGrounded();
    }
}
```

### ScriptableObject Data Pattern
```csharp
[CreateAssetMenu(fileName = "PlayerStats", menuName = "Game Data/Player Stats")]
public class PlayerStats : ScriptableObject
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 12f;
    public float coyoteTime = 0.2f;
    
    [Header("Health")]
    public int maxHealth = 3;
    public float invulnerabilityDuration = 1f;
}
```

### Animator.Play() Controller Pattern
```csharp
public class PlayerAnimationController : MonoBehaviour
{
    [Header("Animation State Names")]
    [SerializeField] private string idleState = "Player_Idle";
    [SerializeField] private string walkState = "Player_Walk";
    [SerializeField] private string jumpState = "Player_Jump";
    [SerializeField] private string fallState = "Player_Fall";
    [SerializeField] private string attackState = "Player_Attack";
    
    private Animator animator;
    private string currentState;
    private float stateTimer;
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    
    public void ChangeAnimationState(string newState)
    {
        // Tránh interrupt cùng animation
        if (currentState == newState) return;
        
        // Play animation mới
        animator.Play(newState);
        currentState = newState;
        stateTimer = 0f;
    }
    
    public void PlayAnimation(string stateName, float normalizedTime = 0f)
    {
        animator.Play(stateName, 0, normalizedTime);
        currentState = stateName;
    }
    
    public bool IsAnimationFinished()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.normalizedTime >= 1f && !stateInfo.IsName("loop_animation");
    }
    
    public bool IsCurrentState(string stateName)
    {
        return currentState == stateName;
    }
    
    public float GetCurrentStateTime()
    {
        return animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
    }
    
    private void Update()
    {
        stateTimer += Time.deltaTime;
    }
}
```

### Advanced Animation Manager
```csharp
public class AnimationManager : MonoBehaviour
{
    [System.Serializable]
    public class AnimationData
    {
        public string stateName;
        public float duration;
        public bool canInterrupt = true;
        public int priority = 0;
        public UnityEvent onAnimationStart;
        public UnityEvent onAnimationEnd;
    }
    
    [SerializeField] private AnimationData[] animationDatabase;
    private Animator animator;
    private Dictionary<string, AnimationData> animDict;
    private Coroutine animationWatcher;
    private AnimationData currentAnimation;
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
        
        // Tạo dictionary cho quick lookup
        animDict = new Dictionary<string, AnimationData>();
        foreach (var anim in animationDatabase)
        {
            animDict[anim.stateName] = anim;
        }
    }
    
    public bool PlayAnimation(string stateName, bool forcePlay = false)
    {
        if (!animDict.TryGetValue(stateName, out AnimationData animData))
        {
            Debug.LogWarning($"Animation {stateName} not found!");
            return false;
        }
        
        // Check priority và interrupt
        if (currentAnimation != null && !forcePlay)
        {
            if (!currentAnimation.canInterrupt || 
                animData.priority < currentAnimation.priority)
            {
                return false;
            }
        }
        
        // Stop previous animation watcher
        if (animationWatcher != null)
        {
            StopCoroutine(animationWatcher);
        }
        
        // Play animation
        animator.Play(stateName);
        currentAnimation = animData;
        
        // Trigger start event
        animData.onAnimationStart?.Invoke();
        
        // Start watching animation
        animationWatcher = StartCoroutine(WatchAnimation(animData));
        
        return true;
    }
    
    private IEnumerator WatchAnimation(AnimationData animData)
    {
        // Chờ animation bắt đầu
        yield return null;
        
        while (true)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            
            // Check nếu animation đã finished
            if (stateInfo.normalizedTime >= 1f && 
                !animator.IsInTransition(0))
            {
                animData.onAnimationEnd?.Invoke();
                currentAnimation = null;
                break;
            }
            
            yield return null;
        }
    }
    
    public void StopCurrentAnimation()
    {
        if (animationWatcher != null)
        {
            StopCoroutine(animationWatcher);
            animationWatcher = null;
        }
        currentAnimation = null;
    }
    
    public bool IsPlaying(string stateName)
    {
        return currentAnimation != null && currentAnimation.stateName == stateName;
    }
    
    public float GetAnimationProgress()
    {
        if (currentAnimation == null) return 0f;
        return animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
    }
}
```

### Player Movement với Animation Integration
```csharp
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 12f;
    
    private Rigidbody2D rb;
    private PlayerAnimationController animController;
    private bool isGrounded;
    private float horizontalInput;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animController = GetComponent<PlayerAnimationController>();
    }
    
    private void Update()
    {
        HandleInput();
        HandleAnimations();
    }
    
    private void HandleAnimations()
    {
        // Priority system cho animations
        if (!isGrounded)
        {
            if (rb.velocity.y > 0.1f)
                animController.ChangeAnimationState("Player_Jump");
            else if (rb.velocity.y < -0.1f)
                animController.ChangeAnimationState("Player_Fall");
        }
        else
        {
            if (Mathf.Abs(horizontalInput) > 0.1f)
                animController.ChangeAnimationState("Player_Walk");
            else
                animController.ChangeAnimationState("Player_Idle");
        }
    }
    
    public void PlayAttackAnimation()
    {
        // Force play attack animation với specific timing
        animController.PlayAnimation("Player_Attack");
        
        // Disable movement trong lúc attack
        StartCoroutine(DisableMovementDuringAttack());
    }
    
    private IEnumerator DisableMovementDuringAttack()
    {
        while (!animController.IsAnimationFinished())
        {
            yield return null;
        }
        
        // Re-enable movement khi attack finished
    }
}
```

## Folder Structure
```
Assets/
├── Scripts/
│   ├── Player/
│   ├── Enemies/
│   ├── Collectibles/
│   ├── Managers/
│   ├── UI/
│   └── Utilities/
├── Prefabs/
├── Materials/
├── Sprites/
├── Audio/
├── Scenes/
└── ScriptableObjects/
```

## Performance Best Practices

### Object Pooling
- Pool bullets, particles, enemies
- Sử dụng Queue<GameObject> cho object pool
- Tạo PoolManager singleton
- Pool audio sources cho sound effects

### Optimization Tips
- Sử dụng CompareTag thay vì == "Tag"
- Cache component references trong Awake/Start
- Tránh Find methods trong Update
- Sử dụng coroutines cho delayed actions
- Implement LOD system cho off-screen objects

### Memory Management
- Unsubscribe events trong OnDestroy
- Dispose Handles và NativeArrays
- Tránh boxing trong hot paths
- Sử dụng StringBuilder cho string concatenation

## Common Unity Patterns

### Singleton Pattern
```csharp
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
```

### Event System
```csharp
public static class GameEvents
{
    public static UnityAction<int> OnScoreChanged;
    public static UnityAction<int> OnHealthChanged;
    public static UnityAction OnPlayerDied;
    
    public static void TriggerScoreChanged(int newScore)
    {
        OnScoreChanged?.Invoke(newScore);
    }
}
```

## Unity Features to Utilize

### Input System
- Sử dụng PlayerInput component
- Tạo Input Action Assets
- Implement device switching support
- Handle input buffering properly

### Cinemachine
- Sử dụng Virtual Cameras cho smooth following
- Setup Camera Confiner cho level bounds
- Implement screen shake với Impulse
- Use priority system cho camera transitions

## Animation Best Practices với Animator.Play()

### Animator Controller Setup
- Tạo các animation states riêng biệt, không kết nối transitions
- Mỗi state là một animation clip độc lập
- Không sử dụng parameters hoặc conditions
- Set "Has Exit Time" = false cho tất cả states
- Sử dụng "Any State" chỉ khi cần thiết

### Code-Driven Animation Principles
- Sử dụng animator.Play() để switch states trực tiếp
- Kiểm soát animation logic hoàn toàn trong script
- Implement priority system cho animation conflicts
- Sử dụng GetCurrentAnimatorStateInfo() để track progress
- Cache animation state names thành constants hoặc strings

### Performance Considerations
- Tránh gọi animator.Play() với cùng state liên tục
- Cache Animator component reference trong Awake()
- Sử dụng string hashes thay vì strings khi có thể
- Implement animation culling cho off-screen objects
- Tránh check IsCurrentState() mỗi frame không cần thiết

### Common Animation Patterns
- State machine pattern với explicit state control
- Animation queuing system với priorities
- Interrupt-safe animation switching
- Event-driven animation callbacks
- Normalized time tracking cho precise timing

### Tilemap System
- Sử dụng Tilemap Renderer và Collider
- Create Rule Tiles cho auto-tiling
- Implement animated tiles
- Use Tilemap Extras packages

## Testing & Debugging
- Sử dụng Debug.DrawRay cho visual debugging
- Create custom inspectors cho level design
- Implement cheat codes cho testing
- Use Unity Profiler cho performance analysis
- Add Gizmos cho collision boundaries

## Build & Deployment
- Setup build configurations properly
- Compress textures appropriately
- Use Addressables cho large projects
- Implement proper error handling
- Add loading screens cho scene transitions

## Common Pitfalls to Avoid
- Đừng sử dụng Camera.main trong Update loops
- Tránh Instantiate/Destroy trong gameplay
- Không forget null checks khi destroy objects
- Đừng use static variables cho game state
- Tránh deep hierarchy trong GameObjects