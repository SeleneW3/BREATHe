using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Polyperfect.Examples
{
    public class Example_SDF_Shapes : MonoBehaviour
    {
        [SerializeField] Material _SDF_Shapes_Mat;

        [SerializeField] TMPro.TMP_Dropdown _TMP_Dropdown;

        [SerializeField] List<string> _shaderKeywords;

        private void Awake()
        {
            _TMP_Dropdown.options.Clear();
            _TMP_Dropdown.AddOptions(_shaderKeywords);
        }

        private void Start()
        {
            _TMP_Dropdown.onValueChanged.AddListener(x =>
            {
                SwapShaderKeyword(_TMP_Dropdown.options[_TMP_Dropdown.value].text);
                _TMP_Dropdown.RefreshShownValue();
            });

        }

        void SwapShaderKeyword(string keyWord)
        {
            CheckShaderKeywordState(keyWord);
        }

        void CheckShaderKeywordState(string keywordName)
        {
            // Get the instance of the Shader class that the material uses
            var shader = _SDF_Shapes_Mat.shader;

            // Get all the local keywords that affect the Shader
            var keywordSpace = shader.keywordSpace;

            foreach (var item in _shaderKeywords)
            {
                foreach (var localKeyword in keywordSpace.keywords)
                {
                    if(item == localKeyword.name)
                    {
                        if(item == keywordName)
                        {
                            _SDF_Shapes_Mat.SetKeyword(localKeyword, true);
                        }
                        else
                        {
                            _SDF_Shapes_Mat.SetKeyword(localKeyword, false);
                        }
                    }
                }
            }
        }
    }
}

