using UnityEngine;
using Febucci.UI;

public class WaveEffectController : MonoBehaviour
{
    public TextAnimatorPlayer textAnimatorPlayer;
    public TextAnimator textAnimator;
    public PlayerManager playerManager;
    
    [Header("Wave Effect Settings")]
    [SerializeField] private float baseAmplitude = 0.5f;    
    [SerializeField] private float baseFrequency = 0.4f;    
    [SerializeField] private float maxAmplitude = 2f;       

    [Header("Intensity Settings")]
    [SerializeField] private float minIntensity = 5f;     // 对应 minBreathForce
    [SerializeField] private float maxIntensity = 15f;    // 对应 maxBreathForce
    
    private void Start()
    {
        if (textAnimatorPlayer == null)
        {
            textAnimatorPlayer = GetComponent<TextAnimatorPlayer>();
        }

        if (textAnimator == null)
        {
            textAnimator = GetComponent<TextAnimator>();
        }

        if (playerManager == null)
        {
            playerManager = FindObjectOfType<PlayerManager>();
        }

        // 只在开始时显示一次带波浪效果的文本
        string waveText = $"<wave a={baseAmplitude:F2} f={baseFrequency:F2}>{textAnimator.tmproText.text}</wave>";
        textAnimatorPlayer.ShowText(waveText);
    }

    private void Update()
    {
        if (UDPReceiver.Instance != null && textAnimator != null && !playerManager.isCalibrating)
        {
            // 使用校准后的范围来映射强度，并确保在 TextAnimator 的有效范围内
            float normalizedIntensity = Mathf.InverseLerp(
                playerManager.RecordedMinIntensity, 
                playerManager.RecordedMaxIntensity, 
                UDPReceiver.Instance.Intensity
            );
            
            // 直接映射到 TextAnimator 的有效范围
            float finalIntensity = Mathf.Lerp(minIntensity, maxIntensity, normalizedIntensity);
            
            textAnimator.effectIntensityMultiplier = finalIntensity;

            // 为了让效果更明显，我们也可以同时调整波浪的振幅
            float mappedAmplitude = Mathf.Lerp(baseAmplitude, maxAmplitude, normalizedIntensity);
            string waveText = $"<wave a={mappedAmplitude:F2} f={baseFrequency:F2}>{textAnimator.tmproText.text}</wave>";
            textAnimatorPlayer.ShowText(waveText);
        }
    }
}
