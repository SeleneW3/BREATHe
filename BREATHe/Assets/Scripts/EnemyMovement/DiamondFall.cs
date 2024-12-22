using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiamondFall : MonoBehaviour
{
    public float fallSpeed = 8.0f;
    private float startY;
    private bool canFall = false;
    private Camera mainCamera;
    void Start()
    {
        mainCamera = Camera.main;

        startY = transform.position.y;
    }

    void Update()
    {
        if (IsInCameraView())
        {
            canFall = true;
        }
        if (canFall)
        {
            Fall();
        }

        bool IsInCameraView()
        {
            Vector3 screenPoint = mainCamera.WorldToViewportPoint(transform.position);
            bool onScreen = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;
            return onScreen;
        }

        void Fall()
        {
            float newY = transform.position.y - fallSpeed * Time.deltaTime;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }
}
