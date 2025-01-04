using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

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

    // 修改校准相关变量的可见性
    [Header("校准设置")]
    public bool isCalibrating = false;  // 改为 public 以便 TutorialArea 访问
    [SerializeField] private float minBreathForce = 5f;
    [SerializeField] private float maxBreathForce = 15f;
    [SerializeField] private float recordedMinIntensity = float.MaxValue;
    [SerializeField] private float recordedMaxIntensity = float.MinValue;

    // 添加公共访问器
    public float RecordedMinIntensity => recordedMinIntensity;
    public float RecordedMaxIntensity => recordedMaxIntensity;

    private float lastJumpTime = 0f;
    private int jumpSoundIndex = 0;
    private string[] jumpSounds = { "Jump1", "Jump2", "Jump3", "Jump4", "Jump5" };
    private float jumpSoundInterval = 1f; // 1 second interval

    private int bounceIndex = 0;
    private string[] bounceSounds = { "bounce1", "bounce2", "bounce3", "bounce4" };

    [Header("呼吸力度")]
    [SerializeField] private float currentBreathForce;    // 当前呼吸产生的力
    [SerializeField] private float lastMappedForce;       // 最后一次映射的力
    [SerializeField] private float currentIntensity;      // 当前呼吸强度

    private Vector3 currentCheckPoint;  // 当前存档点位置

    // 添加重生事件
    public event System.Action OnPlayerRespawn;

    [Header("Speed Boost")]
    [SerializeField] private float speedBoostAmount = 2f;    // 加速倍数
    [SerializeField] private float speedBoostDuration = 3f;  // 加速持续时间
    [SerializeField] private GameObject rippleEffectPrefab;  // 涟漪预制体
    [SerializeField] private float rippleAnimationDuration = 0.5f;  // 涟漪动画持续时间
    private Coroutine speedBoostCoroutine;

    [Header("Ground Effects")]
    [SerializeField] private ParticleSystem groundParticles;  // 地面粒子效果
    [SerializeField] private float effectDuration = 1f;       // 效果持续时间

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

        // 订阅呼吸开始事件
        if (UDPReceiver.Instance != null)
        {
            UDPReceiver.Instance.OnBreathStarted += PlayJumpSound;
        }

        // 初始化存档点为起始位置
        currentCheckPoint = initialTransform;

        // 加载保存的校准值
        if (PlayerPrefs.HasKey("MinBreathIntensity") && PlayerPrefs.HasKey("MaxBreathIntensity"))
        {
            recordedMinIntensity = PlayerPrefs.GetFloat("MinBreathIntensity");
            recordedMaxIntensity = PlayerPrefs.GetFloat("MaxBreathIntensity");
            Debug.Log($"[PlayerManager] 加载校准值: {recordedMinIntensity:F4} ~ {recordedMaxIntensity:F4}");
        }
    }

    private void OnDestroy()
    {
        // 取消订阅事件，防止内存泄漏
        if (UDPReceiver.Instance != null)
        {
            UDPReceiver.Instance.OnBreathStarted -= PlayJumpSound;
        }
    }

    private void Update()
    {
        // 只在非校准且非死亡状态下进行水平移动
        if (!isDead && !isCalibrating)
        {
            rb.velocity = new Vector2(moveSpeed, rb.velocity.y);
        }
        else
        {
            // 在校准或死亡状态下停止水平移动，但保持垂直速度
            rb.velocity = new Vector2(0f, rb.velocity.y);
        }

        if (UDPReceiver.Instance != null)
        {
            // 处理呼吸数据
            float frequency = UDPReceiver.Instance.Frequency;
            float breathDuration = UDPReceiver.Instance.BreathDuration;
            float intensity = UDPReceiver.Instance.Intensity;
            currentIntensity = intensity;  // 更新当前强度

            // 调整灯光
            float baseIntensity = 0.8f;
            if (UDPReceiver.Instance.IsBreathing)
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
                // 实时更新校准范围
                if (intensity > 0)  // 只在有效强度时更新
                {
                    recordedMinIntensity = Mathf.Min(recordedMinIntensity, intensity);
                    recordedMaxIntensity = Mathf.Max(recordedMaxIntensity, intensity);
                    Debug.Log($"校准中 - 当前强度: {intensity:F4}, 更新范围: {recordedMinIntensity:F4} ~ {recordedMaxIntensity:F4}");
                }
                currentBreathForce = 0f;
                lastMappedForce = 0f;
            }
            else
            {
                // 计算映射后的力
                float mappedForce = MapBreathToForce(intensity);
                lastMappedForce = mappedForce;  // 更新最后映射的力

                // 只在检测到呼吸时进行跳跃，并且强度大于阈值
                if (UDPReceiver.Instance.IsBreathing && intensity > 0.01f)  // 添加最小阈值检查
                {
                    currentBreathForce = mappedForce;  // 更新当前实际施加的力
                    rb.AddForce(Vector2.up * mappedForce);
                    Debug.LogWarning($"施加力 - 强度: {intensity:F4}, 力: {mappedForce:F4}");
                }
                else
                {
                    currentBreathForce = 0f;
                }
            }

            // 调整透明度
            float alpha = Mathf.Clamp01(intensity);
            Color currentColor = spriteRenderer.color;
            spriteRenderer.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
        }
    }

    private void PlayJumpSound()
    {
        if (AudioManager.Instance != null && !isDead)  // 添加死亡检查
        {
            float currentTime = Time.time;
            if (currentTime - lastJumpTime > jumpSoundInterval)
            {
                jumpSoundIndex = 0; // Reset index if more than 1 second has passed
            }

            AudioManager.Instance.PlaySound(jumpSounds[jumpSoundIndex]);
            jumpSoundIndex = (jumpSoundIndex + 1) % jumpSounds.Length; // Cycle through sounds
            lastJumpTime = currentTime;
        }
    }

    public void TriggerDeath()
    {
        if (isDead) return;
        isDead = true;

        // 停止所有正在播放的音效
        if (AudioManager.Instance != null)
        {
            // 停止所有跳跃音效
            foreach (string jumpSound in jumpSounds)
            {
                AudioManager.Instance.StopSound(jumpSound);
            }

            // 停止所有弹跳音效
            foreach (string bounceSound in bounceSounds)
            {
                AudioManager.Instance.StopSound(bounceSound);
            }

            // 处理背景音乐和死亡音效
            AudioManager.Instance.HandlePlayerDeath();
            AudioManager.Instance.PlaySound("Death");
        }

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
        else if (collision.gameObject.CompareTag("Ground"))
        {
            // 播放弹跳音效
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(bounceSounds[bounceIndex]);
                bounceIndex = (bounceIndex + 1) % bounceSounds.Length;
            }

            // 在碰撞点播放粒子效果
            if (groundParticles != null)
            {
                Vector2 contactPoint = collision.GetContact(0).point;
                ParticleSystem effect = Instantiate(groundParticles, contactPoint, Quaternion.identity);
                
                // 获取动画组件
                Animator animator = effect.GetComponent<Animator>();
                if (animator != null)
                {
                    // 获取动画片段的实际长度
                    float animationLength = animator.GetCurrentAnimatorStateInfo(0).length;
                    // 使用动画长度或指定的持续时间
                    Destroy(effect.gameObject, animationLength > 0 ? animationLength : effectDuration);
                }
                else
                {
                    // 如果没有动画组件，使用指定的持续时间
                    Destroy(effect.gameObject, effectDuration);
                }
            }
        }
    }

    public void InitializePos()
    {
        // 只在游戏第一次开始时调用这个方法
        currentCheckPoint = initialTransform;
        transform.position = initialTransform;
        Debug.Log($"[PlayerManager] 初始化位置到起点: {initialTransform}");
    }

    public void respawn()
    {
        isDead = false;
        
        // 确保使用当前存档点
        Debug.Log($"[PlayerManager] 重生到存档点: {currentCheckPoint}");
        transform.position = currentCheckPoint;
        rb.velocity = Vector2.zero; // 重置速度
        
        // 触发重生事件，重置所有钻石
        OnPlayerRespawn?.Invoke();
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.HandleGameRestart();
        }
    }

    // 新增：开始校准
    public void StartCalibration()
    {
        isCalibrating = true;
        recordedMinIntensity = float.MaxValue;
        recordedMaxIntensity = float.MinValue;
        
        // 停止水平移动
        rb.velocity = new Vector2(0f, rb.velocity.y);
        
        // 启动UI倒计时
        if (uiManager != null)
        {
            uiManager.StartCalibrationUI();
        }
        
        Debug.Log("[PlayerManager] 开始校准 - 重置范围值");
    }

    // 新增：结束校准
    public void EndCalibration()
    {
        if (recordedMaxIntensity <= recordedMinIntensity)
        {
            Debug.LogError("[PlayerManager] 校准失败 - 未检测到有效的呼吸范围!");
            return;
        }

        isCalibrating = false;
        Debug.Log($"[PlayerManager] 校准完成 - 有效范围: {recordedMinIntensity:F4} ~ {recordedMaxIntensity:F4}");
        
        // 保存校准值
        PlayerPrefs.SetFloat("MinBreathIntensity", recordedMinIntensity);
        PlayerPrefs.SetFloat("MaxBreathIntensity", recordedMaxIntensity);
        PlayerPrefs.Save();

        // 恢复水平移动
        rb.velocity = new Vector2(moveSpeed, rb.velocity.y);
    }

    // 新增：将呼强度映射到跳跃力
    private float MapBreathToForce(float intensity)
    {
        // 如果还没有校准过，使用默认范围
        if (recordedMaxIntensity <= recordedMinIntensity)
        {
            Debug.LogWarning($"未校准或校准异常 - 使用默认力度: {minBreathForce}");
            return minBreathForce;
        }

        // 打印调试信息
        //Debug.Log($"映射力度 - 当前强度: {intensity:F4}, 范围: {recordedMinIntensity:F4} ~ {recordedMaxIntensity:F4}");
        
        // 将当前强度映射到校准范围内
        float normalizedIntensity = Mathf.InverseLerp(recordedMinIntensity, recordedMaxIntensity, intensity);
        
        // 将归一化的强度映射到力的范围
        float force = Mathf.Lerp(minBreathForce, maxBreathForce, normalizedIntensity);
        
        //Debug.Log($"映射结果 - 归一化强度: {normalizedIntensity:F4}, 最终力度: {force:F4}");
        
        return force;
    }

    // 更新存档点
    public void UpdateCheckPoint(Vector3 newCheckPoint)
    {
        Debug.Log($"[PlayerManager] 更新存档点 - 当前: {currentCheckPoint}, 新: {newCheckPoint}");
        currentCheckPoint = newCheckPoint;
    }

    // 获取当前存档点位置
    public Vector3 GetCurrentCheckPoint()
    {
        return currentCheckPoint;
    }

    public void ActivateSpeedBoost()
    {
        if (speedBoostCoroutine != null)
        {
            StopCoroutine(speedBoostCoroutine);
        }
        speedBoostCoroutine = StartCoroutine(SpeedBoostRoutine());
    }

    private void PlayRippleEffect()
    {
        if (rippleEffectPrefab != null)
        {
            // 在玩家位置生成涟漪效果
            GameObject ripple = Instantiate(rippleEffectPrefab, transform.position, Quaternion.identity);
            
            // 获取动画组件
            Animator animator = ripple.GetComponent<Animator>();
            if (animator != null)
            {
                // 获取动画片段的实际长度
                float animationLength = animator.GetCurrentAnimatorStateInfo(0).length;
                // 使用动画长度或指定的持续时间
                Destroy(ripple, animationLength > 0 ? animationLength : rippleAnimationDuration);
            }
            else
            {
                // 如果没有动画组件，使用指定的持续时间
                Destroy(ripple, rippleAnimationDuration);
            }
        }
    }

    private IEnumerator SpeedBoostRoutine()
    {
        // 启用加速
        moveSpeed *= speedBoostAmount;
        
        // 播放涟漪效果
        PlayRippleEffect();

        yield return new WaitForSeconds(speedBoostDuration);

        // 恢复正常速度
        moveSpeed /= speedBoostAmount;
    }
}
