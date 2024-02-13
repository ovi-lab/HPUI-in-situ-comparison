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

        [HideInInspector] public GameObject backplateObject;

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
            frameObject.transform.parent = parent;

            fixedTargetLayout = frameObject.AddComponent<FixedTargetLayout>();
            fixedTargetLayout.numberOfColumns = fixedLayoutColumns;
            fixedTargetLayout.numberOfRows = fixedLayoutRows;
            List<Transform> targets = Enumerable.Range(1, fixedLayoutColumns * fixedLayoutRows)
                .Select(i => new GameObject($"anchor_{i}").transform)
                .Select(t => {
                    t.parent = parent;
                    t.rotation = targetRotation;
                    return t;
                             })
                .ToList();
            fixedTargetLayout.targets = targets;

            fixedTargetLayout.SetParameters(fixedLayoutSeperation, fixedLayoutButtonScale, layoutPosition, layoutRotation);

            backplateObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
            Transform backplate = backplateObject.transform;
            backplate.parent = frameObject.transform;
            backplate.position = targets.Select(t => t.position).Aggregate((tot, next) => tot + next) / targets.Count;
            backplate.position -= backplate.up.normalized * 0.001f;
            backplate.localScale = new Vector3(fixedLayoutSeperation * fixedLayoutRows, 0.01f, fixedLayoutSeperation * fixedLayoutColumns) * 0.1f;
            backplate.rotation = targetRotation;

            MeshRenderer backplateRenderer = backplate.GetComponent<MeshRenderer>();
            Material material = new Material(Shader.Find("Standard"));

            // Yoinked from https://forum.unity.com/threads/change-standard-shader-render-mode-in-runtime.318815/
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.DisableKeyword("_ALPHABLEND_ON");
            material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            Color c = Color.white;
            c.a = 0.4f;
            material.color = c;

            backplateRenderer.material = material;

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
