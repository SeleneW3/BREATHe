using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    private void Start()
    {
        Debug.Log($"存档点初始化: {gameObject.name} 位置: {transform.position}");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"触发器检测到碰撞 - 碰撞对象: {other.gameObject.name}, 标签: {other.tag}");

        if (other.CompareTag("Player"))
        {
            // 更新玩家的重生点
            if (PlayerManager.Instance != null)
            {
                Vector3 oldCheckPoint = PlayerManager.Instance.GetCurrentCheckPoint(); // 需要在PlayerManager中添加此方法
                PlayerManager.Instance.UpdateCheckPoint(transform.position);
                Debug.Log($"更新存档点 - 从: {oldCheckPoint} 到: {transform.position}");
            }
            else
            {
                Debug.LogError("PlayerManager.Instance 为空！无法更新存档点");
            }
        }
        else
        {
            Debug.Log($"忽略非玩家碰撞 - 对象: {other.gameObject.name}");
        }
    }

    private void OnDrawGizmos()
    {
        // 在Scene视图中可视化存档点
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
} 