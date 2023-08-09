using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UXF;

namespace ubc.ok.ovilab.hpuiInSituComparison.study1
{
    public abstract class Slider : Tracker
    {
        public override string MeasurementDescriptor => "slider";
        public override IEnumerable<string> CustomHeader => new string[] {
            "val"
        };

        // A value between 0 and 1;
        public event System.Action<float, Slider> OnSliderEventChange;
        public new string name
        {
            get {
                return transform.name;
            }
            set {
                transform.name = value;
            }
        }

        public abstract bool inUse {get; set;}

        private float val;

        #region unity functions
        private void OnEnable()
        {
            StartCoroutine(DelayedHook());
        }

        private void Start()
        {
            this.updateType = TrackerUpdateType.Manual;
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
        #endregion

        #region slider functions
        public void _InvokeSliderEvent(float val, Slider slider)
        {
            this.val = val;
            OnSliderEventChange.Invoke(val, slider);
            this.RecordRow();
        }
        #endregion

        #region UXF functions
        protected override UXFDataRow GetCurrentValues()
        {
            UXFDataRow data = new UXFDataRow()
            {
                ("val", val)
            };

            return data;
        }
        #endregion
    }
}
