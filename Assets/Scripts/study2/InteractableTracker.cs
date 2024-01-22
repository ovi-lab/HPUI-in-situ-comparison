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
        [SerializeField] private IHPUIInteractable interactable;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private HPUIInteratableTracker tracker;

        public IHPUIInteractable Interactable
        {
            get
            {
                if (interactable == null)
                {
                    interactable = GetComponent<IHPUIInteractable>();
                }
                return interactable;
            }
            set => interactable = value;
        }

        public SpriteRenderer SpriteRenderer
        {
            get
            {
                if (spriteRenderer == null)
                {
                    spriteRenderer = GetComponentInChildren<SpriteRenderer>();
                }
                return spriteRenderer;
            }

            set => spriteRenderer = value;
        }

        public HPUIInteratableTracker Tracker
        {
            get
            {
                if (tracker == null)
                {
                    tracker = GetComponent<HPUIInteratableTracker>();
                }
                return tracker;
            }

            set => tracker = value;
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
