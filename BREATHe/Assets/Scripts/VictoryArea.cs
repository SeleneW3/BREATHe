using UnityEngine;
using System.Collections;

public class VictoryArea : MonoBehaviour
{
    public ParticleSystem victoryParticles;  // 胜利粒子效果
    private bool hasWon = false;  // 防止重复触发

    private void Start()
    {
        // 确保粒子系统开始时是停止的
        if (victoryParticles != null)
        {
            victoryParticles.Stop();
            victoryParticles.Clear();  // 清除任何可能的粒子
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!hasWon && collision.CompareTag("Player"))
        {
            hasWon = true;
            StartCoroutine(VictorySequence());
        }
    }

    private IEnumerator VictorySequence()
    {
        // 停止玩家移动
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.enabled = false;
            PlayerManager.Instance.rb.velocity = Vector2.zero;
        }

        // 播放粒子效果
        if (victoryParticles != null && !victoryParticles.isPlaying)
        {
            victoryParticles.Play();
        }

        // 等待2秒后显示 End 文本
        yield return new WaitForSeconds(3f);
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowEndText();
        }

        // 再等待3秒
        yield return new WaitForSeconds(7f);

        // 退出游戏
        if (UIManager.Instance != null)
        {
            UIManager.Instance.OnExitGame();
        }
        else
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
} 