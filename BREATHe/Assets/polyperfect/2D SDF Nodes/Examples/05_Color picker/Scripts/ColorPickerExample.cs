using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Polyperfect.Examples
{
    public class ColorPickerExample : MonoBehaviour
    {
        [SerializeField] GameObject cursor;

        [SerializeField] float outerCircleRadius;
        [SerializeField] float outerCircleRadiusRange;

        [SerializeField] float offset;

        [SerializeField] float angleOffset;

        Vector3 mousePosition;
        Vector3 middle;
        Vector3 dir;

        Bounds Bounds;

        bool userClickedOutside;
        bool userClickedInside;


        [SerializeField] float h;
        [SerializeField] float s;
        [SerializeField] float v;

        [SerializeField] Color currentColor;

        [SerializeField] Material colorPickerMat;

        [SerializeField] Vector2 insideRange;

        [SerializeField] UnityEngine.UI.Image pickedColorImage;

        [SerializeField] CanvasScaler canvasScaler;


        private void Update()
        {
            var screenPoint = Input.mousePosition;
            screenPoint.z = offset;
            mousePosition = Camera.main.ScreenToWorldPoint(screenPoint);
            middle = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, offset));

            dir = mousePosition - middle;

            OutsideCursor();
            InsideCursor();

            currentColor = Color.HSVToRGB(h, s, v);
            colorPickerMat.SetColor("_CurrentColour", currentColor);
            colorPickerMat.SetFloat("_MatchWidthOrHeight", canvasScaler.matchWidthOrHeight);
            pickedColorImage.color = currentColor;
        }

        void InsideCursor()
        {

            Bounds = new Bounds(middle, insideRange);

            //Check left right
            if (Bounds.Contains(dir))
            {
                if (Input.GetMouseButtonDown(0)) userClickedInside = true;

            }

            if (userClickedInside)
            {
                var pos = Bounds.center - dir;
                pos -= Vector3.one / 2;
                pos = -pos;
                s = pos.x;
                v = pos.y;

                if (Input.GetMouseButtonUp(0)) userClickedInside = false;
            }
        }

        void OutsideCursor()
        {
            //Check to see if the mouse is within the outer radius
            if (dir.magnitude > (outerCircleRadius - outerCircleRadiusRange))
            {
                if (dir.magnitude < (outerCircleRadius + outerCircleRadiusRange))
                {
                    Debug.Log("uSER IN RANGE");

                    if (Input.GetMouseButtonDown(0)) userClickedOutside = true;
                }
            }

            if (userClickedOutside)
            {
                cursor.transform.position = dir.normalized * outerCircleRadius;

                //calculate the angle for the hue
                float angle = Vector3.Angle(dir.normalized, Vector3.up);

                if (Camera.main.WorldToViewportPoint(dir).x < 0.5)
                    angle = 360 - angle;

                h = 1 - (Mathf.Repeat(angle + angleOffset, 360)) / 360;


                if (Input.GetMouseButtonUp(0))
                {
                    userClickedOutside = false;
                }
            }
        }
    }
}
