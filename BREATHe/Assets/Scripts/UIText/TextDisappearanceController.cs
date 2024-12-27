using UnityEngine;
using Febucci.UI;
using System.Collections;

public class TextDisappearanceController : MonoBehaviour
{
    //Inside your script 
    public TextAnimator textAnimator; 
    //Manage the event subscription 
    private void Awake() 
    { 
        textAnimator.onEvent += OnEvent; 
    } 
    private void OnDestroy() 
    { 
        textAnimator.onEvent -= OnEvent; 
    } 
    //Do things based on messages 
    void OnEvent(string message) 
    { 
        switch (message) 
        { 
            case "customInput": 
            StartCoroutine(WaitForBreathing()); // 等待检测到呼吸
            break; 
        } 
    }

    // 自定义逻辑：等待检测到呼吸
    private IEnumerator WaitForBreathing()
    {
        Debug.Log("Waiting for breathing...");
        // 假设通过禁用组件来暂停动画
        textAnimator.enabled = false;

        while (!UDPReceiver.Instance.IsBreathing)
        {
            yield return null;
        }

        Debug.Log("Breathing detected!");
        // 恢复动画
        textAnimator.enabled = true;
    }
}