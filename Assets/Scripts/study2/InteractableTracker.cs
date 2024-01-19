using ubco.ovilab.HPUI.Interaction;
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
        public IHPUIInteractable interactable;
        public SpriteRenderer spriteRenderer;
        public HPUIInteratableTracker tracker;

        /// <inheritdoc />
        private void OnEnable()
        {
            if (interactable == null)
            {
                interactable = GetComponent<IHPUIInteractable>();
            }

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (tracker == null)
            {
                tracker = GetComponent<HPUIInteratableTracker>();
            }
        }

        /// <inheritdoc />
        private void Update()
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
