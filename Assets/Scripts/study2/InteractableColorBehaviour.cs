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

        public void OnEnable()
        {
            if (interactable != null)
            {
                interactable.hoverEntered.AddListener(_ => SetColor("hovered"));
                interactable.selectEntered.AddListener(_ => SetColor("selected"));
                interactable.TapEvent.AddListener(_ => SetColor("tap"));
                interactable.selectExited.AddListener(_ => SetColor("hovered"));
                interactable.hoverExited.AddListener(_ => SetColor());
            }
        }

        public void SetColor(string state=null)
        {
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
