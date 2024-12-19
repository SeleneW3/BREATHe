using System.Net.Sockets;
using System.Net;
using System.Text;
using UnityEngine.SceneManagement;
using UnityEngine;

public class UDPReceiver : MonoBehaviour
{
    public static UDPReceiver Instance { get; private set; } // 单例模式

    public int port = 65432; // 默认端口
    private UdpClient udpClient;
    private IPEndPoint remoteEndPoint;

    public Rigidbody2D playerRigidbody;   // 2D 角色的 Rigidbody2D 组件
    public float baseJumpForce = 15.0f;   // 基础跳跃力度
    private float adjustedJumpForce;      // 动态调整后的跳跃力度
    private float gravityScale = 1.0f;    // 重力缩放
    private float lastTimeScale = 5.0f;   // 时间因子
    private bool timeScaleUpdated = false;
    private float recoveryRate = 0.1f;  // 恢复速率

    private UIManager uiManager;

    // 新手引导阶段布尔值
    private bool isTutorialStage = false;  // 默认不在新手引导阶段

    // 动态范围阈值
    private float dynamicMinThreshold = 0.005f; // 呼吸强度的最小阈值
    private float dynamicMaxThreshold = 0.02f;  // 呼吸强度的最大阈值

    void Start()
    {
        InitializeUDP();
        SceneManager.sceneLoaded += OnSceneLoaded;

        uiManager = FindObjectOfType<UIManager>();
        TryFindPlayerRigidbody();

        // 自动进入新手引导阶段
        //StartTutorial();
    }

