using System.Linq;
using ubco.ovilab.HPUI.Interaction;
using UnityEngine;

namespace ubco.ovilab.hpuiInSituComparison.study2
{
    /// <summary>
    /// This class dictates where and how a given is displayed. 
    /// </summary>
    public class SlideWindowManager : SwipeWindowManager
    {
        private float windowSize;
        private int minIndex;

        // NOTE: Expecting the frame.index to go from -N to +N, exluding 0.
        public override void SetupFrames()
        {
            base.SetupFrames();
            windowSize = (frames.Count + 1); // number of frames + hpui
            minIndex = frames.Select(f => f.index).Min();
        }

        /// <inheritdoc />
        public override void OnGesture(HPUIGestureEventArgs args)
        {
            // Start gesturing only after moving for a bit
            if (args.CumilativeDistance > 0.02)
            {
                float max = args.interactableObject.boundsMax.y;
                float min = args.interactableObject.boundsMin.y;

                float currentVal = (args.CumilativeDirection.y - min) / (max - min);

                int newOffset = Mathf.FloorToInt(currentVal * windowSize);
                if (currentOffset != newOffset)
                {
                    SetupWindows(newOffset);
                }
            }
        }
    }
}
