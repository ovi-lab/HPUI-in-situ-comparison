using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ubco.ovilab.hpuiInSituComparison.study1;

namespace ubco.ovilab.hpuiInSituComparison.study2
{
    public class PegV2 : Peg
    {
        private InteractableTrackingSwitch tracker;

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
            tracker = GetComponent<InteractableTrackingSwitch>();
            trackingObject = null; // trigger activation
        }
    }
}
