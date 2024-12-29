using UnityEngine;

public class TutorialArea : MonoBehaviour
{
    public bool hasEndedTutorial = false;
    private float calibrationDuration = 10f;  // 校准持续时间
    private float calibrationTimer = 0f;      // 校准计时器
    private bool isCalibrating = false;
    private PlayerManager playerManager;

    private void Start()
    {
        // 获取玩家管理器
        playerManager = FindObjectOfType<PlayerManager>();
        if (playerManager == null)
        {
            Debug.LogError("[TutorialArea] PlayerManager not found!");
            return;
        }

        // 开始校准
        StartCalibration();
    }

    private void Update()
    {
        if (isCalibrating)
        {
            calibrationTimer += Time.deltaTime;

            // 显示剩余时间
            float remainingTime = calibrationDuration - calibrationTimer;
            Debug.Log($"[TutorialArea] 校准剩余时间: {remainingTime:F1}秒");

            // 检查是否达到校准时间
            if (calibrationTimer >= calibrationDuration)
            {
                EndCalibration();
            }
        }
    }

    private void StartCalibration()
    {
        if (playerManager != null)
        {
            Debug.Log("[TutorialArea] 开始校准阶段");
            playerManager.StartCalibration();
            isCalibrating = true;
            calibrationTimer = 0f;
        }
    }

    private void EndCalibration()
    {
        if (playerManager != null && isCalibrating)
        {
            Debug.Log("[TutorialArea] 校准阶段结束");
            playerManager.EndCalibration();
            hasEndedTutorial = true;
            isCalibrating = false;
        }
    }

    // 如果需要手动重置校准
    public void ResetCalibration()
    {
        isCalibrating = false;
        calibrationTimer = 0f;
        hasEndedTutorial = false;
    }
}

