using ubco.ovilab.HPUI.Legacy;
using ubco.ovilab.hpuiInSituComparison.common;
using UnityEngine;

namespace ubco.ovilab.hpuiInSituComparison.study1
{
    public class MoveTrigger : MonoBehaviour
    {
        public MoveRange moveRange;
        public FixedTargetLayout targetLayout;
        public GameObject otherTrigger;

        private bool moving;
        private Transform trackingTransform;
        private Vector3 initialOffset, initialPosition;
        private Transform movingTransform;

        #region Unity functions
        private void Start()
        {
            movingTransform = targetLayout.transform;
        }

        private void OnEnable()
        {
            moving = false;
            if (otherTrigger != null)
            {
                otherTrigger.SetActive(true);
            }
        }

        private void OnDisable()
        {
            moving = false;
            if (otherTrigger != null)
            {
                otherTrigger.SetActive(false);
            }
        }

        private void Update()
        {
            if (moving)
            {
                movingTransform.position = trackingTransform.position - initialOffset;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (moveRange.inRange && other.GetComponent<ButtonTriggerCollider>() != null)
            {
                moving = true;
                moveRange.SetSelected(true);
                trackingTransform = other.transform;
                initialOffset = trackingTransform.position - movingTransform.position;
                initialPosition = movingTransform.position;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.GetComponent<ButtonTriggerCollider>() != null)
            {
                moving = false;
                moveRange.SetSelected(false);
                trackingTransform = null;
                targetLayout.offset = movingTransform.position - initialPosition;
            }
        }
        #endregion
    }
}
