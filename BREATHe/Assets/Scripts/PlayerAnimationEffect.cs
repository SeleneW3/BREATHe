using UnityEngine;
using System.Collections;

public class PlayerAnimationEffect : MonoBehaviour
{
    private Renderer playerRenderer; // ��ɫ��Renderer
    private Material playerMaterial; // ��ɫ��Material

    // ���Ʒ������ɫ
    public Color glowColor = Color.white;
    public float glowDuration = 0.5f;
    public float glowIntensity = 5f;

    private void Start()
    {
        playerRenderer = GetComponent<Renderer>();
        if (playerRenderer != null)
        {
            playerMaterial = playerRenderer.material;
        }
        else
        {
            Debug.LogError("Player has no Renderer component.");
        }
    }

    

    
}
