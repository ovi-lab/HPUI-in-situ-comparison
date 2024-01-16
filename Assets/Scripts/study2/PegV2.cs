using UnityEngine;
using ubco.ovilab.hpuiInSituComparison.study1;

namespace ubco.ovilab.hpuiInSituComparison.study2
{
    public class PegV2 : Peg
    {
        private InteractableTracker tracker;

        public new Transform trackingObject
        {
            get {
                return tracker.parent;
            }
            set {
                tracker.enabled = value != null;
                tracker.parent = value;
            }
        }

        private void Start()
        {
            tracker = GetComponent<InteractableTracker>();
            trackingObject = null; // trigger activation
        }
    }
}
