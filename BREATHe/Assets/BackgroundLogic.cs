using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundLogic : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform playerTransform;
    void Start()
    {
        gameObject.transform.position = playerTransform.position;
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.position = playerTransform.position;
    }
}
