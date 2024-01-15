using System;
using System.Collections.Generic;
using System.Linq;
using ubco.ovilab.hpuiInSituComparison.common;
using UnityEditor;
using UnityEngine;

namespace ubco.ovilab.hpuiInSituComparison.study2
{
    /// <summary>
    /// This class dictates where and how a given is displayed. 
    /// </summary>
    public class WindowManager : MonoBehaviour
    {
        public List<Frame> frames;
        [Tooltip("From the left most to right most.")]
        public List<InteractablesWindow> windows;
        public int currentOffset;
        public float fixedLayoutSeperation = 0.005f;
        public float fixedLayoutButtonScale = 1f;
        public int fixedLayoutColumns = 3;
        public int fixedLayoutRows = 2;

        private Dictionary<int, InteractablesWindow> frameWindowMapping = new Dictionary<int, InteractablesWindow>();

        public void SetupFrames()
        {
            foreach(Frame frame in frames)
            {
                frame.SetupLayout(fixedLayoutColumns, fixedLayoutRows, fixedLayoutSeperation, fixedLayoutButtonScale);
            }
        }

        public void SetupWindows(int offset)
        {
            Debug.Assert(frames.Select(f => f.index == 0).Any(), "Cannot have a frame with index 0!!");

            IEnumerable<int> indices = frames.Select(f => f.index);
            currentOffset = Mathf.Clamp(offset, indices.Min(), indices.Max() - 1);

            List<Frame> sortedFrames = frames.ToList().Append(new Frame(0)).OrderBy(f => f.index).ToList();
            List<InteractablesWindow> selectedWindows = windows.ToList(); // Creating a copy

            if (offset > 0)
            {
                sortedFrames = sortedFrames.Skip(offset).ToList();
            }
            else if (offset < 0)
            {
                selectedWindows = selectedWindows.Skip(-offset).ToList();
            }

            for (int i = 0; i < selectedWindows.Count; i++)
            {
                InteractablesWindow window = selectedWindows[i];
                if (i < sortedFrames.Count)
                {
                    Frame frame = sortedFrames[i];
                    if (frame.index == 0)
                    {
                        window.UseHPUI();
                    }
                    else
                    {
                        window.UseFrame(frame);
                    }
                }
                else
                {
                    window.Hide();
                }
            }
        }

        public void ShiftWindowsRight()
        {
            SetupWindows(currentOffset + 1);
        }

        public void ShiftWindowsLeft()
        {
            SetupWindows(currentOffset - 1);
        }
    }

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
