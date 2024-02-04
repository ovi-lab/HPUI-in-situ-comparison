using System;
using System.Collections.Generic;
using System.Linq;
using ubco.ovilab.HPUI.Interaction;
using UnityEngine;

namespace ubco.ovilab.hpuiInSituComparison.study2
{
    /// <summary>
    /// Window manager which spawns an abstract window to select from.
    /// </summary>
    public class TapWindowManager : WindowManager
    {
        [Tooltip("The button prefab used to populate a window.")]
        [SerializeField] private GameObject interactablePrefab;
        [Tooltip("The interactable which would launch the switch view.")]
        [SerializeField] private HPUIBaseInteractable switchInteractable;
        [Tooltip("The anchor where the secondary display will be placed.")]
        [SerializeField] private Transform displayAnchor;
        [Tooltip("Sprites used to select sub frames. Must be in order.")]
        [SerializeField] private List<Sprite> sprites;

        [SerializeField] private float displayToSubFrameRatio = 4;

        private Frame displayFrame;
        private List<Frame> frames;
        private int currentOffset;
        private bool frameSelection;

        private InteractablesWindow frameSelectionWindow;
        private Dictionary<HPUIBaseInteractable, InteractableTracker> interactableToTrackerMapping;

        #region window manager implementations
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
                frames.Add(subFrame);
            }

            frameSelectionWindow = InteractablesWindow.GenerateWindow(-1, 9, this.transform, OnTap, interactablePrefab);

            interactableToTrackerMapping = frameSelectionWindow.interactables.ToDictionary(i => i.Interactable as HPUIBaseInteractable, i => i);
        }

        /// <inheritdoc />
        public override void SetupWindows(int offset)
        {
            currentOffset = offset;
            Windows[offset].UseFrame(hpuiFrame); // set frame and show
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
        #endregion

        /// <summary>
        /// Toggle between frame selection and active targets
        /// </summary>
        public void ToggleState()
        {
            frameSelection = !frameSelection;
            if (frameSelection)
            {
                foreach((InteractablesWindow w, Frame f) val in Windows.Zip(frames, (w, f) => (w, f)))
                {
                    val.w.UseFrame(val.f);
                }

                frameSelectionWindow.UseFrame(hpuiFrame);
            }
            else
            {
                frameSelectionWindow.Hide();

                foreach(InteractablesWindow window in Windows)
                {
                    window.Hide();
                }

                SetupWindows(currentOffset);
            }
        }

        private void OnTap(HPUITapEventArgs args)
        {
            // Should always get a HPUIBaseInteractable
            currentOffset = frameSelectionWindow.interactables.IndexOf(interactableToTrackerMapping[args.interactableObject as HPUIBaseInteractable]);
            ToggleState();
        }
    }
}
