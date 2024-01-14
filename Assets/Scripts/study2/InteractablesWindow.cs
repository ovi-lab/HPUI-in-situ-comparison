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
        public List<InteractableTrackingSwitch> interactables;

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
                InteractableTrackingSwitch interactable = interactables[i];
                interactable.UseTransformAnchor(frame.GetAnchor(i));
            }
        }

        public void UseHPUI()
        {
            Show();
            foreach (InteractableTrackingSwitch interactable in interactables)
            {
                interactable.UseJointFollower(true);
            }
        }
    }
}
