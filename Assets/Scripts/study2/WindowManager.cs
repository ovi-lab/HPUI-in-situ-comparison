using System.Collections.Generic;
using UnityEngine;

namespace ubco.ovilab.hpuiInSituComparison.study2
{
    /// <summary>
    /// This class dictates where and how a given is displayed. 
    /// </summary>
    public abstract class WindowManager : MonoBehaviour
    {
        public List<Frame> frames;
        [Tooltip("From the left most to right most.")]
        public AnchorIndexToJointMapping anchorIndexToJointMapping;
        public int currentOffset;
        public float fixedLayoutSeperation = 0.005f;
        public float fixedLayoutButtonScale = 1f;
        public int fixedLayoutColumns = 3;
        public int fixedLayoutRows = 2;


        private Dictionary<int, InteractablesWindow> frameWindowMapping = new Dictionary<int, InteractablesWindow>();
        private Frame hpuiFrame;

        private List<InteractablesWindow> windows;
        public List<InteractablesWindow> Windows { get => windows; set => windows = value; }

        /// <summary>
        /// Setup the frames on which the windows will be displayed.
        /// </summary>
        public abstract void SetupFrames();

        /// <summary>
        /// Setup the windows with the correct layout of windows.
        /// </summary>
        public abstract void SetupWindows(int offset);

        /// <summary>
        /// Shift all the windows to the right
        /// </summary>
        public abstract void ShiftWindowsRight();

        /// <summary>
        /// Shift all the windows to the left
        /// </summary>
        public abstract void ShiftWindowsLeft();
    }
}
