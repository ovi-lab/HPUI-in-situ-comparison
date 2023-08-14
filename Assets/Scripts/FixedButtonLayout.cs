using System.Collections;
using System.Collections.Generic;
using ubc.ok.ovilab.HPUI.Core;
using UnityEngine;
using UnityEngine.Events;

namespace ubc.ok.ovilab.hpuiInSituComparison.study1
{
    public class FixedButtonLayout : MonoBehaviour
    {
        [Tooltip("The buttons in order, from top right to bottom left")]
        public List<Transform> buttons;
        public int numberOfRows = 4;
        public int numberOfColumns = 3;
        public Renderer backplate;

        [HideInInspector]
        public float Seperation { get; private set; }
        public float relativeSeperateFactor = 1;

        public event System.Action ParametersSet;
        // Offset to add to the transform after setting parameters
        [HideInInspector]
        public Vector3 offset;

        // Start is called before the first frame update
        void Start()
        {
            // SetParameters(0.075f, 4, transform.position, transform.rotation);
        }

        public void SetParameters(float seperation, float scale, FixedButtonLayout relativeFixedButtonLayout)
        {
            Vector3 topRowCenter = (relativeFixedButtonLayout.buttons[0].transform.position +
                                    relativeFixedButtonLayout.buttons[relativeFixedButtonLayout.numberOfColumns - 1].transform.position) / 2;
            Vector3 position = topRowCenter + relativeFixedButtonLayout.transform.up.normalized * relativeFixedButtonLayout.Seperation * relativeSeperateFactor;
            Quaternion rotation = relativeFixedButtonLayout.transform.rotation;
            SetParameters(seperation, scale, position, rotation);
        }

        public void SetParameters(float seperation, float scale, Vector3 position, Quaternion rotation)
        {
            transform.position = position + offset;
            transform.rotation = rotation;
            transform.localScale = Vector3.one;
            Seperation = seperation;

            float horizontalOffset = seperation * numberOfColumns/2;
            float verticalOffset = seperation * numberOfRows/2;
            if (numberOfColumns % 2 == 0)
            {
                horizontalOffset -= seperation/2;
            }

            for (int i = 0; i < numberOfRows * numberOfColumns; ++i) {
                int row = (int) Mathf.Floor(i / numberOfColumns);
                int column = i % numberOfColumns;

                Transform buttonTransform = buttons[i];
                buttonTransform.localPosition = new Vector3(horizontalOffset - seperation * column, verticalOffset - seperation * row, 0);
                buttonTransform.localScale = scale * Vector3.one;
            }

            SetBackplate();

            ParametersSet?.Invoke();
        }

        public void SetBackplate()
        {
            // FIXME: coords hard coded for now!
            Bounds topBounds = buttons[1].GetComponentInChildren<SpriteRenderer>().bounds;
            Vector3 topedge = new Vector3(0, topBounds.center.y, topBounds.center.z) + topBounds.extents.y * buttons[1].up.normalized;
            topedge.x = topBounds.center.x;

            Bounds bottomBounds = buttons[10].GetComponentInChildren<SpriteRenderer>().bounds;
            Vector3 bottomedge = new Vector3(0, bottomBounds.center.y, bottomBounds.center.z) - bottomBounds.extents.y * buttons[10].up.normalized;
            bottomedge.x = bottomBounds.center.x;

            Bounds leftBounds = buttons[0].GetComponentInChildren<SpriteRenderer>().bounds;
            Vector3 leftEdge = new Vector3(leftBounds.center.x, 0, leftBounds.center.z) + leftBounds.extents.x * buttons[0].right.normalized;
            leftEdge.y = leftBounds.center.y;

            Bounds rightBounds = buttons[2].GetComponentInChildren<SpriteRenderer>().bounds;
            Vector3 rightEdge = new Vector3(rightBounds.center.x, 0, rightBounds.center.z) - rightBounds.extents.x * buttons[2].right.normalized;
            rightEdge.y = rightBounds.center.y;

            Bounds backplateBounds = backplate.bounds;
            Vector3 backplateScale = backplate.transform.localScale;
            Vector3 newLocalScale = new Vector3(backplateScale.x * (leftEdge - rightEdge).magnitude * 1.2f / backplateBounds.size.x,
                                                backplateScale.y * (topedge - bottomedge).magnitude * 1.2f / backplateBounds.size.y,
                                                backplateScale.z);

            backplate.transform.localScale = newLocalScale;
            backplate.transform.position = topedge + (bottomedge - topedge) / 2 - Vector3.Cross(rightEdge - leftEdge, bottomedge - topedge).normalized * 0.005f;
        }
    }

}
