using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public float baseMoveSpeed = 10f; // 敌人基础移动速度
    private float currentMoveSpeed;   // 当前移动速度

    void Start()
    {
        // 初始化敌人速度
        currentMoveSpeed = baseMoveSpeed;
    }

    private void Update()
    {
        MoveLeft();
    }

    // 敌人向左移动
    void MoveLeft()
    {
        transform.Translate(Vector2.left * currentMoveSpeed * Time.deltaTime);
    }

    // 映射时间因子到敌人移动速度
    public void UpdateMoveSpeed(float timeScale)
    {
        // 将 timeScale 从 0.1-5.0 映射到移动速度 2f（慢）到 10f（快）
        currentMoveSpeed = Mathf.Lerp(2f, baseMoveSpeed, (timeScale - 0.1f) / (5.0f - 0.1f));
        Debug.Log($"Enemy Move Speed Updated: {currentMoveSpeed} (Time Scale: {timeScale})");
    }

    // 检测与玩家的触发碰撞
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("敌人碰到玩家！");

            // 调用玩家管理器的死亡方法
            PlayerManager playerManager = other.GetComponent<PlayerManager>();
            if (playerManager != null)
            {
                playerManager.TriggerDeath();
            }
        }
    }
}



