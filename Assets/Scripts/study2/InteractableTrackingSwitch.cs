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
        private JointFollower jointFollower;
        public bool usingJointFollower;
        public Transform parent;

        /// <inheritdoc />
        private void OnEnable()
        {
            if (jointFollower == null)
            {
                jointFollower = GetComponent<JointFollower>();
            }

            jointFollower.enabled = usingJointFollower;
        }

        /// <inheritdoc />
        public void Update()
        {
            if (usingJointFollower)
                return;

            transform.position = parent.position;
            transform.rotation = parent.rotation;
        }

        /// <summary>
        /// Switch tracking.
        /// </summary>
        public void UseJointFollower(bool usingJointFollower)
        {
            this.usingJointFollower = usingJointFollower;
            jointFollower.enabled = usingJointFollower;
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
