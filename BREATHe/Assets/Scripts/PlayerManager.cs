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

    // 添加校准相关变量
    public bool isCalibrating = false;
    [SerializeField] private float minBreathForce = 5f;    // 最小跳跃力
    [SerializeField] private float maxBreathForce = 15f;   // 最大跳跃力
    [SerializeField] private float recordedMinIntensity = float.MaxValue;  // 记录校准过程中的最小呼吸强度
    [SerializeField] private float recordedMaxIntensity = float.MinValue;  // 记录校准过程中的最大呼吸强度

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

        if (UDPReceiver.Instance != null)
        {
            // 处理呼吸数据
            float intensity = UDPReceiver.Instance.Intensity;
            float frequency = UDPReceiver.Instance.Frequency;
            float breathDuration = UDPReceiver.Instance.BreathDuration;

            // 调整灯光
            float baseIntensity = 0.8f;  // 基础亮度
            if (UDPReceiver.Instance.IsBreathing)  // 使用呼吸状态来控制灯光
            {
                targetIntensity = baseIntensity + Mathf.Abs(rb.velocity.y) * 0.5f + intensity * 0.5f;
                light2D.pointLightOuterRadius = 2f + intensity * 5f;
            }
            else
            {
                targetIntensity = baseIntensity;  // 直接设置为基础亮度
                light2D.pointLightOuterRadius = 2f;  // 恢复基础半径
            }
            light2D.intensity = Mathf.Lerp(light2D.intensity, targetIntensity, Time.deltaTime * 5f);  // 平滑过渡

            // 校准和跳跃逻辑
            if (isCalibrating)
            {
                // 在校准模式下，当收到新的呼吸总结数据时更新范围
                if (UDPReceiver.Instance.LastMaxIntensity > 0)  // 使用 LastMaxIntensity > 0 作为收到新数据的标志
                {
                    recordedMinIntensity = Mathf.Min(recordedMinIntensity, UDPReceiver.Instance.LastMinIntensity);
                    recordedMaxIntensity = Mathf.Max(recordedMaxIntensity, UDPReceiver.Instance.LastMaxIntensity);
                    Debug.Log($"校准中 - 本次呼吸范围: {UDPReceiver.Instance.LastMinIntensity:F4} ~ {UDPReceiver.Instance.LastMaxIntensity:F4}, " +
                            $"当前记录范围: {recordedMinIntensity:F4} ~ {recordedMaxIntensity:F4}");
                }
            }
            else
            {
                // 只在检测到呼吸时进行跳跃
                if (UDPReceiver.Instance.IsBreathing)
                {
                    float jumpForce = MapBreathToForce(intensity);
                    rb.AddForce(Vector2.up * jumpForce);
                }
            }

            // 调整透明度
            float alpha = Mathf.Clamp01(intensity);
            Color currentColor = spriteRenderer.color;
            spriteRenderer.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
        }
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

    // 新增：开始校准
    public void StartCalibration()
    {
        isCalibrating = true;
        recordedMinIntensity = float.MaxValue;
        recordedMaxIntensity = float.MinValue;
        Debug.Log("开始呼吸强度校准");
    }

    // 新增：结束校准
    public void EndCalibration()
    {
        isCalibrating = false;
        Debug.Log($"校准完成 - 最小强度: {recordedMinIntensity:F4}, 最大强度: {recordedMaxIntensity:F4}");
        
        // 可以选择保存这些值到 PlayerPrefs
        PlayerPrefs.SetFloat("MinBreathIntensity", recordedMinIntensity);
        PlayerPrefs.SetFloat("MaxBreathIntensity", recordedMaxIntensity);
        PlayerPrefs.Save();
    }

    // 新增：将呼吸强度映射到跳跃力
    private float MapBreathToForce(float intensity)
    {
        if (recordedMaxIntensity <= recordedMinIntensity)
        {
            return minBreathForce; // 防止除以零
        }

        // 将当前强度映射到校准范围内
        float normalizedIntensity = Mathf.InverseLerp(recordedMinIntensity, recordedMaxIntensity, intensity);
        // 将归一化的强度映射到力的范围
        return Mathf.Lerp(minBreathForce, maxBreathForce, normalizedIntensity);
    }
}
