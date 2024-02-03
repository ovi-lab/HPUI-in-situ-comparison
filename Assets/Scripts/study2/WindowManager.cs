using System.Collections.Generic;
using ubco.ovilab.HPUI.Interaction;
using ubco.ovilab.HPUI.Tracking;
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
        [Tooltip("From the left most to right most.")]
        [SerializeField] protected AnchorIndexToJointMapping anchorIndexToJointMapping;

        [Header("Parameters to use with any fixedLayouts generated for the frames.")]
        [SerializeField] protected float fixedLayoutSeperation = 0.005f;
        [SerializeField] protected float fixedLayoutButtonScale = 1f;
        [SerializeField] protected int fixedLayoutColumns = 3;
        [SerializeField] protected int fixedLayoutRows = 2;

        protected Frame hpuiFrame;

        public List<InteractablesWindow> Windows { get;set; }

        /// <summary>
        /// Setup the frames on which the windows will be displayed.
        /// </summary>
        public virtual void SetupFrames()
        {
            hpuiFrame = new Frame(0);
            foreach (JointFollower follower in anchorIndexToJointMapping.JointFollowers)
            {
                hpuiFrame.gridAnchors.Add(follower.transform);
            }
        }

        /// <summary>
        /// Setup the windows with the correct layout of windows.
        /// </summary>
        public abstract void SetupWindows(int offset);

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
