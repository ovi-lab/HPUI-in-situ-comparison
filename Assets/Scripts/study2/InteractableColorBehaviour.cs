using System.Collections;
using System.Linq;
using ubco.ovilab.HPUI.Interaction;
using UnityEngine;

namespace ubco.ovilab.hpuiInSituComparison.study2
{
    public class InteractableColorBehaviour : MonoBehaviour
    {
        public ColorTheme theme;
        public HPUIBaseInteractable interactable;
        public Renderer targetRenderer;

        private bool awaitingDelayedReset = false;
        private string stateAfterDelayedReset = null;

        public void OnEnable()
        {
            if (interactable != null)
            {
                interactable.hoverEntered.AddListener(_ => SetColor("hovered"));
                interactable.selectEntered.AddListener(_ => SetColor("selected"));
                interactable.TapEvent.AddListener(_ => StartCoroutine(SetWithDelayedReset("tap")));
                interactable.selectExited.AddListener(_ => SetColor("hovered"));
                interactable.hoverExited.AddListener(_ => SetColor());
            }
        }

        private IEnumerator SetWithDelayedReset(string state)
        {
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
