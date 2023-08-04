using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ubc.ok.ovilab.hpuiInSituComparison.study1
{
    public class AlwaysFaceCamera : MonoBehaviour
    {
        void Update()
        {
            Vector3 forward = Camera.main.transform.position - transform.position;
            Vector3 right = Vector3.Cross(forward, Vector3.up);
            transform.rotation = Quaternion.LookRotation(forward, Vector3.Cross(right, forward));
        }
    }
}
