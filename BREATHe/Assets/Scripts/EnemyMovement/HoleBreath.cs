using UnityEngine;

public class HoleBreath : MonoBehaviour
{
    public float maxAttractionStrength = 10f;  // 吸引力
    public CircleCollider2D attractionArea;    // 吸引范围的碰撞器
    public Transform player;
    private bool isPlayerInRange = false;

    public ParticleSystem attractionParticles;

    void Start()
    {
        attractionParticles.Play();
        if (attractionArea == null)
        {
            Debug.LogError("吸引范围碰撞器未设置！请在检查器中设置 Attraction Area");
        }
    }

    private void Update()
    {
        if (isPlayerInRange && player != null && attractionArea != null)
        {
            // 只计算竖直方向的位置差
            float verticalDifference = transform.position.y - player.position.y;
            float distance = Mathf.Abs(verticalDifference);

            // 使用指定碰撞器的半径作为最大距离
            float attractionRadius = attractionArea.radius;
            
            // 计算吸引力
            float attractionStrength = maxAttractionStrength;
            
            // 只在竖直方向移动
            Vector3 newPosition = player.position;
            newPosition.y = Mathf.MoveTowards(
                player.position.y, 
                transform.position.y, 
                attractionStrength * Time.deltaTime
            );
            
            // 保持x位置不变，只更新y位置
            player.position = newPosition;
        }
    }

    // 新增这两个方法供 AttractionTrigger 调用
    public void OnPlayerEnterRange(Transform playerTransform)
    {
        player = playerTransform;
        isPlayerInRange = true;
        Debug.Log("玩家进入吸引范围");
    }

    public void OnPlayerExitRange()
    {
        player = null;
        isPlayerInRange = false;
        Debug.Log("玩家离开吸引范围");
    }

    private void OnValidate()
    {
        // 在编辑器中验证碰撞器设置
        if (attractionArea != null)
        {
            if (!attractionArea.isTrigger)
            {
                Debug.LogWarning("Attraction Area 的 Is Trigger 属性未启用！");
                attractionArea.isTrigger = true;
            }
        }
    }
}
