using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class BaseBreathData
{
    public string type;  // 消息类型："update" 或 "state_change"
}

[System.Serializable]
public class UpdateData : BaseBreathData
{
    public bool is_breathing;    // 当前是否在呼吸
    public float intensity;      // 实时呼吸强度
    public float frequency;      // 添加频率字段
}

[System.Serializable]
public class StateChangeData : BaseBreathData
{
    public bool is_breathing;    // 呼吸状态
    public float time;           // 时间戳
    public float intensity;      // 当前强度
    public float frequency;      // 呼吸频率
    public int breath_count;     // 呼吸次数
}

public class UDPReceiver : MonosingletonTemp<UDPReceiver>
{
    public int port = 65432;
    
    // 实时数据
    [SerializeField] private float intensity = 0f;     // 当前呼吸强度
    public float Intensity { get => intensity; private set => intensity = value; }
    
    // 总结数据（只在呼吸结束时更新）
    [SerializeField] private float frequency = 0f;     // 当前呼吸频率
    public float Frequency { get => frequency; private set => frequency = value; }
    
    [SerializeField] private float breathDuration = 0f; // 当前呼吸持续时间
    public float BreathDuration { get => breathDuration; private set => breathDuration = value; }
    
    [SerializeField] private float averageIntensity = 0f; // 平均呼吸强度
    public float AverageIntensity { get => averageIntensity; private set => averageIntensity = value; }

    private UdpClient udpClient;
    private IPEndPoint remoteEndPoint;
    private List<float> intensityValues = new List<float>();
    public bool isMeasuring = false;

    [SerializeField] private float lastMaxIntensity = 0f;  // 最近一次呼吸的最大强度
    public float LastMaxIntensity { get => lastMaxIntensity; private set => lastMaxIntensity = value; }
    
    [SerializeField] private float lastMinIntensity = 0f;  // 最近一次呼吸的最小强度
    public float LastMinIntensity { get => lastMinIntensity; private set => lastMinIntensity = value; }

    [SerializeField] private bool isBreathing = false;  // 当前是否在呼吸
    public bool IsBreathing { get => isBreathing; private set => isBreathing = value; }
    
    private float lastBreathTime = 0f;  // 上次收到呼吸数据的时间
    private const float BREATH_TIMEOUT = 0.1f;  // 如果超过这个时间没收到数据，认为不在呼吸

    [SerializeField] private int breathCount = 0;  // 添加呼吸次数字段
    public int BreathCount { get => breathCount; private set => breathCount = value; }

    // 添加一个事件，用于通知呼吸开始
    public event Action OnBreathStarted;

    void Start()
    {
        InitializeUDP();
    }

    void Update()
    {
        // 检查是否超时
        if (Time.time - lastBreathTime > BREATH_TIMEOUT)
        {
            IsBreathing = false;
            Intensity = 0f;  // 清零强度
        }

        if (udpClient == null)
        {
            Debug.LogWarning("[UDPReceiver] udpClient not initialized, attempting to reinitialize...");
            InitializeUDP();
            return;
        }

        try 
        {    
            while (udpClient.Available > 0)
            {
                byte[] data = udpClient.Receive(ref remoteEndPoint);
                string jsonData = Encoding.UTF8.GetString(data);
                
                // 添加更详细的数据包检查
                Debug.Log($"[UDPReceiver] 原始数据包: {jsonData}");
                Debug.Log($"[UDPReceiver] 数据包长度: {data.Length} 字节");
                Debug.Log($"[UDPReceiver] 发送方地址: {remoteEndPoint.Address}:{remoteEndPoint.Port}");

                var baseData = JsonUtility.FromJson<BaseBreathData>(jsonData);
                if (baseData != null)
                {
                    Debug.Log($"[UDPReceiver] 消息类型: {baseData.type}");
                    
                    switch (baseData.type)
                    {
                        case "update":
                            var updateData = JsonUtility.FromJson<UpdateData>(jsonData);
                            if (updateData != null)
                            {
                                Debug.Log($"[UDPReceiver] Update 数据包解析结果:");
                                Debug.Log($"  - is_breathing: {updateData.is_breathing}");
                                Debug.Log($"  - intensity: {updateData.intensity}");
                                Debug.Log($"  - frequency: {updateData.frequency}");

                                bool wasBreathing = IsBreathing;
                                Intensity = updateData.intensity;
                                IsBreathing = updateData.is_breathing;
                                Frequency = updateData.frequency;
                                lastBreathTime = Time.time;
                            }
                            break;

                        case "state_change":
                            var stateData = JsonUtility.FromJson<StateChangeData>(jsonData);
                            if (stateData != null)
                            {
                                bool wasBreathing = IsBreathing;
                                Intensity = stateData.intensity;
                                IsBreathing = stateData.is_breathing;
                                Frequency = stateData.frequency;  // 更新频率
                                BreathCount = stateData.breath_count;
                                lastBreathTime = Time.time;

                                Debug.Log($"[UDPReceiver] StateChange -> 频率: {stateData.frequency} -> {Frequency}");
                            }
                            break;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[UDPReceiver] 接收错误: {e.Message}\n{e.StackTrace}");
        }

        if (isMeasuring && Intensity > 0)
        {
            intensityValues.Add(Intensity);
        }
        else
        {
            isMeasuring = false;
            AverageIntensity = CalculateAverageIntensity(intensityValues);
        }
    }

    private void InitializeUDP()
    {
        try
        {
            if (udpClient != null)
            {
                Debug.Log("[UDPReceiver] Closing existing UDP client before reinitializing...");
                ReleaseUDPResources();
            }

            udpClient = new UdpClient(port);
            remoteEndPoint = new IPEndPoint(IPAddress.Any, port);
            udpClient.EnableBroadcast = true;
            Debug.Log($"[UDPReceiver] UDP client initialized successfully on port: {port}");
            
            // 测试 UDP 是否正常工作
            byte[] testData = Encoding.UTF8.GetBytes("test");
            try
            {
                udpClient.Send(testData, testData.Length, "127.0.0.1", port);
                Debug.Log("[UDPReceiver] Test packet sent successfully");
            }
            catch (Exception e)
            {
                Debug.LogError($"[UDPReceiver] Failed to send test packet: {e.Message}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[UDPReceiver] Error initializing UDP client: {e.Message}");
            udpClient = null;
        }
    }


    public void ReleaseUDPResources()
    {
        if (udpClient != null)
        {
            udpClient.Close();
            udpClient = null;
            Debug.Log("[UDPReceiver] UDP client closed and port released.");
        }
    }

    void OnApplicationQuit()
    {
        ReleaseUDPResources();
    }

    void OnDestroy()
    {
        ReleaseUDPResources();
    }

    public float CalculateAverageIntensity(List<float> intensityValues)
    {
        if (intensityValues.Count == 0)
            return 0f;

        float sum = 0f;
        foreach (var intensity in intensityValues)
        {
            sum += intensity;
        }

        AverageIntensity = sum / intensityValues.Count;
        return AverageIntensity;
    }
}