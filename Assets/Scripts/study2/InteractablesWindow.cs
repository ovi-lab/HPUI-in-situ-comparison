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

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void UseFrame(Frame frame)
        {
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
    }
}
