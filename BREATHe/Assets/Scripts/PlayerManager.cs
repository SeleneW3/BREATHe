using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    private bool isDead = false; // 玩家是否死亡
    public static PlayerManager Instance; // 单例模式

    private UIManager uiManager; // 引用 UIManager
    public float moveSpeed = 5f; // 角色的自动前进速度

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 查找 UIManager
        uiManager = FindObjectOfType<UIManager>();
        if (uiManager == null)
        {
            Debug.LogError("UIManager 未找到，请确保场景中存在并正确设置！");
        }
    }

    private void Update()
    {
        if (!isDead)
        {
            // 角色自动向右前进
            transform.Translate(Vector3.right * moveSpeed * Time.deltaTime);
        }
    }

    public void TriggerDeath()
    {
        if (isDead) return; // 防止重复调用
        isDead = true;

        Debug.Log("玩家死亡，触发死亡逻辑！");

        // 显示游戏结束画面（得分等 UI）
        if (uiManager != null)
        {
            uiManager.ShowGameOverMenu();
        }
    }

    private void PauseAndResetScene(float delay)
    {
        // 暂停游戏
        Time.timeScale = 0f;

        // 延迟调用重置场景
        StartCoroutine(ResetSceneAfterDelay(delay));
    }

    private System.Collections.IEnumerator ResetSceneAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay); // 等待真实时间

        // 恢复时间流动
        Time.timeScale = 1f;

        // 重置场景
        ResetScene();
    }

    private void ResetScene()
    {
        Debug.Log("重置场景！");

        // 直接通过单例调用 UDPReceiver 的 ReleaseUDPResources
        if (UDPReceiver.Instance != null)
        {
            UDPReceiver.Instance.ReleaseUDPResources();
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        ); // 重新加载当前场景
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 检测碰撞对象是否是地面或敌人
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log($"玩家碰到 {collision.gameObject.tag}，触发死亡逻辑！");
            TriggerDeath();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 检测触发对象是否是地面或敌人
        if (other.CompareTag("Ground") || other.CompareTag("Enemy"))
        {
            Debug.Log($"玩家触发 {other.gameObject.tag}，触发死亡逻辑！");
            TriggerDeath();
        }
    }
}
