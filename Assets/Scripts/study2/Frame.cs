using System;
using System.Collections.Generic;
using System.Linq;
using ubco.ovilab.hpuiInSituComparison.common;
using UnityEditor;
using UnityEngine;

namespace ubco.ovilab.hpuiInSituComparison.study2
{
    [Serializable]
    /// <summary>
    /// Individual frames where a window can be placed.
    /// </summary>
    public class Frame
    {
        public int index;
        public Transform baseAnchor;
        /// <summary>
        /// The elements in a given layout, when not on HPUI, would be using these transforms.
        /// </summary>
        public List<Transform> gridAnchors;
        [HideInInspector] public FixedTargetLayout fixedTargetLayout;

        public Frame(int index)
        {
            this.index = index;
            this.gridAnchors = new List<Transform>();
        }

        public void SetupLayout(int fixedLayoutColumns, int fixedLayoutRows, float fixedLayoutSeperation, float fixedLayoutButtonScale)
        {
            // targetRotation is accounting for different corrdinate systems used
            SetupLayout(fixedLayoutColumns, fixedLayoutRows, fixedLayoutSeperation, fixedLayoutButtonScale, baseAnchor, baseAnchor.position, baseAnchor.rotation, Quaternion.LookRotation(baseAnchor.right, baseAnchor.forward));
        }

        public void SetupLayout(int fixedLayoutColumns, int fixedLayoutRows, float fixedLayoutSeperation, float fixedLayoutButtonScale, Quaternion targetRotation)
        {
            SetupLayout(fixedLayoutColumns, fixedLayoutRows, fixedLayoutSeperation, fixedLayoutButtonScale, baseAnchor, baseAnchor.position, baseAnchor.rotation, targetRotation);
        }

        public void SetupLayout(int fixedLayoutColumns, int fixedLayoutRows, float fixedLayoutSeperation, float fixedLayoutButtonScale, Transform parent, Vector3 layoutPosition, Quaternion layoutRotation, Quaternion targetRotation)
        {
            GameObject frameObject = new GameObject($"frame_{parent.name}");
            // FIXME: Should be first destroyed?
            fixedTargetLayout = frameObject.AddComponent<FixedTargetLayout>();
            fixedTargetLayout.numberOfColumns = fixedLayoutColumns;
            fixedTargetLayout.numberOfRows = fixedLayoutRows;
            fixedTargetLayout.targets = Enumerable.Range(1, fixedLayoutColumns * fixedLayoutRows)
                .Select(i => new GameObject($"anchor_{i}").transform)
                .Select(t => {
                    t.parent = parent;
                    t.rotation = targetRotation;
                    return t;
                             })
                .ToList();
            // TODO: Backplate
            fixedTargetLayout.SetParameters(fixedLayoutSeperation, fixedLayoutButtonScale, layoutPosition, layoutRotation);
            frameObject.transform.parent = parent;

            // FIXME: Debug code
            foreach(var t in fixedTargetLayout.targets)
            {
                var x = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                x.transform.parent = t;
                x.transform.localPosition = Vector3.zero;
                x.transform.localRotation = Quaternion.identity;
                x.transform.localScale = Vector3.one * 0.004f;
            }
        }

        public Transform GetAnchor(int index)
        {
            if (fixedTargetLayout != null)
            {
                return fixedTargetLayout.targets[index];
            }
            return gridAnchors[index];
        }
    }
}
