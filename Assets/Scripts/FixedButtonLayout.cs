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

            ParametersSet?.Invoke();
        }
    }

}
