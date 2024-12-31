using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

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

    public TMP_Text scoreText;            // 实时显示的呼吸 Text
    public GameObject gameOverPanel;       // 游戏结束时的全屏 Panel
    public TMP_Text gameOverText;         // 游戏结束时的得分显示 Text
    public Button continueButton;         // 继续游戏按钮（重新尝试）
    public Button exitButton;             // 退出游戏按钮

    [Header("Tutorial UI")]
    public GameObject tutorialPanel;        // 新手引导面板
    public TMP_Text countdownText;          // 倒计时文本
    private float calibrationTimer = 10f;   // 校准时间，与 TutorialArea 保持一致
    private bool isCountingDown = false;    // 是否正在倒计时

    private float score = 0f;             // 当前分数

    [Header("Victory UI")]
    public TMP_Text endText;  // 结束文本

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

        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }

        // 确保结束文本开始时是隐藏的
        if (endText != null)
        {
            endText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        UpdateScore(Time.deltaTime);

        // 检测 ESC 键
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnExitGame();
        }

        // 更新倒计时
        if (isCountingDown && countdownText != null)
        {
            calibrationTimer -= Time.deltaTime;
            if (calibrationTimer <= 0)
            {
                EndCalibrationUI();
            }
            else
            {
                UpdateCountdownText();
            }
        }
    }

    // 更新呼吸显示
    public void UpdateScore(float increment)
    {
        score += increment;
        //Debug.Log($"[UIManager] Score updated: {score}"); // 调试日志

        if (scoreText != null)
        {
            scoreText.text = $"{score:F1} M"; // 显示为 xx.x M
            //Debug.Log("[UIManager] Score text updated.");
        }
        else
        {
            //Debug.LogWarning("[UIManager] scoreText 未设置，请检查设置。");
        }
    }

    // 显示游戏结束界面
    public void ShowGameOverMenu()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);  // 显示游戏结束界面
        }

        if (gameOverText != null)
        {
            gameOverText.text = $"{score:F1} M"; // 显示得分
        }

        // 暂停游戏时间
        Time.timeScale = 0f;
    }

    // 重新尝试（继续游戏）
    private void OnContinueGame()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);  // 隐藏游戏结束界面
        }

        // 恢复时间流速
        Time.timeScale = 1f;
        
        // 直接调用 respawn，而不是 InitializePos
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.respawn();
        }
    }

    // 退出游戏
    public void OnExitGame()
    {
        Debug.Log("退出游戏");
        Application.Quit();  // 退出游戏
        // 在 Unity 编辑器中模拟退出：
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // 重置呼吸和 UI 状态
    public void ResetUI()
    {
        score = 0f;

        if (scoreText != null)
        {
            scoreText.text = $"{score:F1} M"; // 重置呼吸显示
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false); // 隐藏结束面板
        }
    }

    // 重新尝试
    private void ResetScene()
    {
        Time.timeScale = 1f; // 恢复游戏时间
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // 重新加载当前场景
    }

    // 开始校准倒计时
    public void StartCalibrationUI()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(true);
        }
        calibrationTimer = 10f;
        isCountingDown = true;
        UpdateCountdownText();
    }

    // 结束校准倒计时
    private void EndCalibrationUI()
    {
        isCountingDown = false;
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }
    }

    // 更新倒计时文本
    private void UpdateCountdownText()
    {
        if (countdownText != null)
        {
            countdownText.text = $"Calibrating...\n\n\n{Mathf.CeilToInt(calibrationTimer)}";
        }
    }

    // 显示结束文本
    public void ShowEndText()
    {
        if (endText != null)
        {
            endText.gameObject.SetActive(true);
        }
    }

    // 隐藏结束文本
    public void HideEndText()
    {
        if (endText != null)
        {
            endText.gameObject.SetActive(false);
        }
    }
}
