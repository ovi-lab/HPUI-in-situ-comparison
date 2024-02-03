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
    public class SwipeWindowManager : WindowManager
    {
        public List<HPUIContinuousInteractable> continuousInteractables;
        private Dictionary<int, InteractablesWindow> frameWindowMapping = new Dictionary<int, InteractablesWindow>();
        private Frame hpuiFrame;

        // NOTE: Expecting the frame.index to go from -N to +N, exluding 0.
        /// <inheritdoc />
        public override void SetupFrames()
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

        /// <inheritdoc />
        protected virtual void OnEnable()
        {
            foreach (HPUIContinuousInteractable interactable in continuousInteractables)
            {
                interactable.GestureEvent.AddListener(OnGesture);
            }
        }

        /// <inheritdoc />
        protected virtual void OnDisable()
        {
            foreach (HPUIContinuousInteractable interactable in continuousInteractables)
            {
                interactable.GestureEvent.RemoveListener(OnGesture);
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

        /// <inheritdoc />
        public override void ShiftWindowsRight()
        {
            SetupWindows(currentOffset + 1);
        }

        /// <inheritdoc />
        public override void ShiftWindowsLeft()
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
    }
}
