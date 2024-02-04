using System.Collections.Generic;
using ubco.ovilab.HPUI.Interaction;
using UnityEngine;

namespace ubco.ovilab.hpuiInSituComparison.study2
{
    /// <summary>
    /// Window manager which spawns an abstract window to select from.
    /// </summary>
    public class TapWindowManager : WindowManager
    {
        [Tooltip("The interactable which would launch the switch view.")]
        [SerializeField] private HPUIBaseInteractable switchInteractable;
        [Tooltip("The anchor where the secondary display will be placed.")]
        [SerializeField] private Transform displayAnchor;

        [SerializeField] private float displayToSubFrameRatio = 4;

        private Frame displayFrame;
        private List<Frame> frames;

        private InteractablesWindow activeWindow;

        /// <inheritdoc />
        public override void SetupFrames()
        {
            base.SetupFrames();
            displayFrame = new Frame(-1);
            displayFrame.baseAnchor = displayAnchor;
            // NOTE: The fixedLayoutButtonScale doesn't impact the targets as they are only following the buttons.
            // This would take effect if they are set as the FixedLayoutButton.targets
            displayFrame.SetupLayout(fixedLayoutColumns, fixedLayoutRows, fixedLayoutSeperation, fixedLayoutButtonScale, displayAnchor.rotation);
            for (int i = 0; i < fixedLayoutColumns * fixedLayoutRows; i++)
            {
                Frame subFrame = new Frame(i);
                subFrame.baseAnchor = displayFrame.GetAnchor(i);
                subFrame.SetupLayout(fixedLayoutColumns,
                                     fixedLayoutRows,
                                     fixedLayoutSeperation / displayToSubFrameRatio,
                                     fixedLayoutButtonScale / displayToSubFrameRatio);
            }
        }

        /// <inheritdoc />
        public override void SetupWindows(int offset)
        {
            Windows[offset].UseFrame(hpuiFrame);
        }

        /// <inheritdoc />
        public override List<IHPUIInteractable> GetManagedInteractables()
        {
            return new List<IHPUIInteractable>(){ switchInteractable };
        }

        /// <inheritdoc />
        public override void Enable()
        {
            base.Enable();
            switchInteractable.gameObject.SetActive(true);
        }

        /// <inheritdoc />
        public override void Disable()
        {
            base.Disable();
            switchInteractable.gameObject.SetActive(false);
        }
    }
}
