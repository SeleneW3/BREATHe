using UnityEngine;

public class AttractionTrigger : MonoBehaviour
{
    public HoleBreath holeBreath;

    private void Start()
    {
        if (holeBreath == null)
        {
            holeBreath = transform.parent?.GetComponent<HoleBreath>();
            
            if (holeBreath == null)
            {
                Debug.LogError($"[AttractionTrigger] {gameObject.name} 未设置 HoleBreath 组件！请手动设置或确保父物体有 HoleBreath 组件");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"AttractionTrigger: {other.name} 进入触发区");
        if (other.CompareTag("Player") && holeBreath != null)
        {
            holeBreath.OnPlayerEnterRange(other.transform);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && holeBreath != null)
        {
            holeBreath.OnPlayerExitRange();
        }
    }
} 