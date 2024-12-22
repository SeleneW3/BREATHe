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
        // 使用 Mathf.PingPong 来控制敌人的上下移动，确保它在 10 到 -10 之间
        // Mathf.PingPong 会返回一个从 0 到 moveRange 的值，通过减去 moveRange/2 来平移，使得结果是从 10 到 -10
        float newY = Mathf.PingPong(Time.time * moveSpeed, moveRange) - (moveRange / 2);

        transform.position = new Vector3(transform.position.x, startY + newY, transform.position.z);
    }
}