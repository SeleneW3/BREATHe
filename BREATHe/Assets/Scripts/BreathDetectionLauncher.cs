using UnityEngine;
using System.IO;
using System.Diagnostics;

public class BreathDetectionLauncher : MonoBehaviour
{
    private Process breathDetectionProcess;

    void Start()
    {
        LaunchBreathDetection();
    }

    void LaunchBreathDetection()
    {
        string exePath = Path.Combine(Application.streamingAssetsPath, "BreathDetection.exe");
        if (File.Exists(exePath))
        {
            breathDetectionProcess = new Process();
            breathDetectionProcess.StartInfo.FileName = exePath;
            breathDetectionProcess.Start();
        }
        else
        {
            UnityEngine.Debug.LogError("找不到呼吸检测程序！");
        }
    }

    void OnApplicationQuit()
    {
        if (breathDetectionProcess != null && !breathDetectionProcess.HasExited)
        {
            breathDetectionProcess.Kill();
        }
    }
} 