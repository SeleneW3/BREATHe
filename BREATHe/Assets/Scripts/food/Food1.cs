using UnityEngine;

public class Food1 : MonoBehaviour
{
    [Header("Food Settings")]
    [SerializeField] private string pickupSound = "Pickup";  // 拾取音效名称
    
    [Header("Animation Settings")]
    [SerializeField] private float rotateSpeed = 100f;      // 旋转速度
    [SerializeField] private float floatSpeed = 2f;         // 上下浮动速度
    [SerializeField] private float floatHeight = 0.5f;      // 浮动高度
    
    private Vector3 startPos;  // 初始位置

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        // 旋转动画
        transform.Rotate(Vector3.forward * rotateSpeed * Time.deltaTime);
        
        // 上下浮动动画
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 播放拾取音效
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(pickupSound);
            }

            // 触发加速效果
            if (PlayerManager.Instance != null)
            {
                PlayerManager.Instance.ActivateSpeedBoost();
            }

            // 销毁食物
            Destroy(gameObject);
        }
    }
}
