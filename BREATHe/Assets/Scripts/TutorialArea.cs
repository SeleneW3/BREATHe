using UnityEngine;

public class TutorialArea : MonoBehaviour
{
    public bool hasEndedTutorial = false; // ����һ����־λ

    // ����ҽ����������ʱ����
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("TutorialArea: OnTriggerEnter2D");
        Debug.Log("TutorialArea: other.CompareTag(Player): " + other.CompareTag("Player"));
        // ������Ķ����Ƿ������
        if (other.CompareTag("Player"))
        {
            // �����־����ʾ��ҽ�����������������
            Debug.Log("[TutorialArea] EXIT TUTORIAL");

            // ���� UDPReceiver �е� EndTutorial ������������������
            UDPReceiver.Instance.EndTutorial();

            // ���ñ�־λ��ȷ��ֻ����һ��
            hasEndedTutorial = true;
        }
    }
}

