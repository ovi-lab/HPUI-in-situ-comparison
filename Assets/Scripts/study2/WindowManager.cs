using System;
using System.Collections.Generic;
using System.Linq;
using ubco.ovilab.hpuiInSituComparison.common;
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

        private Dictionary<int, InteractablesWindow> frameWindowMapping = new Dictionary<int, InteractablesWindow>();

        public void SetupWindows(int offset)
        {
            currentOffset = offset;
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

            for (int i = 0; i < windows.Count; i++)
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
        /// <summary>
        /// The elements in a given layout, when not on HPUI, would be using these transforms.
        /// </summary>
        public List<Transform> gridAnchors;
        public FixedTargetLayout fixedTargetLayout;

        public Frame(int index)
        {
            this.index = index;
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
