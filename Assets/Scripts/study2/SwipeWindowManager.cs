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

            foreach (HPUIContinuousInteractable interactable in continuousInteractables)
            {
                interactable.GestureEvent.AddListener(OnGesture);
            }
        }

        /// <inheritdoc />
        public override void SetupWindows(int offset)
        {
            Debug.Assert(frames.Select(f => f.index == 0).Any(), "Cannot have a frame with index 0!!");

            IEnumerable<int> indices = frames.Select(f => f.index);
            currentOffset = Mathf.Clamp(offset, indices.Min(), indices.Max() - 1);

            List<Frame> sortedFrames = frames.ToList().Append(hpuiFrame).OrderBy(f => f.index).ToList();
            List<InteractablesWindow> selectedWindows = Windows.ToList(); // Creating a copy

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
