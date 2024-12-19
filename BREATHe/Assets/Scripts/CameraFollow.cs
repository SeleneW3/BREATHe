using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;  // ��ɫ�� Transform
    public float offsetY = 0f;  // ��������ɫ�� Y ���ƫ�����������������ˮƽλ�����룩
    public float offsetZ = -10f;  // ��������ɫ�� Z ���ƫ����
    public float smoothSpeed = 0.125f;  // �����ƽ���ƶ����ٶ�

    private Vector3 velocity = Vector3.zero;  // ���ڴ洢��ǰ��������ƶ��ٶ�

    private void LateUpdate()
    {
        if (player != null)
        {
            // ��������������λ��
            Vector3 desiredPosition = new Vector3(player.position.x, offsetY, offsetZ);

            // ʹ��ƽ���ķ�ʽ�ƶ������
            // ʹ�� SmoothDamp ������ Lerp�������ƽ����
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothSpeed);
        }
    }
}
