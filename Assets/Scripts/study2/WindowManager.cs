using System.Collections.Generic;
using ubco.ovilab.HPUI.Interaction;
using UnityEngine;

namespace ubco.ovilab.hpuiInSituComparison.study2
{
    /// <summary>
    /// This class dictates where and how a given is displayed. 
    /// </summary>
    public abstract class WindowManager : MonoBehaviour
    {
        [Tooltip("The identifier for this windowmanager. Will be refernced in all other material by this.")]
        public string id;
        [Tooltip("The list of frames this window manager is handling.")]
        [SerializeField] protected List<Frame> frames;
        [Tooltip("From the left most to right most.")]
        [SerializeField] protected AnchorIndexToJointMapping anchorIndexToJointMapping;

        [Header("Parameters to use with any fixedLayouts generated for the frames.")]
        [SerializeField] protected float fixedLayoutSeperation = 0.005f;
        [SerializeField] protected float fixedLayoutButtonScale = 1f;
        [SerializeField] protected int fixedLayoutColumns = 3;
        [SerializeField] protected int fixedLayoutRows = 2;

        protected int currentOffset;

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

        /// <summary>
        /// Get any interactables specifically managed by the window manager.
        /// </summary>
        public virtual List<IHPUIInteractable> GetManagedInteractables()
        {
            return null;
        }

        /// <summary>
        /// Disable this window manager
        /// </summary>
        public virtual void Disable()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Enable this window manager
        /// </summary>
        public virtual void Enable()
        {
            gameObject.SetActive(true);
        }
    }
}
