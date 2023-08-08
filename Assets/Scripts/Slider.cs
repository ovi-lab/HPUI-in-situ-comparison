using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ubc.ok.ovilab.hpuiInSituComparison.study1
{
    public class Slider : MonoBehaviour
    {
        // A value between 0 and 1;
        public event System.Action<float> OnSliderEventChange;

        protected void InvokeSliderEvent(float val)
        {
            OnSliderEventChange.Invoke(val);
        }
    }
}
