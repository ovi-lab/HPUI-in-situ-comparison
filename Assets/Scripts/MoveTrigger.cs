using ubc.ok.ovilab.HPUI.Core;
using UnityEngine;

namespace ubc.ok.ovilab.hpuiInSituComparison.study1
{
    public class MoveTrigger : MonoBehaviour
    {
        public MoveRange moveRange;
        public FixedButtonLayout buttonLayout;

        private bool moving;
        private Transform trackingTransform;
        private Vector3 initialOffset, initialPosition;
        private Transform movingTransform;

        private void Start()
        {
            movingTransform = buttonLayout.transform;
        }

        private void OnEnable()
        {
            moving = false;
        }

        private void OnDisable()
        {
            moving = false;
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
                buttonLayout.offset = movingTransform.position - initialPosition;
            }
        }
    }
}
