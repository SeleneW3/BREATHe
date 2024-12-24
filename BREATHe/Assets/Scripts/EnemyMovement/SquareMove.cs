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
        float newY = Mathf.PingPong(Time.time * moveSpeed, moveRange) - (moveRange / 2);

        transform.position = new Vector3(transform.position.x, startY + newY, transform.position.z);
    }
}