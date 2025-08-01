using UnityEngine;
using TMPro;

public class HealthText : MonoBehaviour
{
    public Vector3 moveSpeed = new Vector3(0, 75, 0);
    public float timeToFade = 1f;
    
    private RectTransform textTransform;
    private TMP_Text textMeshPro;

    void Awake()
    {
        textTransform = GetComponent<RectTransform>();
        textMeshPro = GetComponent<TMP_Text>();


        // Bắt đầu làm mờ ngay lập tức
        // Tham số: targetAlpha, duration, ignoreTimeScale
        textMeshPro.CrossFadeAlpha(0, timeToFade, false);
    }

    void Update()
    {
        // Di chuyển text lên trên
        textTransform.position += moveSpeed * Time.deltaTime;
        
        // Hủy đối tượng sau khi hết thời gian tồn tại
        // Thêm một khoảng nhỏ 0.1s để chắc chắn hiệu ứng fade đã hoàn thành
        Destroy(gameObject, timeToFade + 0.1f); 
    }
}