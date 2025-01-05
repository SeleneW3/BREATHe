using UnityEngine;

public class DiamondFall : MonoBehaviour
{
    [Header("Diamond Settings")]
    [SerializeField] private float fallSpeed = 8.0f;
    [SerializeField] private string collectSound = "Collect";  // 收集音效
    [SerializeField] private ParticleSystem collectEffect;     // 收集特效

    private Vector3 startPosition;  // 存储初始位置
    private bool canFall = false;
    private bool isCollected = false;

    void Start()
    {
        startPosition = transform.position;
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

        // 播放收集特效
        if (collectEffect != null)
        {
            ParticleSystem effect = Instantiate(collectEffect, transform.position, Quaternion.identity);
            Destroy(effect.gameObject, effect.main.duration);
        }

        // 隐藏钻石
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<Collider2D>().enabled = false;

        // 延迟销毁物体（等待音效和特效播放完）
        Destroy(gameObject, 1f);
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
        if (GetComponent<SpriteRenderer>() != null)
            GetComponent<SpriteRenderer>().enabled = true;
        if (GetComponent<Collider2D>() != null)
            GetComponent<Collider2D>().enabled = true;
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
