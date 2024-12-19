using UnityEngine;

public class TutorialArea : MonoBehaviour
{
    private bool hasEndedTutorial = false; // ���һ����־λ

    // ����ҽ����������ʱ����
    private void OnTriggerEnter2D(Collider2D other)
    {
        // ������Ķ����Ƿ������
        if (other.CompareTag("Player"))
        {
            // �����־����ʾ��ҽ�����������������
            Debug.Log("[TutorialArea] ����ѽ��������������򣬽�������������");

            // ���� UDPReceiver �е� EndTutorial ������������������
            UDPReceiver.Instance.EndTutorial();

            // ���ñ�־λ��ȷ��ֻ����һ��
            hasEndedTutorial = true;
        }
    }
}

