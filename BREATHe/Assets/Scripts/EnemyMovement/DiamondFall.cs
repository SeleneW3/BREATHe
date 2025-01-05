using UnityEngine;

public class DiamondFall : MonoBehaviour
{
    [Header("Diamond Settings")]
    [SerializeField] private float fallSpeed = 8.0f;
    [SerializeField] private string collectSound = "Collect";  // 收集音效

    private Vector3 startPosition;  // 存储初始位置
    private bool canFall = false;
    private bool isCollected = false;
    private SpriteRenderer spriteRenderer;
    private Collider2D myCollider;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        myCollider = GetComponent<Collider2D>();
    }

    void Start()
    {
        startPosition = transform.position;
        ResetDiamond();  // 确保初始状态正确
    }

    void Update()
    {
        if (!isCollected && canFall)
        {
            Fall();
        }
    }

    private void Fall()
    {
        float newY = transform.position.y - fallSpeed * Time.deltaTime;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isCollected && collision.CompareTag("Player"))
        {
            Collect();
        }
    }

    private void Collect()
    {
        isCollected = true;

        // 播放收集音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(collectSound);
        }

        // 触发玩家动画
        if (PlayerManager.Instance != null)
        {
            // 假设 PlayerManager 有一个播放涟漪动画的方法
            PlayerManager.Instance.PlayRippleEffect();
        }

        // 隐藏钻石
        if (spriteRenderer != null) spriteRenderer.enabled = false;
        if (myCollider != null) myCollider.enabled = false;
    }

    public void StartFalling()
    {
        canFall = true;
    }

    public void ResetDiamond()
    {
        transform.position = startPosition;
        canFall = false;
        isCollected = false;
        
        // 重新启用渲染和碰撞
        if (spriteRenderer != null) spriteRenderer.enabled = true;
        if (myCollider != null) myCollider.enabled = true;
    }

    private void OnEnable()
    {
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.OnPlayerRespawn += ResetDiamond;
        }
    }

    private void OnDisable()
    {
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.OnPlayerRespawn -= ResetDiamond;
        }
    }
}
