using UnityEngine;

public class HoleBreath : MonoBehaviour
{
    public float maxAttractionStrength = 10f;  // 吸引力
    public CircleCollider2D attractionArea;    // 吸引范围的碰撞器
    private Rigidbody2D playerRb;              // 玩家的刚体组件
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
        if (isPlayerInRange && playerRb != null && attractionArea != null)
        {
            // 计算竖直方向的吸引力
            float verticalDifference = transform.position.y - playerRb.position.y;
            float distance = Mathf.Abs(verticalDifference);
            
            // 计算吸引力方向（只在Y轴）
            Vector2 attractionForce = new Vector2(
                0f,  // X方向力为0
                verticalDifference  // Y方向力
            ).normalized * maxAttractionStrength;
            
            // 使用力来移动玩家，而不是直接修改位置
            playerRb.AddForce(attractionForce);
        }
    }

    public void OnPlayerEnterRange(Transform playerTransform)
    {
        playerRb = playerTransform.GetComponent<Rigidbody2D>();
        isPlayerInRange = true;
        Debug.Log("玩家进入吸引范围");
    }

    public void OnPlayerExitRange()
    {
        playerRb = null;
        isPlayerInRange = false;
        Debug.Log("玩家离开吸引范围");
    }

    private void OnValidate()
    {
        if (attractionArea != null && !attractionArea.isTrigger)
        {
            Debug.LogWarning("Attraction Area 的 Is Trigger 属性未启用！");
            attractionArea.isTrigger = true;
        }
    }
}
