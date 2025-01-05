using UnityEngine;

public class WindArea : MonoBehaviour
{
    [Header("Wind Settings")]
    [SerializeField] private float windForce = 1f;    // 风力大小
    [SerializeField] private Vector2 windDirection = Vector2.left;  // 风向，默认向左
    
    [Header("Effects")]
    [SerializeField] private ParticleSystem windParticles;  // 风的粒子效果
    [SerializeField] private string windSoundName = "Wind";  // 风声音效的名称
    private bool isPlayingSound = false;  // 是否正在播放声音

    private void Start()
    {
        // 确保粒子系统正确设置
        if (windParticles != null)
        {
            var main = windParticles.main;
            // 设置粒子系统为循环播放
            main.loop = true;
            // 启动粒子系统
            windParticles.Play();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 进入风区域时播放声音
            if (AudioManager.Instance != null && !isPlayingSound)
            {
                AudioManager.Instance.PlaySound(windSoundName);
                isPlayingSound = true;
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 获取玩家的 Rigidbody2D
            Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // 施加风力
                rb.AddForce(windDirection * windForce);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 离开风区域时停止声音
            if (AudioManager.Instance != null && isPlayingSound)
            {
                AudioManager.Instance.StopSound(windSoundName);
                isPlayingSound = false;
            }
        }
    }

    private void OnDisable()
    {
        // 确保在组件禁用时停止声音
        if (AudioManager.Instance != null && isPlayingSound)
        {
            AudioManager.Instance.StopSound(windSoundName);
            isPlayingSound = false;
        }
    }

    // 可视化风区域（仅在编辑器中）
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 1, 1, 0.2f);  // 青色半透明
        Gizmos.DrawCube(transform.position, transform.localScale);
        
        // 绘制风向箭头
        Vector3 center = transform.position;
        Vector3 direction = (Vector3)(windDirection.normalized * transform.localScale.x * 0.5f);
        Gizmos.DrawRay(center, direction);
    }
} 