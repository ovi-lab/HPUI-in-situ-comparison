using System.Collections.Generic;
using System.Linq;
using ubco.ovilab.HPUI.Interaction;
using ubco.ovilab.HPUI.Tracking;
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
        public GameObject continuousInteractablesRoot;
        public AnchorIndexToJointMapping anchorIndexToJointMapping;
        public int currentOffset;
        public float fixedLayoutSeperation = 0.005f;
        public float fixedLayoutButtonScale = 1f;
        public int fixedLayoutColumns = 3;
        public int fixedLayoutRows = 2;

        private Dictionary<int, InteractablesWindow> frameWindowMapping = new Dictionary<int, InteractablesWindow>();
        private Frame hpuiFrame;

        /// <summary>
        /// Setup the frames on which the windows will be displayed.
        /// </summary>
        public void SetupFrames()
        {
            Debug.Assert(frames.Select(f => f.index == 0).Any(), "Cannot have a frame with index 0!!");
            foreach(Frame frame in frames)
            {
                frame.SetupLayout(fixedLayoutColumns, fixedLayoutRows, fixedLayoutSeperation, fixedLayoutButtonScale);
            }

            hpuiFrame = new Frame(0);
            foreach (JointFollower follower in anchorIndexToJointMapping.JointFollowers)
            {
                hpuiFrame.gridAnchors.Add(follower.transform);
            }
        }

        /// <summary>
        /// Setup the windows with the correct layout of windows.
        /// </summary>
        public void SetupWindows(int offset)
        {
            Debug.Assert(frames.Select(f => f.index == 0).Any(), "Cannot have a frame with index 0!!");

            IEnumerable<int> indices = frames.Select(f => f.index);
            currentOffset = Mathf.Clamp(offset, indices.Min(), indices.Max() - 1);

            List<Frame> sortedFrames = frames.ToList().Append(hpuiFrame).OrderBy(f => f.index).ToList();
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
                    window.UseFrame(frame);
                }
                else
                {
                    window.Hide();
                }
            }
        }

        /// <summary>
        /// Shift all the windows to the right
        /// </summary>
        public void ShiftWindowsRight()
        {
            SetupWindows(currentOffset + 1);
        }

        /// <summary>
        /// Shift all the windows to the left
        /// </summary>
        public void ShiftWindowsLeft()
        {
            SetupWindows(currentOffset - 1);
        }
    }
}
