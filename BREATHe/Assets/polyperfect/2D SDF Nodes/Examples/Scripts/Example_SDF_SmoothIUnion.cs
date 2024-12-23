using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Polyperfect.Examples
{
    public class Example_SDF_SmoothIUnion : MonoBehaviour
    {
        [SerializeField] Material _SDF_Shapes_Mat;

        [SerializeField] UnityEngine.UI.Slider _Slider;

        private void Start()
        {
            _Slider.onValueChanged.AddListener(x =>
            {
                SetMaterialValue(x);
            });

        }

        void SetMaterialValue(float value)
        {
            _SDF_Shapes_Mat.SetFloat("_Smooth_Union", value);
        }
    }
}

