using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerManager : MonoBehaviour
{
    private bool isDead = false; 
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
    }

    private void Update()
    {
        if (!isDead)
        {
            transform.Translate(Vector3.right * moveSpeed * Time.deltaTime);
        }

        light2D.intensity = 0.2f + Mathf.Abs(rb.velocity.y) * 0.5f;
        light2D.pointLightOuterRadius = 2f + UDPReceiver.Instance.Intensity * 5f;

        rb.AddForce(Vector2.up * UDPReceiver.Instance.Intensity * 1000); // 根据呼吸强度动态调整跳跃力度

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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            Vector2 normal = collision.GetContact(0).normal;

            float minAngle = 0f;
            float maxAngle = 0f;

            if (normal.y > 0)
            {
                minAngle = 210f;
                maxAngle = 330f;
            }

            else if (normal.y < 0)
            {
                minAngle = -60f;
                maxAngle = 60f;
            }

            // 随机生成一个角度，并转化为弧度
            float randomAngle = Random.Range(minAngle, maxAngle) * Mathf.Deg2Rad;

            // 计算反弹方向
            Vector2 bounceDirection = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle));

            // 计算反弹力，并应用到刚体
            Vector2 bounceForce = bounceDirection * bounceStrength;

            // 先衰减垂直速度（避免跳跃过大）
            rb.velocity = new Vector2(rb.velocity.x * bounceDamping, rb.velocity.y);

            // 停止水平运动
            rb.velocity = new Vector2(0f, rb.velocity.y);  // 停止水平移动速度

            // 对刚体施加反弹力
            rb.AddForce(bounceForce, ForceMode2D.Impulse); // 施加反弹力

            Debug.Log("与地面碰撞，触发随机反弹!");
        }

        if (collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log($"与 {collision.gameObject.tag} 碰撞，触发死亡逻辑");
            TriggerDeath();
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
