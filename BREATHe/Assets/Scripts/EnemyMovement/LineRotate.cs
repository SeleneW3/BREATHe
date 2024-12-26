using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineRotate : MonoBehaviour
{
    public Vector3 rotationAxis = new Vector3(1, 0, 1);
    public float rotationSpeed = 30f; // 旋转速度，单位为度/秒

    void Update()
    {
        // 绕着自身的中心轴旋转
        transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime);
    }
}
