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

        private Transform previousParent;
        private float tween;

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
                if (previousParent != null && tween < 1)
                {
                    transform.position = Vector3.Lerp(previousParent.position, parent.position, tween);
                }
                else
                {
                    transform.position = parent.position;
                }
                transform.rotation = parent.rotation;
            }
        }

        /// <summary>
        /// Switch tracking.
        /// </summary>
        public void UseTransformAnchor(Transform parent)
        {
            if (parent != this.parent)
            {
                previousParent = this.parent;
            }
            this.parent = parent;
        }

        public void SetTween(float tween)
        {
            this.tween = Mathf.Clamp(tween, 0, 1);
        }
    }
}
