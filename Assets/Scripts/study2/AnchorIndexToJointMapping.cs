using System.Collections.Generic;
using ubco.ovilab.HPUI.Tracking;
using UnityEngine;

namespace ubco.ovilab.hpuiInSituComparison.study2
{
    public class AnchorIndexToJointMapping: MonoBehaviour
    {
        [Tooltip("The joints ordered by the index.")]
        public List<JointFollower> jointFollowers;
    }
}
