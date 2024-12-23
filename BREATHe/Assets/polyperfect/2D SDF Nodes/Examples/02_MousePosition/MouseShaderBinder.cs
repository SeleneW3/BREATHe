using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class MouseShaderBinder : MonoBehaviour
{
    [SerializeField] Material material;

    private void Update()
    {
        if (material == null) return;
        material.SetVector("_MousePosition", Camera.main.ScreenToViewportPoint(Input.mousePosition));
    }
}
