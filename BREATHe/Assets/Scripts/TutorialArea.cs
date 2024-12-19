using UnityEngine;

public class TutorialArea : MonoBehaviour
{
    private bool hasEndedTutorial = false; // 添加一个标志位

    // 当玩家进入这个区域时触发
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 检查进入的对象是否是玩家
        if (other.CompareTag("Player"))
        {
            // 输出日志，表示玩家进入了新手引导区域
            Debug.Log("[TutorialArea] 玩家已进入新手引导区域，结束新手引导。");

            // 调用 UDPReceiver 中的 EndTutorial 方法，结束新手引导
            UDPReceiver.Instance.EndTutorial();

            // 设置标志位，确保只调用一次
            hasEndedTutorial = true;
        }
    }
}

