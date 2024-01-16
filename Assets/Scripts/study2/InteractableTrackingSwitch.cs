using ubco.ovilab.HPUI.Tracking;
using UnityEngine;

namespace ubco.ovilab.hpuiInSituComparison.study2
{
    [RequireComponent(typeof(JointFollower))]
    /// <summary>
    /// This class allows switching between using the <see
    /// cref="JointFollower"/> or making this follow the pose of an
    /// object.
    /// </summary>
    public class InteractableTrackingSwitch : MonoBehaviour
    {
        public JointFollower jointFollower;
        public bool usingJointFollower;
        public Transform parent;

        /// <inheritdoc />
        private void OnEnable()
        {
            if (jointFollower != null)
            {
                jointFollower.enabled = usingJointFollower;
            }
        }

        /// <inheritdoc />
        public void Update()
        {
            if (usingJointFollower)
                return;

            if (parent != null)
            {
                transform.position = parent.position;
                transform.rotation = parent.rotation;
            }
        }

        /// <summary>
        /// Switch tracking.
        /// </summary>
        public void UseJointFollower(bool usingJointFollower, JointFollower jointFollower=null)
        {
            this.usingJointFollower = usingJointFollower;
            if (jointFollower != null)
            {
                this.jointFollower = jointFollower;
                jointFollower.enabled = usingJointFollower;
            }
        }

        /// <summary>
        /// Switch tracking.
        /// </summary>
        public void UseTransformAnchor(Transform parent)
        {
            this.parent = parent;
            UseJointFollower(false);
        }
    }
}
