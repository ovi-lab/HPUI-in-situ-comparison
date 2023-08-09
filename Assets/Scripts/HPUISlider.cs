using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ubc.ok.ovilab.HPUI.Core;
using ubc.ok.ovilab.HPUI.Core.DeformableSurfaceDisplay;
using ubc.ok.ovilab.HPUI.Utils;

namespace ubc.ok.ovilab.hpuiInSituComparison.study1
{
    public class HPUISlider : Slider
    {
        public DeformableSurfaceDisplayManager deformableSurfaceDisplayManager;
        public Color defaultColor, highlightColor;

        public override bool inUse
        {
            get {
                return deformableSurfaceDisplayManager.inUse;
            }
            set {
                deformableSurfaceDisplayManager.inUse = value;
            }
        }
        private float range;

        private void Start()
        {
            deformableSurfaceDisplayManager.SurfaceReadyAction.AddListener(OnSurfaceReady);
            deformableSurfaceDisplayManager.currentCoord.OnStateChanged += OnCoordStateChanged;
        }

        private void OnSurfaceReady()
        {
            range = deformableSurfaceDisplayManager.currentCoord.maxY;
        }

        private void OnCoordStateChanged(int x, int y)
        {
            foreach(var otherBtn in deformableSurfaceDisplayManager.buttonControllers)
            {
                int _x, _y;
                deformableSurfaceDisplayManager.idToXY(otherBtn.id, out _x, out _y);
                if (_y >= y)
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
    }
}
