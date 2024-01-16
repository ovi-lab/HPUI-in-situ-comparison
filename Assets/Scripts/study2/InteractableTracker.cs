using ubco.ovilab.HPUI.Tracking;
using UnityEngine;

namespace ubco.ovilab.hpuiInSituComparison.study2
{
    /// <summary>
    /// This class allows switching between using the <see
    /// cref="JointFollower"/> or making this follow the pose of an
    /// object.
    /// </summary>
    public class InteractableTracker : MonoBehaviour
    {
        public Transform parent;

        /// <inheritdoc />
        public void Update()
        {
            if (parent != null)
            {
                transform.position = parent.position;
                transform.rotation = parent.rotation;
            }
        }

        /// <summary>
        /// Switch tracking.
        /// </summary>
        public void UseTransformAnchor(Transform parent)
        {
            this.parent = parent;
        }
    }
}
