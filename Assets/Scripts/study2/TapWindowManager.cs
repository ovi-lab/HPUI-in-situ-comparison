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
        private Dictionary<HPUIBaseInteractable, int> interactableToTrackerMapping;

        public bool tapRecieved { get; private set; }

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

            frames = new List<Frame>();

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

            foreach ((InteractableTracker t, Sprite s) item in frameSelectionWindow.interactables.Zip(sprites, (t, s) => (t, s)))
            {
                item.t.SpriteRenderer.sprite = item.s;
            }

            foreach((Frame f, Sprite s) item in frames.Zip(sprites, (f, s) => (f, s)))
            {
                // TODO: Revisit position
                GameObject label = new GameObject("label");
                SpriteRenderer labelRenderer = label.AddComponent<SpriteRenderer>();
                labelRenderer.sprite = item.s;
                label.transform.parent = item.f.baseAnchor;
                label.transform.localScale = Vector3.one * 0.005f;
                float offset = fixedLayoutSeperation / displayToSubFrameRatio;
                label.transform.localPosition = new Vector3(offset, -offset, -0.001f);
                label.transform.rotation = Quaternion.LookRotation(-item.f.baseAnchor.forward, item.f.baseAnchor.up);
            }

            interactableToTrackerMapping = frameSelectionWindow.interactables.ToDictionary(i => i.Interactable as HPUIBaseInteractable, i => frameSelectionWindow.interactables.IndexOf(i));

            switchInteractable.TapEvent.AddListener(OnTap);
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

                SetupWindows(currentOffset);

                for (int i=0; i < Windows.Count; ++i)
                {
                    if (i != currentOffset)
                    {
                        Windows[i].Hide();
                    }
                }
            }
        }

        /// <summary>
        /// The callback used on the items that are to appear on the fingers when switching
        /// </summary>
        private void OnTap(HPUITapEventArgs args)
        {
            // Should always get a HPUIBaseInteractable
            HPUIBaseInteractable interactable = args.interactableObject as HPUIBaseInteractable;
            if (interactableToTrackerMapping.ContainsKey(interactable))
            {
                currentOffset = interactableToTrackerMapping[interactable];
            }
            tapRecieved = true;
        }

        private void Update()
        {
            // This is done to avoid the race condition where the Interactable.OnDisable the the TapEvent being called in the same frame.
            if (tapRecieved)
            {
                ToggleState();
                tapRecieved = false;
            }
        }
    }
}
