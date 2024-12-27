using UnityEngine;

public class TutorialArea : MonoBehaviour
{
    public bool hasEndedTutorial = false;
    private bool isCalibrating = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("[TutorialArea] 进入校准区域，开始校准");
            
            var playerManager = other.GetComponent<PlayerManager>();
            if (playerManager != null)
            {
                playerManager.StartCalibration();
                isCalibrating = true;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && isCalibrating)
        {
            Debug.Log("[TutorialArea] 离开校准区域，结束校准");
            
            var playerManager = other.GetComponent<PlayerManager>();
            if (playerManager != null)
            {
                playerManager.EndCalibration();
                hasEndedTutorial = true;
                isCalibrating = false;
            }
        }
    }
}

