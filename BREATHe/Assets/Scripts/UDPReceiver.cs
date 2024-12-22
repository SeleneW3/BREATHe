using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections.Generic;

public class UDPReceiver : MonosingletonTemp<UDPReceiver>
{

    public int port = 65432; // Default port
    public float averageIntensity = 0f;
    public float Intensity = 0f;

    private UdpClient udpClient;
    private IPEndPoint remoteEndPoint;

    private float lastTimeScale = 5.0f;    // Time scale
    private bool timeScaleUpdated = false;
    private float recoveryRate = 0.1f;     // Recovery rate

    private List<float> intensityValues = new List<float>();
    public bool isMeasuring = false;
    


    void Start()
    {
        InitializeUDP();
    }

    void Update()
    {

        timeScaleUpdated = false;



        if (udpClient == null)
        {
            Debug.LogWarning("[UDPReceiver] udpClient not initialized, attempting to reinitialize...");
            InitializeUDP();
            return;
        }

        try 
        {
            
            if (udpClient.Available > 0)
            {
                try
                {
                    byte[] data = udpClient.Receive(ref remoteEndPoint);
                    string jsonData = Encoding.UTF8.GetString(data);

                    var parsedData = JsonUtility.FromJson<BreathData>(jsonData);
                    Debug.LogWarning($"[UDPReceiver] Parsed data -> intensity: {parsedData.intensity}");
                    Intensity = parsedData.intensity;
                    

                    lastTimeScale = parsedData.time_scale;
                    timeScaleUpdated = true;

                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[UDPReceiver] Error processing data: {e.Message}");
                }
            }
        }
        catch (SocketException e)
        {
            Debug.LogError($"[UDPReceiver] Socket error: {e.Message}. Attempting to reinitialize UDP...");
            ReleaseUDPResources();
            InitializeUDP();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[UDPReceiver] Unexpected error: {e.Message}");
        }

        if (isMeasuring && Intensity > 0)
        {
            intensityValues.Add(Intensity);
        }
        else
        {
            isMeasuring = false;
            averageIntensity = CalculateAverageIntensity(intensityValues);
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

        return sum / intensityValues.Count;
    }

    [System.Serializable]
    public class BreathData
    {
        public float time;
        public float intensity;
        public float time_scale;
    }
}