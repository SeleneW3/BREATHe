using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquareMove : MonoBehaviour
{
    public float moveSpeed = 2.0f;
    public float moveRange = 12f;
    private float startY;

    void Start()
    {
        startY = transform.position.y;
    }
    void Update()
    {
        // ʹ�� Mathf.PingPong �����Ƶ��˵������ƶ���ȷ������ 10 �� -10 ֮��
        // Mathf.PingPong �᷵��һ���� 0 �� moveRange ��ֵ��ͨ����ȥ moveRange/2 ��ƽ�ƣ�ʹ�ý���Ǵ� 10 �� -10
        float newY = Mathf.PingPong(Time.time * moveSpeed, moveRange) - (moveRange / 2);

        transform.position = new Vector3(transform.position.x, startY + newY, transform.position.z);
    }
}