    void Update()
    {
        timeScaleUpdated = false;

        if (udpClient == null)
        {
            Debug.LogWarning("[UDPReceiver] udpClient 未初始化。");
            return;
        }

        if (udpClient.Available > 0)
        {
            Debug.Log("[UDPReceiver] 数据可用，尝试接收数据...");
            try
            {
                byte[] data = udpClient.Receive(ref remoteEndPoint);
                string jsonData = Encoding.UTF8.GetString(data);
                Debug.Log($"[UDPReceiver] 接收到数据: {jsonData}");

                var parsedData = JsonUtility.FromJson<BreathData>(jsonData);
                Debug.Log($"[UDPReceiver] 解析结果 -> time_scale: {parsedData.time_scale}, intensity: {parsedData.intensity}");

                lastTimeScale = parsedData.time_scale;
                timeScaleUpdated = true;

                if (isTutorialStage)
                {
                    // 如果处于新手引导阶段，动态调整阈值
                    AdjustThresholdsDuringTutorial(parsedData);
                }
                else
                {
                    // 否则，继续正常接收数据
                    AdjustJumpAndGravity(lastTimeScale);
                    PerformJump();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[UDPReceiver] 数据接收或解析错误: {e.Message}");
            }
        }

        if (!timeScaleUpdated)
        {
            lastTimeScale = Mathf.MoveTowards(lastTimeScale, 5.0f, recoveryRate * Time.deltaTime);
        }
    }

    private void InitializeUDP()
    {
        try
        {
            udpClient = new UdpClient(port);
            remoteEndPoint = new IPEndPoint(IPAddress.Any, port); // 监听来自任何 IP 的数据
            udpClient.EnableBroadcast = true;  // 如果需要广播功能，可以启用广播
            udpClient.Client.ReceiveTimeout = 100;  // 设置接收超时（毫秒）
            Debug.Log($"[UDPReceiver] UDP 客户端初始化成功，端口号: {port}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[UDPReceiver] 初始化 UDP 客户端时出错: {e.Message}");
        }
    }

    private void AdjustThresholdsDuringTutorial(BreathData parsedData)
    {
        // 新手引导阶段动态调整呼吸强度的阈值
        // 根据玩家的呼吸强度来调整动态阈值，直到新手引导结束
        dynamicMinThreshold = Mathf.Lerp(0.005f, 0.02f, Mathf.Clamp01(parsedData.intensity));
        dynamicMaxThreshold = Mathf.Lerp(0.02f, 0.05f, Mathf.Clamp01(parsedData.intensity));  // 示例范围

        Debug.Log($"[UDPReceiver] 当前动态阈值: minThreshold={dynamicMinThreshold}, maxThreshold={dynamicMaxThreshold}");

        // 向 Python 发送更新后的阈值
        SendThresholdsToPython(dynamicMinThreshold, dynamicMaxThreshold);
    }

    private void AdjustJumpAndGravity(float timeScale)
    {
        timeScale = Mathf.Clamp(timeScale, 0.1f, 5.0f); // 限制时间因子的范围

        // 根据时间因子调整跳跃力度和重力缩放
        adjustedJumpForce = Mathf.Lerp(5f, 15f, (timeScale - 0.1f) / (5.0f - 0.1f));
        gravityScale = Mathf.Lerp(0.5f, 2.0f, (timeScale - 0.1f) / (5.0f - 0.1f));

        if (playerRigidbody != null)
        {
            playerRigidbody.gravityScale = gravityScale;
            Debug.Log($"[UDPReceiver] Adjusted Jump Force: {adjustedJumpForce}, Gravity Scale: {gravityScale}");
        }
        else
        {
            Debug.LogWarning("[UDPReceiver] playerRigidbody 未找到，无法调整重力缩放！");
        }
    }

    private void PerformJump()
    {
        if (playerRigidbody != null)
        {
            playerRigidbody.velocity = new Vector2(playerRigidbody.velocity.x, adjustedJumpForce);
            Debug.Log("[UDPReceiver] Player Jump Triggered!");
        }
        else
        {
            Debug.LogWarning("[UDPReceiver] playerRigidbody 未设置，无法跳跃！");
        }
    }

    // 结束新手引导阶段
    public void EndTutorial()
    {
        isTutorialStage = false;
        Debug.Log("[UDPReceiver] 新手引导已结束，开始正式游戏！");
    }

    // 开始新手引导阶段
    public void StartTutorial()
    {
        isTutorialStage = true;
        Debug.Log("[UDPReceiver] 已进入新手引导阶段。");
    }

    // 向 Python 发送更新后的阈值
    private void SendThresholdsToPython(float minThreshold, float maxThreshold)
    {
        if (udpClient == null)
        {
            Debug.LogError("[UDPReceiver] udpClient 未初始化，无法发送数据！");
            return;
        }

        string thresholdData = $"{{\"min_threshold\": {minThreshold}, \"max_threshold\": {maxThreshold}}}";
        byte[] dataToSend = Encoding.UTF8.GetBytes(thresholdData);
        udpClient.Send(dataToSend, dataToSend.Length, remoteEndPoint);

        Debug.Log($"[UDPReceiver] 向 Python 发送了更新后的阈值: minThreshold={minThreshold}, maxThreshold={maxThreshold}");
    }

    // 监听场景加载
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryFindPlayerRigidbody();
    }

    private void TryFindPlayerRigidbody()
    {
        // 通过标签查找玩家 GameObject，然后获取 Rigidbody2D
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerRigidbody = player.GetComponent<Rigidbody2D>();

            if (playerRigidbody != null)
            {
                Debug.Log("[UDPReceiver] Player Rigidbody 重新绑定成功！");
            }
            else
            {
                Debug.LogWarning("[UDPReceiver] 玩家对象存在，但缺少 Rigidbody2D 组件！");
            }
        }
        else
        {
            Debug.LogWarning("[UDPReceiver] 未找到带有 'Player' 标签的 GameObject！");
        }
    }

    // 释放 UDP 资源
    public void ReleaseUDPResources()
    {
        if (udpClient != null)
        {
            udpClient.Close();
            udpClient = null;
            Debug.Log("[UDPReceiver] UDP 客户端已关闭，并释放端口。");
        }
    }

    void OnApplicationQuit()
    {
        ReleaseUDPResources();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        ReleaseUDPResources();
    }

    [System.Serializable]
    public class BreathData
    {
        public float time;
        public float intensity;
        public float time_scale;
    }
}