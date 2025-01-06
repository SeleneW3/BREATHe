using UnityEngine;

public class Food1 : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float rotateSpeed = 100f;      // 旋转速度
    [SerializeField] private float floatSpeed = 2f;         // 上下浮动速度
    [SerializeField] private float floatHeight = 0.5f;      // 浮动高度
    
    private Vector3 startPos;  // 初始位置
    private bool isCollected = false;
    private SpriteRenderer spriteRenderer;
    private Collider2D myCollider;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        myCollider = GetComponent<Collider2D>();
    }

    private void Start()
    {
        startPos = transform.position;
        ResetFood();
    }

    private void Update()
    {
        if (!isCollected)
        {
            // 旋转动画
            transform.Rotate(Vector3.forward * rotateSpeed * Time.deltaTime);
            
            // 上下浮动动画
            float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isCollected && collision.CompareTag("Player"))
        {
            // 播放食物收集音效（循环使用三种音效）
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayFoodSound();
            }

            // 触发加速效果
            if (PlayerManager.Instance != null)
            {
                PlayerManager.Instance.ActivateSpeedBoost();
            }

            // 隐藏食物
            isCollected = true;
            if (spriteRenderer != null) spriteRenderer.enabled = false;
            if (myCollider != null) myCollider.enabled = false;
        }
    }

    public void ResetFood()
    {
        transform.position = startPos;
        isCollected = false;
        
        // 重新启用渲染和碰撞
        if (spriteRenderer != null) spriteRenderer.enabled = true;
        if (myCollider != null) myCollider.enabled = true;
    }

    private void OnEnable()
    {
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.OnPlayerRespawn += ResetFood;
        }
    }

    private void OnDisable()
    {
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.OnPlayerRespawn -= ResetFood;
        }
    }
}
