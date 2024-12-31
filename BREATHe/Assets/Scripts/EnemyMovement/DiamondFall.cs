using UnityEngine;

public class DiamondFall : MonoBehaviour
{
    public float fallSpeed = 8.0f;
    private Vector3 startPosition;  // 存储初始位置
    private bool canFall = false;

    void Start()
    {
        // 保存完整的初始位置
        startPosition = transform.position;
    }

    void Update()
    {
        if (canFall)
        {
            Fall();
        }
    }

    private void Fall()
    {
        float newY = transform.position.y - fallSpeed * Time.deltaTime;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    // 提供一个公共方法让触发器调用
    public void StartFalling()
    {
        canFall = true;
    }

    // 重置钻石位置和状态
    public void ResetDiamond()
    {
        transform.position = startPosition;
        canFall = false;
    }

    // 订阅玩家重生事件
    private void OnEnable()
    {
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.OnPlayerRespawn += ResetDiamond;
        }
    }

    // 取消订阅事件
    private void OnDisable()
    {
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.OnPlayerRespawn -= ResetDiamond;
        }
    }
}
