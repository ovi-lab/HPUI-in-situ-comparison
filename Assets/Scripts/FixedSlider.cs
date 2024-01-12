using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ubco.ovilab.HPUI.Legacy.DeformableSurfaceDisplay;
using ubco.ovilab.HPUI.Legacy;

namespace ubco.ovilab.hpuiInSituComparison.study1
{
    public class FixedSlider : Slider
    {
        // NOTE: expecting this to be in the same row and the row on which the slider will be
        public int leftMostButtonIdx, rightMostButtonIdx;

        private FixedTargetLayout fixedTargetLayout;
        private PlaneMeshGenerator planeMeshGenerator;

        protected override void Start()
        {
            base.Start();
            fixedTargetLayout = GetComponentInParent<FixedTargetLayout>();
            planeMeshGenerator = GetComponentInChildren<PlaneMeshGenerator>();
            fixedTargetLayout.ParametersSet += SetupSlider;
        }

        public void SetupSlider()
        {
            Transform leftMostButton = fixedTargetLayout.targets[leftMostButtonIdx];
            Bounds leftBounds = leftMostButton.GetComponentInChildren<SpriteRenderer>().bounds;
            Vector3 leftEdge = new Vector3(leftBounds.center.x, 0, leftBounds.center.z) + leftBounds.extents.x * leftMostButton.right.normalized;
            leftEdge.y = leftBounds.center.y;
            Vector3 bottomedge = new Vector3(0, leftBounds.center.y, leftBounds.center.z) - leftBounds.extents.y * leftMostButton.up.normalized;
            bottomedge.x = leftBounds.center.x;
            Vector3 topedge = new Vector3(0, leftBounds.center.y, leftBounds.center.z) + leftBounds.extents.y * leftMostButton.up.normalized;
            topedge.x = leftBounds.center.x;

            Transform rightMostButton = fixedTargetLayout.targets[rightMostButtonIdx];
            Bounds rightBounds = rightMostButton.GetComponentInChildren<SpriteRenderer>().bounds;
            Vector3 rightEdge = new Vector3(rightBounds.center.x, 0, rightBounds.center.z) - rightBounds.extents.x * rightMostButton.right.normalized;
            rightEdge.y = rightBounds.center.y;

            // height, width
            float[] dimensions = new float[2]
            {
                (leftEdge - rightEdge).magnitude /1.5f,
                (topedge - bottomedge).magnitude /1.5f
            };

            displayManager.Setup();

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
                fixedTargetLayout.targets[i].gameObject.SetActive(false);
            }
        }
    }
}
