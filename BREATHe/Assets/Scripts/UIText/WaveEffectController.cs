using UnityEngine;
using Febucci.UI;

public class WaveEffectController : MonoBehaviour
{
    public TextAnimatorPlayer textAnimatorPlayer;
    public TextAnimator textAnimator;  // 添加 TextAnimator 引用
    
    [Header("Wave Effect Settings")]
    [SerializeField] private float baseAmplitude = 0.5f;    // 基础振幅
    [SerializeField] private float baseFrequency = 0.4f;    // 基础频率
    [SerializeField] private float maxAmplitude = 2f;       // 最大振幅

    [Header("Intensity Multiplier Settings")]
    [SerializeField] private float minIntensityMultiplier = 0.5f;  // 最小效果强度
    [SerializeField] private float maxIntensityMultiplier = 3.5f;  // 最大效果强度
    
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
    }

    private void Update()
    {
        if (UDPReceiver.Instance != null && textAnimator != null)
        {
            // 将呼吸强度映射到振幅范围
            float mappedAmplitude = Mathf.Lerp(baseAmplitude, maxAmplitude, UDPReceiver.Instance.Intensity);
            
            // 将呼吸强度映射到效果强度范围
            float mappedIntensity = Mathf.Lerp(minIntensityMultiplier, maxIntensityMultiplier, UDPReceiver.Instance.Intensity);
            
            // 更新效果强度
            textAnimator.effectIntensityMultiplier = mappedIntensity;

            // 更新波浪效果
            string waveText = $"<wave a={mappedAmplitude:F2} f={baseFrequency:F2}>{textAnimator.tmproText.text}</wave>";
            textAnimatorPlayer.ShowText(waveText);
        }
    }
}
