using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public TMP_Text scoreText;            // 实时显示的积分 Text
    public GameObject gameOverPanel;     // 游戏结束时的全屏 Panel
    public TMP_Text gameOverText;        // 游戏结束时的得分显示 Text
    public Button continueButton;        // 继续游戏按钮（重置场景）
    public Button exitButton;            // 退出游戏按钮

    private float score = 0f;            // 当前积分

    void Start()
    {
        // 添加按钮点击事件
        continueButton.onClick.AddListener(OnContinueGame);
        exitButton.onClick.AddListener(OnExitGame);

        // 隐藏游戏结束 Panel
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    private void Update()
    {

        UpdateScore(Time.deltaTime);
    }

    // 更新积分显示
    public void UpdateScore(float increment)
    {
        score += increment;
        //Debug.Log($"[UIManager] Score updated: {score}"); // 添加日志

        if (scoreText != null)
        {
            scoreText.text = $"{score:F1} M"; // 显示为 xx.x M
            //Debug.Log("[UIManager] Score text updated.");
        }
        else
        {
            //Debug.LogWarning("[UIManager] scoreText 未设置，请检查引用。");
        }
    }

    // 显示游戏结束界面
    public void ShowGameOverMenu()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);  // 显示游戏结束面板
        }

        if (gameOverText != null)
        {
            gameOverText.text = $"{score:F1} M"; // 显示得分
        }

        // 暂停游戏时间
        Time.timeScale = 0f;
    }

    // 重置场景（继续游戏）
    private void OnContinueGame()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);  // 隐藏游戏结束面板
        }

        // 恢复时间流逝
        Time.timeScale = 1f;
        PlayerManager.Instance.InitializePos();
        PlayerManager.Instance.respawn();

        // 重置场景
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // 退出游戏
    private void OnExitGame()
    {
        Debug.Log("退出游戏");
        Application.Quit();  // 退出游戏
        // 在 Unity 编辑器中模拟退出：
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // 重置积分和 UI 状态
    public void ResetUI()
    {
        score = 0f;

        if (scoreText != null)
        {
            scoreText.text = $"{score:F1} M"; // 重置积分显示
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false); // 隐藏结束面板
        }
    }

    // 重置场景
    private void ResetScene()
    {
        Time.timeScale = 1f; // 恢复游戏时间
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // 重新加载当前场景
    }
}
