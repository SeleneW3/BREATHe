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

    public float bounceStrength = 50f;
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
        light2D.pointLightOuterRadius = 2f + UDPReceiver.Instance.Intensity * 10f;

        rb.AddForce(Vector2.up * UDPReceiver.Instance.Intensity * 1000); // 根据呼吸强度动态调整跳跃力度
        
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
            Vector2 bounceForce = Vector2.up * bounceStrength;
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * bounceDamping); 

            rb.AddForce(bounceForce, ForceMode2D.Impulse);
            Debug.Log("与地面碰撞，触发反弹!");
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
