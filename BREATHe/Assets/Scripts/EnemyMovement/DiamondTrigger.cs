using UnityEngine;

public class DiamondTrigger : MonoBehaviour
{
    public DiamondFall[] diamonds;  // 改为数组，可以控制多个钻石

    private void Start()
    {
        if (diamonds == null || diamonds.Length == 0)
        {
            // 如果没有手动设置，尝试在父物体及其子物体中查找所有 DiamondFall
            diamonds = transform.parent?.GetComponentsInChildren<DiamondFall>();
            
            if (diamonds == null || diamonds.Length == 0)
            {
                Debug.LogWarning($"[DiamondTrigger] {gameObject.name} 未设置任何 DiamondFall 组件！请手动设置或确保场景中有 DiamondFall 组件");
            }
            else
            {
                Debug.Log($"[DiamondTrigger] 自动找到 {diamonds.Length} 个钻石");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && diamonds != null)
        {
            Debug.Log($"玩家进入钻石触发区域，触发 {diamonds.Length} 个钻石掉落");
            foreach (var diamond in diamonds)
            {
                if (diamond != null)
                {
                    diamond.StartFalling();
                }
            }
        }
    }
} 