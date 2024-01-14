using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ubco.ovilab.HPUI.Legacy;
using System;

namespace ubco.ovilab.hpuiInSituComparison.study1
{
    public class VisualFeedbackContainer : MonoBehaviour
    {
        [SerializeField]
        List<CoupledButtonControllers> coupledButtonControllers;
        [SerializeField]
        List<CoupledSliders> coupledSliders;

        void Start()
        {
            foreach(Collider c in GetComponentsInChildren<Collider>())
            {
                c.enabled = false;
            }
            foreach(ButtonController btn in GetComponentsInChildren<ButtonController>())
            {
                btn.name = btn.transform.parent.parent.name;
            }

            foreach(CoupledButtonControllers c in coupledButtonControllers)
            {
                c.Couple();
            }
            foreach(CoupledSliders c in coupledSliders)
            {
                c.Couple();
            }
        }
    }

    [Serializable]
    class CoupledButtonControllers
    {
        public ButtonController target;
        public ButtonController source;

        public void Couple()
        {
            target.contactAction.AddListener((btn) => source.InvokeContact());
            target.proximateAction.AddListener((btn) => source.InvokeProximate());
            target.defaultAction.AddListener((btn) => source.InvokeDefault());
        }
    }

    [Serializable]
    class CoupledSliders
    {
        public Slider target;
        public Slider source;

        public void Couple()
        {
            target.OnSliderEventChange += (y, slider) => source.SetSliderValue(y);
        }
    }
}
