using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriangleRotate : MonoBehaviour
{
    public float rotatespeed = 180f;
    public float rotateTime = 0.5f;
    public float pauseTime = 1f;

    private bool isRotating = true;
    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;

        if (isRotating)
        {
            float step = rotatespeed * Time.deltaTime;
            transform.Rotate(Vector3.forward, step);

            if (timer >= rotateTime)
            {
                isRotating = false;
                timer = 0f;
            }
        }
        else
        {
            if (timer >= pauseTime)
            {
                isRotating = true;
                timer = 0f;
            }
        }
    }
}
