using UnityEngine;
using Febucci.UI;

public class WaveEffectController : MonoBehaviour
{
    public TextAnimatorPlayer textAnimatorPlayer;
    public TextAnimator textAnimator;
    
    [Header("Wave Effect Settings")]
    [SerializeField] private float baseAmplitude = 0.5f;    
    [SerializeField] private float baseFrequency = 0.4f;    
    [SerializeField] private float maxAmplitude = 2f;       

    private string originalText;  // 存储原始文本
    
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

        // 保存原始文本
        if (textAnimator != null)
        {
            originalText = textAnimator.tmproText.text;
            // 初始显示带波浪效果的文本
            UpdateWaveEffect(baseAmplitude);
        }
    }

    private void Update()
    {
        if (UDPReceiver.Instance != null && textAnimator != null)
        {
            // 将呼吸强度映射到振幅范围
            float mappedAmplitude = Mathf.Lerp(baseAmplitude, maxAmplitude, UDPReceiver.Instance.Intensity);
            
            // 更新波浪效果
            UpdateWaveEffect(mappedAmplitude);
        }
    }

    private void UpdateWaveEffect(float amplitude)
    {
        if (string.IsNullOrEmpty(originalText)) return;

        // 使用原始文本更新波浪效果
        string waveText = $"<wave a={amplitude:F2} f={baseFrequency:F2}>{originalText}</wave>";
        textAnimatorPlayer.ShowText(waveText);
    }
}
