using System.Collections.Generic;
using System.Linq;
using ubco.ovilab.HPUI.Interaction;
using UnityEditor;
using UnityEngine;

namespace ubco.ovilab.hpuiInSituComparison.study2
{
    /// <summary>
    /// This class dictates where and how a given is displayed. 
    /// </summary>
    public class SwipeWindowManager : WindowManager
    {
        [Tooltip("The list of frames this window manager is handling.")]
        [SerializeField] protected List<Frame> frames;

        protected int currentOffset;

        public List<HPUIContinuousInteractable> continuousInteractables;
        private Dictionary<int, InteractablesWindow> frameWindowMapping = new Dictionary<int, InteractablesWindow>();

        // NOTE: Expecting the frame.index to go from -N to +N, exluding 0.
        /// <inheritdoc />
        public override void SetupFrames()
        {
            base.SetupFrames();

            Debug.Assert(frames.Select(f => f.index == 0).Any(), "Cannot have a frame with index 0!!");
            foreach(Frame frame in frames)
            {
                frame.SetupLayout(fixedLayoutColumns, fixedLayoutRows, fixedLayoutSeperation, fixedLayoutButtonScale);
            }
        }

        /// <inheritdoc />
        protected virtual void OnEnable()
        {
            foreach (HPUIContinuousInteractable interactable in continuousInteractables)
            {
                interactable.GestureEvent.AddListener(OnGesture);
            }

            foreach (Frame f in frames)
            {
                f.backplateObject.SetActive(true);
            }
        }

        /// <inheritdoc />
        protected virtual void OnDisable()
        {
            foreach (HPUIContinuousInteractable interactable in continuousInteractables)
            {
                interactable.GestureEvent.RemoveListener(OnGesture);
            }

            foreach (Frame f in frames)
            {
                f.backplateObject.SetActive(false);
            }
        }

        /// <inheritdoc />
        public override void SetupWindows(int offset)
        {
            Debug.Assert(frames.Select(f => f.index == 0).Any(), "Cannot have a frame with index 0!!");

            if (offset > (Windows.Count - 1) || offset < 0)
            {
                return;
            }

            currentOffset = offset;

            IEnumerable<Frame> sortedFrames = frames.ToList().Append(hpuiFrame).OrderBy(f => f.index);
            IEnumerable<InteractablesWindow> selectedWindows = Windows.ToList(); // Creating a copy

            int indexOfZero = sortedFrames.ToList().IndexOf(hpuiFrame);

            int frameEndIndex = indexOfZero + offset,
                frameStartIndex = frameEndIndex - selectedWindows.Count() + 1,
                sortedFramesCount = sortedFrames.Count();

            if (frameStartIndex < 0)
            {
                for (int i = frameStartIndex; i < 0; i++)
                {
                    sortedFrames = sortedFrames.Prepend(null);
                }
            }
            else if (frameStartIndex > 0)
            {
                for (int i = frameStartIndex; i > 0; i--)
                {
                    selectedWindows = selectedWindows.Prepend(null);
                }
            }

            if (frameEndIndex > sortedFramesCount)
            {
                for (int i = sortedFramesCount; i < frameEndIndex; i++)
                {
                    sortedFrames = sortedFrames.Append(null);
                }
            }
            else if (frameEndIndex < sortedFramesCount)
            {
                for (int i = sortedFramesCount; i > frameEndIndex; i--)
                {
                    selectedWindows = selectedWindows.Append(null);
                }
            }

            foreach((InteractablesWindow window, Frame frame) pair in selectedWindows.Zip(sortedFrames, (window, frame) => (window, frame)))
            {
                if (pair.window == null)
                {
                    continue;
                }
                else if (pair.frame == null)
                {
                    pair.window.Hide();
                }
                else
                {
                    pair.window.UseFrame(pair.frame);
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

        /// <inheritdoc />
        public override List<IHPUIInteractable> GetManagedInteractables()
        {
            return continuousInteractables.Select(c => c as IHPUIInteractable).ToList();
        }

        /// <summary>
        /// Callback for <see cref="IHPUIInteractable.OnGestureEvent"/> for continuous interactables.
        /// </summary>
        public virtual void OnGesture(HPUIGestureEventArgs args)
        {
            SetBackplateVisibility(args.State);
            if (args.State == HPUIGestureState.Stopped && args.CumilativeDirection.magnitude > 0.02)
            {
                if (args.CumilativeDirection.y < 0)
                {
                    ShiftWindowsLeft();
                }
                else
                {
                    ShiftWindowsRight();
                }
            }
        }

        /// <summary>
        /// Update the visibility of the backplate based on the gesture state. Used in OnGesture
        /// </summary>
        protected void SetBackplateVisibility(HPUIGestureState gestureState)
        {
            if (gestureState == HPUIGestureState.Started || gestureState == HPUIGestureState.Stopped)
            {
                foreach (Frame f in frames)
                {
                    f.backplateObject.SetActive(gestureState == HPUIGestureState.Started);
                }
            }
        }

        /// <inheritdoc />
        public override void Enable()
        {
            base.Enable();
            foreach (HPUIContinuousInteractable interactable in continuousInteractables)
            {
                interactable.gameObject.SetActive(true);
            }
        }

        /// <inheritdoc />
        public override void Disable()
        {
            base.Disable();
            foreach (HPUIContinuousInteractable interactable in continuousInteractables)
            {
                interactable.gameObject.SetActive(false);
            }
        }
    }
}
