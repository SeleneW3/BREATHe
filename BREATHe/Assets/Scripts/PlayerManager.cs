using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerManager : MonoBehaviour
{
    public bool isDead = false; 
    public static PlayerManager Instance;
    public Rigidbody2D rb;

    private UIManager uiManager; 
    public float moveSpeed = 6f;

    Vector3 initialTransform;

    public Light2D light2D;
    public SpriteRenderer spriteRenderer;

    public float bounceStrength = 5f;
    public float bounceDamping = 0.5f; // 反弹衰减 (控制反弹后速度逐渐减小)

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            Debug.LogWarning("destroy!");
        }
        spriteRenderer = GetComponent<SpriteRenderer>();

    }

    private void Start()
    {
        uiManager = FindObjectOfType<UIManager>();
        initialTransform = gameObject.transform.position;
        if (uiManager == null)
        {
            Debug.LogError("UIManager 未找到，请确保它存在并正确设置！");
        }
        rb = GetComponent<Rigidbody2D>();

        // 确保有碰撞器
        var collider = GetComponent<Collider2D>();
        if (collider == null)
        {
            Debug.LogError("Player 缺少 Collider2D 组件！");
        }
    }

    private void Update()
    {

        if (!isDead)
        {
            rb.velocity = new Vector2(moveSpeed, rb.velocity.y);
        }

        light2D.intensity = 0.2f + Mathf.Abs(rb.velocity.y) * 0.5f;
        light2D.pointLightOuterRadius = 2f + UDPReceiver.Instance.Intensity * 5f;

        rb.AddForce(Vector2.up * UDPReceiver.Instance.Intensity * 10); // 根据呼吸强度动态调整跳跃力度

        // 调整角色透明度：intensity越强，透明度越高
        float alpha = Mathf.Clamp01(UDPReceiver.Instance.Intensity);  // 限制透明度在0到1之间
        Color currentColor = spriteRenderer.color;
        spriteRenderer.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
    }


        public void TriggerDeath()
    {
        if (isDead) return; // 防止重复死亡
        isDead = true;

        Debug.Log("玩家死亡，触发死亡逻辑");

        // 显示游戏结束菜单
        if (uiManager != null)
        {
            uiManager.ShowGameOverMenu();
        }
    }

    

    public void InitializePos()
    {
        gameObject.transform.position = initialTransform;
    }

    public void respawn()
    {
        isDead = false;
    }
}
