using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundLogic : MonoBehaviour
{
    public Transform playerTransform;

    public float minFrequency = 0.1f;
    public float maxFrequency = 2f;

    private Color colorMin = new Color(0.569f, 0.627f, 0.749f); // 91A1BF
    private Color colorMax = new Color(0.231f, 0.251f, 0.298f); // 3B404C

    private Camera mainCamera;
    
    void Start()
    {
        mainCamera = Camera.main;
        gameObject.transform.position = playerTransform.position;
    }

    void Update()
    {
        gameObject.transform.position = playerTransform.position;

        float frequency = UDPReceiver.Instance.Frequency;

        float t = Mathf.InverseLerp(minFrequency, maxFrequency, frequency);
        Color color = Color.Lerp(colorMin, colorMax, t);

        mainCamera.backgroundColor = color;
    }
}
