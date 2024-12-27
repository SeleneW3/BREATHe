using UnityEngine;
using Febucci.UI;

public class WaveEffectController : MonoBehaviour
{
    public TextAnimatorPlayer textAnimatorPlayer;
    private float baseAmplitude = 0.5f;    // 基础振幅
    private float baseFrequency = 0.4f;    // 基础频率
    private float maxAmplitude = 2f;       // 最大振幅
    
    private void Start()
    {
        if (textAnimatorPlayer == null)
        {
            textAnimatorPlayer = GetComponent<TextAnimatorPlayer>();
        }
    }

    private void Update()
    {
        if (UDPReceiver.Instance != null)
        {
            // 将呼吸强度映射到振幅范围
            float mappedAmplitude = Mathf.Lerp(baseAmplitude, maxAmplitude, UDPReceiver.Instance.Intensity);
            
            // 使用新的振幅值更新文本
            string waveText = $"<wave a={mappedAmplitude:F2} f={baseFrequency:F2}>{textAnimatorPlayer.textAnimator.tmproText.text}</wave>";
            textAnimatorPlayer.ShowText(waveText);
        }
    }
}
