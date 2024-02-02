using System.Collections.Generic;
using UnityEngine;

namespace ubco.ovilab.hpuiInSituComparison.study2
{
    /// <summary>
    /// Contains information about the acitve windows.
    /// </summary>
    public class InteractablesWindow : MonoBehaviour
    {
        public int index;
        [Tooltip("Interactables with a `InteractableTrackingSwitch`")]
        public List<InteractableTracker> interactables;

        private Frame previousFrame;
        private float tweenStartTime;
        private bool tweening;

        private float tweenTime = 0.1f; // 100ms

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            foreach(InteractableTracker interactable in interactables)
            {
                interactable.SetTween(1);
            }
            gameObject.SetActive(false);
        }

        public void UseFrame(Frame frame)
        {
            if (previousFrame != frame)
            {
                previousFrame = frame;
                tweening = true;
                tweenStartTime = Time.time;
            }

            Show();
            for (int i = 0; i < interactables.Count; i++)
            {
                InteractableTracker interactable = interactables[i];
                interactable.UseTransformAnchor(frame.GetAnchor(i));
            }
        }

        private void OnDestory()
        {
            foreach (InteractableTracker interactable in interactables)
            {
                Destroy(interactable.gameObject);
            }
            interactables.Clear();
        }

        private void Update()
        {
            if (tweening)
            {
                float tween = (Time.time - tweenStartTime) / tweenTime;

                foreach(InteractableTracker interactable in interactables)
                {
                    interactable.SetTween(tween);
                }

                if (tween >= 1)
                {
                    tweening = false;
                }
            }
        }
    }
}
