using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ubc.ok.ovilab.HPUI.Core.DeformableSurfaceDisplay;
using ubc.ok.ovilab.HPUI.Core;

namespace ubc.ok.ovilab.hpuiInSituComparison.study1
{
    public class FixedSlider : Slider
    {
        // NOTE: expecting this to be in the same row and the row on which the slider will be
        public int leftMostButtonIdx, rightMostButtonIdx;

        private FixedButtonLayout fixedButtonLayout;
        private PlaneMeshGenerator planeMeshGenerator;

        protected override void Start()
        {
            base.Start();
            fixedButtonLayout = GetComponentInParent<FixedButtonLayout>();
            planeMeshGenerator = GetComponentInChildren<PlaneMeshGenerator>();
            fixedButtonLayout.ParametersSet += SetupSlider;
        }

        public void SetupSlider()
        {
            Transform leftMostButton = fixedButtonLayout.buttons[leftMostButtonIdx];
            Bounds leftBounds = leftMostButton.GetComponentInChildren<SpriteRenderer>().bounds;
            // float leftEdge = leftBounds.center.x - leftBounds.extents.x;
            Vector3 leftEdge = new Vector3(leftBounds.center.x, 0, leftBounds.center.z) + leftBounds.extents.x * leftMostButton.right.normalized;
            leftEdge.y = leftBounds.center.y;
            Vector3 bottomedge = new Vector3(0, leftBounds.center.y, leftBounds.center.z) - leftBounds.extents.y * leftMostButton.up.normalized;
            bottomedge.x = leftBounds.center.x;
            Vector3 topedge = new Vector3(0, leftBounds.center.y, leftBounds.center.z) + leftBounds.extents.y * leftMostButton.up.normalized;
            topedge.x = leftBounds.center.x;

            Transform rightMostButton = fixedButtonLayout.buttons[rightMostButtonIdx];
            Bounds rightBounds = rightMostButton.GetComponentInChildren<SpriteRenderer>().bounds;
            Vector3 rightEdge = new Vector3(rightBounds.center.x, 0, rightBounds.center.z) - rightBounds.extents.x * rightMostButton.right.normalized;
            rightEdge.y = rightBounds.center.y;

            // height, width
            float[] dimensions = new float[2]
            {
                (leftEdge - rightEdge).magnitude /1.5f,
                (topedge - bottomedge).magnitude /1.5f
            };

            planeMeshGenerator.transformAnchor.position = leftEdge;

            planeMeshGenerator.CreateFlatMesh(dimensions);
            displayManager.SetupButtons();
            displayManager.SetButtonLocations(DeformableSurfaceDisplayManager.Method.multifingerFOR_dynamic_deformed_spline);

            foreach(ButtonController btn in displayManager.buttonControllers)
            {
                btn.transform.parent.localRotation = leftMostButton.transform.localRotation;
            }

            for (int i = leftMostButtonIdx; i <= rightMostButtonIdx; i++)
            {
                fixedButtonLayout.buttons[i].gameObject.SetActive(false);
            }
        }
    }
}
