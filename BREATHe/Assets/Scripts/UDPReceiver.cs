using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using UnityEngine.SceneManagement;
using UnityEngine;

public class UDPReceiver : MonosingletonTemp<UDPReceiver>
{
    // Singleton pattern

    public int port = 65432; // Default port
    private UdpClient udpClient;
    private IPEndPoint remoteEndPoint;

    public Rigidbody2D playerRigidbody;   // Player's Rigidbody2D component
    public GameObject player;
    public float baseJumpForce = 15.0f;    // Base jump force
    private float adjustedJumpForce;       // Dynamically adjusted jump force
    private float gravityScale = 1.0f;     // Gravity scale
    private float lastTimeScale = 5.0f;    // Time scale
    private bool timeScaleUpdated = false;
    private float recoveryRate = 0.1f;     // Recovery rate

    private UIManager uiManager;
    // 引入 PlayerAnimationEffect 脚本
    public PlayerAnimationEffect playerAnimationEffect;  // 在脚本中引用呼吸灯效果

    // Tutorial stage parameters
    [SerializeField]private bool isTutorialStage = false;  // Default not in tutorial stage

    // Dynamic threshold values
    private float dynamicMinThreshold = 0.005f; // Minimum threshold for breath intensity
    private float dynamicMaxThreshold = 0.02f;  // Maximum threshold for breath intensity

    void Start()
    {
        InitializeUDP();

        uiManager = FindObjectOfType<UIManager>();
        //playerRigidbody = player.gameObject.GetComponent<Rigidbody2D>();

    }

    void Update()
    {
        playerRigidbody = PlayerManager.Instance.GetComponent<Rigidbody2D>();

        timeScaleUpdated = false;

        if (udpClient == null)
        {
            Debug.LogWarning("[UDPReceiver] udpClient not initialized, attempting to reinitialize...");
            InitializeUDP();
            return;
        }

        try 
        {
            // Debug.Log($"[UDPReceiver] Checking UDP availability: {udpClient.Available} bytes");
            
            if (udpClient.Available > 0)
            {
                Debug.Log("[UDPReceiver] Data available, processing...");
                try
                {
                    byte[] data = udpClient.Receive(ref remoteEndPoint);
                    string jsonData = Encoding.UTF8.GetString(data);
                    Debug.Log($"[UDPReceiver] Received data: {jsonData}");

                    var parsedData = JsonUtility.FromJson<BreathData>(jsonData);
                    Debug.Log($"[UDPReceiver] Parsed data -> time_scale: {parsedData.time_scale}, intensity: {parsedData.intensity}");

                    lastTimeScale = parsedData.time_scale;
                    timeScaleUpdated = true;

                    if (isTutorialStage)
                    {
                        // Dynamically adjust thresholds during tutorial
                        AdjustThresholdsDuringTutorial(parsedData);
                    }
                    else
                    {
                        // Regular gameplay, adjust jump and gravity
                        AdjustJumpAndGravity(lastTimeScale);
                        PerformJump();
                    }
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

        if (!timeScaleUpdated)
        {
            lastTimeScale = Mathf.MoveTowards(lastTimeScale, 5.0f, recoveryRate * Time.deltaTime);
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

    private void AdjustThresholdsDuringTutorial(BreathData parsedData)
    {
        // Dynamically adjust thresholds during tutorial
        // Based on player's breath intensity, adjust dynamic thresholds
        dynamicMinThreshold = Mathf.Lerp(0.005f, 0.02f, Mathf.Clamp01(parsedData.intensity));
        dynamicMaxThreshold = Mathf.Lerp(0.02f, 0.05f, Mathf.Clamp01(parsedData.intensity));  // Example range

        Debug.Log($"[UDPReceiver] Current dynamic thresholds: minThreshold={dynamicMinThreshold}, maxThreshold={dynamicMaxThreshold}");

        // Send updated thresholds to Python
        SendThresholdsToPython(dynamicMinThreshold, dynamicMaxThreshold);
    }

    private void AdjustJumpAndGravity(float timeScale)
    {
        timeScale = Mathf.Clamp(timeScale, 0.1f, 5.0f); // Clamp time scale to valid range

        // Adjust jump force and gravity based on time scale
        adjustedJumpForce = Mathf.Lerp(5f, 15f, (timeScale - 0.1f) / (5.0f - 0.1f));
        gravityScale = Mathf.Lerp(0.5f, 2.0f, (timeScale - 0.1f) / (5.0f - 0.1f));

        if (playerRigidbody != null)
        {
            playerRigidbody.gravityScale = gravityScale;
            Debug.Log($"[UDPReceiver] Adjusted Jump Force: {adjustedJumpForce}, Gravity Scale: {gravityScale}");
        }
        else
        {
            Debug.LogWarning("[UDPReceiver] playerRigidbody not found, cannot adjust jump and gravity!");
        }
    }

    private void PerformJump()
    {
        if (playerRigidbody != null)
        {
            playerRigidbody.velocity = new Vector2(playerRigidbody.velocity.x, adjustedJumpForce);
            Debug.Log("[UDPReceiver] Player Jump Triggered!");

            // 调用呼吸灯效果
            if (playerAnimationEffect != null)
            {

            }
        }
        else
        {
            Debug.LogWarning("[UDPReceiver] playerRigidbody not found, cannot perform jump!");
        }
    }

    // End tutorial mode
    public void EndTutorial()
    {
        isTutorialStage = false;
        Debug.Log("[UDPReceiver] Tutorial ended, starting formal game.");
    }

    // Start tutorial mode
    public void StartTutorial()
    {
        isTutorialStage = true;
        Debug.Log("[UDPReceiver] Entered tutorial stage.");
    }

    // Send updated thresholds to Python
    private void SendThresholdsToPython(float minThreshold, float maxThreshold)
    {
        if (udpClient == null)
        {
            Debug.LogError("[UDPReceiver] udpClient not initialized, cannot send data!");
            return;
        }

        string thresholdData = $"{{\"min_threshold\": {minThreshold}, \"max_threshold\": {maxThreshold}}}";
        byte[] dataToSend = Encoding.UTF8.GetBytes(thresholdData);
        udpClient.Send(dataToSend, dataToSend.Length, remoteEndPoint);

        Debug.Log($"[UDPReceiver] Sent thresholds to Python: minThreshold={minThreshold}, maxThreshold={maxThreshold}");
    }



    // Release UDP resources
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

    [System.Serializable]
    public class BreathData
    {
        public float time;
        public float intensity;
        public float time_scale;
    }
}