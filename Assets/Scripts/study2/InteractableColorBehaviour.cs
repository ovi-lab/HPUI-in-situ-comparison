using System.Collections;
using System.Linq;
using ubco.ovilab.HPUI.Interaction;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace ubco.ovilab.hpuiInSituComparison.study2
{
    /// <summary>
    /// Simple color behaviour for tap interactions.
    /// </summary>
    public class InteractableColorBehaviour : MonoBehaviour
    {
        public ColorTheme theme;
        public HPUIBaseInteractable interactable;
        public Renderer targetRenderer;

        private bool awaitingDelayedReset = false;
        private string stateAfterDelayedReset = null;

        public void OnEnable()
        {
            awaitingDelayedReset = false;
            SetColor();
            if (interactable != null)
            {
                interactable.hoverEntered.AddListener(SetColorHoverEntered);
                interactable.selectEntered.AddListener(SetColorSelectEntered);
                interactable.TapEvent.AddListener(SetColorTapped);
                interactable.selectExited.AddListener(SetColorSelectExited);
                interactable.hoverExited.AddListener(SetColorHoverExited);
            }
        }

        public void OnDisable()
        {
            if (interactable != null)
            {
                interactable.hoverEntered.RemoveListener(SetColorHoverEntered);
                interactable.selectEntered.RemoveListener(SetColorSelectEntered);
                interactable.TapEvent.RemoveListener(SetColorTapped);
                interactable.selectExited.RemoveListener(SetColorSelectExited);
                interactable.hoverExited.RemoveListener(SetColorHoverExited);
            }
            StopAllCoroutines();
            SetColor();
        }

        private void SetColorHoverEntered(HoverEnterEventArgs arg) { SetColor("hovered"); }
        private void SetColorSelectExited(SelectExitEventArgs arg) { SetColor("hovered"); }
        private void SetColorSelectEntered(SelectEnterEventArgs arg) { SetColor("selected"); }
        private void SetColorTapped(HPUITapEventArgs arg) { StartCoroutine(SetWithDelayedReset("tap")); }
        private void SetColorHoverExited(HoverExitEventArgs arg) { SetColor(); }


        private IEnumerator SetWithDelayedReset(string state)
        {
            // If state has changed while waiting for the color to go back, will have to go the new state
            SetColor(state);
            awaitingDelayedReset = true;
            yield return new WaitForSeconds(0.1f);
            awaitingDelayedReset = false;
            SetColor(stateAfterDelayedReset);
            stateAfterDelayedReset = null;
        }

        private void SetColor(string state=null)
        {
            if (awaitingDelayedReset)
            {
                stateAfterDelayedReset = state;
                return;
            }

            Color c;
            if (string.IsNullOrEmpty(state))
            {
                c = theme.defaultColor;
            }
            else
            {
                c = theme.values.FirstOrDefault(v => v.stateName == state)?.color ?? theme.defaultColor;
            }
            targetRenderer.material.color = c;
        }
    }
}
