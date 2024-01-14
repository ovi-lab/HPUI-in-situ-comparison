using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UXF;
using ubco.ovilab.HPUI.Legacy.DeformableSurfaceDisplay;
using ubco.ovilab.HPUI.Legacy;

namespace ubco.ovilab.hpuiInSituComparison.study1
{
    public class Slider : Tracker
    {
        public override string MeasurementDescriptor => "slider";
        public override IEnumerable<string> CustomHeader => new string[] {
            "val"
        };

        public bool reverseColor = false;
        public Color defaultColor, highlightColor;

        // A value between 0 and 1;
        public event System.Action<float, Slider> OnSliderEventChange;

        public virtual bool inUse
        {
            get {
                return displayManager.inUse;
            }
            set {
                displayManager.inUse = value;
            }
        }
        public new string name
        {
            get {
                return transform.name;
            }
            set {
                transform.name = value;
            }
        }

        protected DeformableSurfaceDisplayManager displayManager;
        private float val;
        private float range;

        #region unity functions
        private void OnEnable()
        {
            StartCoroutine(DelayedHook());
        }

        protected virtual void Start()
        {
            this.updateType = TrackerUpdateType.Manual;
            displayManager = GetComponentInParent<DeformableSurfaceDisplayManager>();
            displayManager.SurfaceReadyAction.AddListener(OnSurfaceReady);
            displayManager.currentCoord.OnStateChanged += OnCoordStateChanged;
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
        private void OnSurfaceReady()
        {
            range = displayManager.currentCoord.maxY;
        }

        public void _InvokeSliderEvent(float val, Slider slider)
        {
            this.val = val;
            OnSliderEventChange?.Invoke(val, slider);
            this.RecordRow();
        }

        public void SetSliderValue(float val)
        {
            int y = (int) Mathf.Round(Mathf.Clamp(val, 0, 1) * range);
            OnCoordStateChanged(0, y);
        }

        private void OnCoordStateChanged(int x, int y)
        {
            foreach(var otherBtn in displayManager.buttonControllers)
            {
                int _x, _y;
                displayManager.idToXY(otherBtn.id, out _x, out _y);
                // Just being clear about whats happening here
                if (!reverseColor ? _y >= y : _y <= y)
                {
                    otherBtn.GetComponent<ButtonColorBehaviour>().DefaultColor = defaultColor;
                }
                else
                {
                    otherBtn.GetComponent<ButtonColorBehaviour>().DefaultColor = highlightColor;
                }
                otherBtn.InvokeDefault();
            }
            _InvokeSliderEvent(y / range, this);
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
