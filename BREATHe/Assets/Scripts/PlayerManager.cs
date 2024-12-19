using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    private bool isDead = false; // ����Ƿ�����
    public static PlayerManager Instance; // ����ģʽ

    private UIManager uiManager; // ���� UIManager
    public float moveSpeed = 5f; // ��ɫ���Զ�ǰ���ٶ�

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // ���� UIManager
        uiManager = FindObjectOfType<UIManager>();
        if (uiManager == null)
        {
            Debug.LogError("UIManager δ�ҵ�����ȷ�������д��ڲ���ȷ���ã�");
        }
    }

    private void Update()
    {
        if (!isDead)
        {
            // ��ɫ�Զ�����ǰ��
            transform.Translate(Vector3.right * moveSpeed * Time.deltaTime);
        }
    }

    public void TriggerDeath()
    {
        if (isDead) return; // ��ֹ�ظ�����
        isDead = true;

        Debug.Log("������������������߼���");

        // ��ʾ��Ϸ�������棨�÷ֵ� UI��
        if (uiManager != null)
        {
            uiManager.ShowGameOverMenu();
        }
    }

    private void PauseAndResetScene(float delay)
    {
        // ��ͣ��Ϸ
        Time.timeScale = 0f;

        // �ӳٵ������ó���
        StartCoroutine(ResetSceneAfterDelay(delay));
    }

    private System.Collections.IEnumerator ResetSceneAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay); // �ȴ���ʵʱ��

        // �ָ�ʱ������
        Time.timeScale = 1f;

        // ���ó���
        ResetScene();
    }

    private void ResetScene()
    {
        Debug.Log("���ó�����");

        // ֱ��ͨ���������� UDPReceiver �� ReleaseUDPResources
        if (UDPReceiver.Instance != null)
        {
            UDPReceiver.Instance.ReleaseUDPResources();
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        ); // ���¼��ص�ǰ����
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // �����ײ�����Ƿ��ǵ�������
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log($"������� {collision.gameObject.tag}�����������߼���");
            TriggerDeath();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // ��ⴥ�������Ƿ��ǵ�������
        if (other.CompareTag("Ground") || other.CompareTag("Enemy"))
        {
            Debug.Log($"��Ҵ��� {other.gameObject.tag}�����������߼���");
            TriggerDeath();
        }
    }
}
