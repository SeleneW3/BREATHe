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
    private float targetIntensity = 0f; // 目标灯光强度
    private enum TutorialStage { NormalBreath, DeepBreath, RapidBreath, Completed }
    private TutorialStage currentStage = TutorialStage.NormalBreath;
    private bool tutorialCompleted = false;

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
            rb.velocity = new Vector2(moveSpeed, rb.velocity.y); // 只改变x轴速度，y轴保持原有速度
        }

        // 如果呼吸强度有效，则调整灯光强度；否则，逐渐衰减到0
        if (UDPReceiver.Instance != null && UDPReceiver.Instance.Intensity > 0)
        {
            targetIntensity = 0.2f + Mathf.Abs(rb.velocity.y) * 0.5f + UDPReceiver.Instance.Intensity * 0.5f;
        }
        else
        {
            targetIntensity = Mathf.Lerp(light2D.intensity, 0f, Time.deltaTime * 2f); // 平滑过渡到0
        }

        light2D.intensity = 0.2f + Mathf.Abs(rb.velocity.y) * 0.5f;
        light2D.pointLightOuterRadius = 2f + UDPReceiver.Instance.Intensity * 5f;

        
        rb.AddForce(Vector2.up * UDPReceiver.Instance.Intensity * 100); // 根据呼吸强度动态调整跳跃力度

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

    private void HandleTutorial()
    {
        switch (currentStage)
        {
            case TutorialStage.NormalBreath:
                // 检测正常呼吸
                if (CheckNormalBreath())
                {
                    currentStage = TutorialStage.DeepBreath;
                    Debug.Log("正常呼吸引导完成，进入深呼吸引导");
                }
                break;
            case TutorialStage.DeepBreath:
                // 检测深呼吸
                if (CheckDeepBreath())
                {
                    currentStage = TutorialStage.RapidBreath;
                    Debug.Log("深呼吸引导完成，进入急促呼吸引导");
                }
                break;
            case TutorialStage.RapidBreath:
                // 检测急促呼吸
                if (CheckRapidBreath())
                {
                    currentStage = TutorialStage.Completed;
                    tutorialCompleted = true;
                    Debug.Log("新手引导完成，进入正常游戏");
                }
                break;
        }
    }

    private bool CheckNormalBreath()
    {
        // 在这里实现正常呼吸的检测逻辑
        return false;
    }

    private bool CheckDeepBreath()
    {
        // 在这里实现深呼吸的检测逻辑
        return false;
    }

    private bool CheckRapidBreath()
    {
        // 在这里实现急促呼吸的检测逻辑
        return false;
    }
}
