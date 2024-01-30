using ubco.ovilab.hpuiInSituComparison.study1;
using ubco.ovilab.HPUI.Tracking;
using UnityEngine.XR.Hands;

namespace ubco.ovilab.hpuiInSituComparison.study2
{
    public class PegV2 : Peg
    {
        public JointFollower follower;

        public Handedness Handedness
        {
            get => follower.Handedness;
            set => follower.Handedness = value;
        }

        private void Start()
        {
        }
    }
}
