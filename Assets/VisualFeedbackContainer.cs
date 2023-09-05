using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ubc.ok.ovilab.hpuiInSituComparison.study1
{
    public class VisualFeedbackContainer : MonoBehaviour
    {
        void Start()
        {
            foreach(Collider c in GetComponentsInChildren<Collider>())
            {
                c.enabled = false;
            }
        }
    }
}
