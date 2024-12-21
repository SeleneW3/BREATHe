using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    private bool isDead = false; 
    public static PlayerManager Instance;

    private UIManager uiManager; 
    public float moveSpeed = 5f;

    Vector3 initialTransform;

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
    }

    private void Update()
    {
        if (!isDead)
        {

            transform.Translate(Vector3.right * moveSpeed * Time.deltaTime);
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
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Enemy"))
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
