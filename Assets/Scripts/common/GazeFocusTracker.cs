using System.Collections;
using System.Collections.Generic;
using UXF;
using System.Linq;
using UnityEngine;

namespace ubco.ovilab.hpuiInSituComparison.common
{
    /// <summary>
    /// Extending the PositionRotationTracker to also include which object is getting gaze focus
    /// </summary>
    public class GazeFocusTracker : PositionRotationTracker
    {
        public override string MeasurementDescriptor => "gazeFocus";
        public override IEnumerable<string> CustomHeader => base.CustomHeader.Union(new string[] {"object_name"}).ToArray();

        private int layerMask;

        private void Start()
        {
            // Bit shift the index of the layer (8) to get a bit mask
            layerMask = 1 << LayerMask.NameToLayer("GazeFocus");
        }

        private void OnEnable()
        {
            StartCoroutine(DelayedHook());
        }

        private IEnumerator DelayedHook()
        {
            yield return new WaitForSeconds(0.1f);
            Session.instance.trackedObjects.Add(this);
        }


        private void OnDisable()
        {
            Session.instance.trackedObjects.Remove(this);
        }

        private string GetFocusObjectName()
        {
            RaycastHit hit;
            Debug.DrawRay(transform.position, transform.forward);
            if (Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity, layerMask))
            {
                if (hit.transform.parent != null && hit.transform.parent.parent != null)
                {
                    return hit.transform.parent.parent.name;
                }
                else
                {
                    return hit.transform.name;
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returnns the data collected from the contactZone object
        /// </summary>
        /// <returns></returns>
        protected override UXFDataRow GetCurrentValues()
        {
            UXFDataRow data = base.GetCurrentValues();
            data.Add(("object_name", GetFocusObjectName()));
            return data;
        }
    }
}
