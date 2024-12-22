using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public TMP_Text scoreText;            // ʵʱ��ʾ�Ļ��� Text
    public GameObject gameOverPanel;     // ��Ϸ����ʱ��ȫ�� Panel
    public TMP_Text gameOverText;        // ��Ϸ����ʱ�ĵ÷���ʾ Text
    public Button continueButton;        // ������Ϸ��ť�����ó�����
    public Button exitButton;            // �˳���Ϸ��ť

    private float score = 0f;            // ��ǰ����

    void Start()
    {
        // ��Ӱ�ť����¼�
        continueButton.onClick.AddListener(OnContinueGame);
        exitButton.onClick.AddListener(OnExitGame);

        // ������Ϸ���� Panel
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    private void Update()
    {

        UpdateScore(Time.deltaTime);
    }

    // ���»�����ʾ
    public void UpdateScore(float increment)
    {
        score += increment;
        //Debug.Log($"[UIManager] Score updated: {score}"); // �����־

        if (scoreText != null)
        {
            scoreText.text = $"{score:F1} M"; // ��ʾΪ xx.x M
            //Debug.Log("[UIManager] Score text updated.");
        }
        else
        {
            //Debug.LogWarning("[UIManager] scoreText δ���ã��������á�");
        }
    }

    // ��ʾ��Ϸ��������
    public void ShowGameOverMenu()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);  // ��ʾ��Ϸ�������
        }

        if (gameOverText != null)
        {
            gameOverText.text = $"{score:F1} M"; // ��ʾ�÷�
        }

        // ��ͣ��Ϸʱ��
        Time.timeScale = 0f;
    }

    // ���ó�����������Ϸ��
    private void OnContinueGame()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);  // ������Ϸ�������
        }

        // �ָ�ʱ������
        Time.timeScale = 1f;
        PlayerManager.Instance.InitializePos();
        PlayerManager.Instance.respawn();

        // ���ó���
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // �˳���Ϸ
    private void OnExitGame()
    {
        Debug.Log("�˳���Ϸ");
        Application.Quit();  // �˳���Ϸ
        // �� Unity �༭����ģ���˳���
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // ���û��ֺ� UI ״̬
    public void ResetUI()
    {
        score = 0f;

        if (scoreText != null)
        {
            scoreText.text = $"{score:F1} M"; // ���û�����ʾ
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false); // ���ؽ������
        }
    }

    // ���ó���
    private void ResetScene()
    {
        Time.timeScale = 1f; // �ָ���Ϸʱ��
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // ���¼��ص�ǰ����
    }
}
