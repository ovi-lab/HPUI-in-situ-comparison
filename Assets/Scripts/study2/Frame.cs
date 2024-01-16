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
        [HideInInspector] public List<Transform> gridAnchors;
        [HideInInspector] public FixedTargetLayout fixedTargetLayout;

        public Frame(int index)
        {
            this.index = index;
        }

        public void SetupLayout(int fixedLayoutColumns, int fixedLayoutRows, float fixedLayoutSeperation, float fixedLayoutButtonScale)
        {
            GameObject frameObject = new GameObject($"frame_{baseAnchor.name}");
            // FIXME: Should be first destroyed?
            fixedTargetLayout = frameObject.AddComponent<FixedTargetLayout>();
            fixedTargetLayout.numberOfColumns = fixedLayoutColumns;
            fixedTargetLayout.numberOfRows = fixedLayoutRows;
            fixedTargetLayout.targets = Enumerable.Range(1, fixedLayoutColumns * fixedLayoutRows)
                .Select(i => new GameObject($"anchor_{i}").transform)
                .Select(t => { t.parent = baseAnchor; return t; })
                .ToList();
            // TODO: Backplate
            fixedTargetLayout.SetParameters(fixedLayoutSeperation, fixedLayoutButtonScale, baseAnchor.position, baseAnchor.rotation);
            frameObject.transform.parent = baseAnchor;

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
