using UnityEngine;

public class Parallax : MonoBehaviour
{
    [Header("Parallax Settings")]
    public Camera cam;
    public Transform followTarget;
    
    [Header("Parallax Factor")]
    [Range(-1f, 1f)]
    public float parallaxEffectMultiplier = 0.5f; // Manual control cho parallax effect
    
    private Vector2 _startPos;
    private Vector2 _startCamPos;
    
    // Tính toán movement của camera từ lúc bắt đầu
    private Vector2 CamMoveSinceStart => (Vector2)cam.transform.position - _startCamPos;
    
    // Z distance để tính parallax factor tự động (nếu không dùng manual)
    private float ZDistanceFromTarget => transform.position.z - followTarget.position.z;
    private float ClippingPlane => (cam.transform.position.z + (ZDistanceFromTarget > 0 ? cam.farClipPlane : cam.nearClipPlane));
    private float AutoParallaxFactor => Mathf.Abs(ZDistanceFromTarget) / ClippingPlane;
    
    private float _startingZ;
    
    [Header("Parallax Mode")]
    public bool useManualParallaxFactor = true; // Chọn dùng manual hay auto
    
    void Start()
    {
        // Lưu vị trí ban đầu của object và camera
        _startPos = transform.position;
        _startCamPos = cam.transform.position;
        _startingZ = transform.position.z;
    }

    void Update()
    {
        // Chọn parallax factor
        float parallaxFactor = useManualParallaxFactor ? parallaxEffectMultiplier : AutoParallaxFactor;
        
        // LOGIC ĐÚNG: Vị trí mới = Vị trí ban đầu + (Camera movement * Parallax factor)
        Vector2 newPos = _startPos + CamMoveSinceStart * parallaxFactor;
        
        // Áp dụng vị trí mới, giữ nguyên Z
        transform.position = new Vector3(newPos.x, newPos.y, _startingZ);
        
    }
    
    #region Debug
    void OnDrawGizmosSelected()
    {
        // Vẽ debug info trong Scene view
        if (cam != null && followTarget != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_startPos, 0.5f);
            
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, followTarget.position);
        }
    }
    #endregion
}