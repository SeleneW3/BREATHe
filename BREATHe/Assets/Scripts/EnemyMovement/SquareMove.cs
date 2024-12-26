using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquareMove : MonoBehaviour
{
    public float moveSpeed = 2.0f;
    public float moveRange = 12f;
    public float startDelay = 0f;
    private float startY;
    private float startTime;

    void Start()
    {
        startY = transform.position.y;
        startTime = Time.time;
    }
    void Update()
    {
        if (Time.time - startTime > startDelay)
        {
            float newY = Mathf.PingPong(Time.time * moveSpeed, moveRange) - (moveRange / 2);
            transform.position = new Vector3(transform.position.x, startY + newY, transform.position.z);
        }
    }
}