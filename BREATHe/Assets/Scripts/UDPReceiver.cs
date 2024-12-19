using System.Net.Sockets;
using System.Net;
using System.Text;
using UnityEngine.SceneManagement;
using UnityEngine;

public class UDPReceiver : MonoBehaviour
{
    public static UDPReceiver Instance { get; private set; } // ����ģʽ

    public int port = 65432; // Ĭ�϶˿�
    private UdpClient udpClient;
    private IPEndPoint remoteEndPoint;

    public Rigidbody2D playerRigidbody;   // 2D ��ɫ�� Rigidbody2D ���
    public float baseJumpForce = 15.0f;   // ������Ծ����
    private float adjustedJumpForce;      // ��̬���������Ծ����
    private float gravityScale = 1.0f;    // ��������
    private float lastTimeScale = 5.0f;   // ʱ������
    private bool timeScaleUpdated = false;
    private float recoveryRate = 0.1f;  // �ָ�����

    private UIManager uiManager;

    // ���������׶β���ֵ
    private bool isTutorialStage = false;  // Ĭ�ϲ������������׶�

    // ��̬��Χ��ֵ
    private float dynamicMinThreshold = 0.005f; // ����ǿ�ȵ���С��ֵ
    private float dynamicMaxThreshold = 0.02f;  // ����ǿ�ȵ������ֵ

    void Start()
    {
        InitializeUDP();
        SceneManager.sceneLoaded += OnSceneLoaded;

        uiManager = FindObjectOfType<UIManager>();
        TryFindPlayerRigidbody();

        // �Զ��������������׶�
        //StartTutorial();
    }

    void Update()
    {
        timeScaleUpdated = false;

        if (udpClient == null)
        {
            Debug.LogWarning("[UDPReceiver] udpClient δ��ʼ����");
            return;
        }

        if (udpClient.Available > 0)
        {
            Debug.Log("[UDPReceiver] ���ݿ��ã����Խ�������...");
            try
            {
                byte[] data = udpClient.Receive(ref remoteEndPoint);
                string jsonData = Encoding.UTF8.GetString(data);
                Debug.Log($"[UDPReceiver] ���յ�����: {jsonData}");

                var parsedData = JsonUtility.FromJson<BreathData>(jsonData);
                Debug.Log($"[UDPReceiver] ������� -> time_scale: {parsedData.time_scale}, intensity: {parsedData.intensity}");

                lastTimeScale = parsedData.time_scale;
                timeScaleUpdated = true;

                if (isTutorialStage)
                {
                    // ����������������׶Σ���̬������ֵ
                    AdjustThresholdsDuringTutorial(parsedData);
                }
                else
                {
                    // ���򣬼���������������
                    AdjustJumpAndGravity(lastTimeScale);
                    PerformJump();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[UDPReceiver] ���ݽ��ջ��������: {e.Message}");
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
            remoteEndPoint = new IPEndPoint(IPAddress.Any, port); // ���������κ� IP ������
            udpClient.EnableBroadcast = true;  // �����Ҫ�㲥���ܣ��������ù㲥
            udpClient.Client.ReceiveTimeout = 100;  // ���ý��ճ�ʱ�����룩
            Debug.Log($"[UDPReceiver] UDP �ͻ��˳�ʼ���ɹ����˿ں�: {port}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[UDPReceiver] ��ʼ�� UDP �ͻ���ʱ����: {e.Message}");
        }
    }

    private void AdjustThresholdsDuringTutorial(BreathData parsedData)
    {
        // ���������׶ζ�̬��������ǿ�ȵ���ֵ
        // ������ҵĺ���ǿ����������̬��ֵ��ֱ��������������
        dynamicMinThreshold = Mathf.Lerp(0.005f, 0.02f, Mathf.Clamp01(parsedData.intensity));
        dynamicMaxThreshold = Mathf.Lerp(0.02f, 0.05f, Mathf.Clamp01(parsedData.intensity));  // ʾ����Χ

        Debug.Log($"[UDPReceiver] ��ǰ��̬��ֵ: minThreshold={dynamicMinThreshold}, maxThreshold={dynamicMaxThreshold}");

        // �� Python ���͸��º����ֵ
        SendThresholdsToPython(dynamicMinThreshold, dynamicMaxThreshold);
    }

    private void AdjustJumpAndGravity(float timeScale)
    {
        timeScale = Mathf.Clamp(timeScale, 0.1f, 5.0f); // ����ʱ�����ӵķ�Χ

        // ����ʱ�����ӵ�����Ծ���Ⱥ���������
        adjustedJumpForce = Mathf.Lerp(5f, 15f, (timeScale - 0.1f) / (5.0f - 0.1f));
        gravityScale = Mathf.Lerp(0.5f, 2.0f, (timeScale - 0.1f) / (5.0f - 0.1f));

        if (playerRigidbody != null)
        {
            playerRigidbody.gravityScale = gravityScale;
            Debug.Log($"[UDPReceiver] Adjusted Jump Force: {adjustedJumpForce}, Gravity Scale: {gravityScale}");
        }
        else
        {
            Debug.LogWarning("[UDPReceiver] playerRigidbody δ�ҵ����޷������������ţ�");
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
            Debug.LogWarning("[UDPReceiver] playerRigidbody δ���ã��޷���Ծ��");
        }
    }

    // �������������׶�
    public void EndTutorial()
    {
        isTutorialStage = false;
        Debug.Log("[UDPReceiver] ���������ѽ�������ʼ��ʽ��Ϸ��");
    }

    // ��ʼ���������׶�
    public void StartTutorial()
    {
        isTutorialStage = true;
        Debug.Log("[UDPReceiver] �ѽ������������׶Ρ�");
    }

    // �� Python ���͸��º����ֵ
    private void SendThresholdsToPython(float minThreshold, float maxThreshold)
    {
        if (udpClient == null)
        {
            Debug.LogError("[UDPReceiver] udpClient δ��ʼ�����޷��������ݣ�");
            return;
        }

        string thresholdData = $"{{\"min_threshold\": {minThreshold}, \"max_threshold\": {maxThreshold}}}";
        byte[] dataToSend = Encoding.UTF8.GetBytes(thresholdData);
        udpClient.Send(dataToSend, dataToSend.Length, remoteEndPoint);

        Debug.Log($"[UDPReceiver] �� Python �����˸��º����ֵ: minThreshold={minThreshold}, maxThreshold={maxThreshold}");
    }

    // ������������
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryFindPlayerRigidbody();
    }

    private void TryFindPlayerRigidbody()
    {
        // ͨ����ǩ������� GameObject��Ȼ���ȡ Rigidbody2D
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerRigidbody = player.GetComponent<Rigidbody2D>();

            if (playerRigidbody != null)
            {
                Debug.Log("[UDPReceiver] Player Rigidbody ���°󶨳ɹ���");
            }
            else
            {
                Debug.LogWarning("[UDPReceiver] ��Ҷ�����ڣ���ȱ�� Rigidbody2D �����");
            }
        }
        else
        {
            Debug.LogWarning("[UDPReceiver] δ�ҵ����� 'Player' ��ǩ�� GameObject��");
        }
    }

    // �ͷ� UDP ��Դ
    public void ReleaseUDPResources()
    {
        if (udpClient != null)
        {
            udpClient.Close();
            udpClient = null;
            Debug.Log("[UDPReceiver] UDP �ͻ����ѹرգ����ͷŶ˿ڡ�");
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