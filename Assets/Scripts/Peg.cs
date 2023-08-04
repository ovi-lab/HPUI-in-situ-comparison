using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ubc.ok.ovilab.HPUI.Core;

namespace ubc.ok.ovilab.hpuiInSituComparison.study1
{
    public class Peg : MonoBehaviour
    {
        private Renderer displayRenderer;
        private TransformLinker linker;
        private int colorIndex;

        public Transform trackingObject
        {
            get {
                return linker.parent;
            }
            set {
                linker.enabled = value != null;
                linker.parent = value;
            }
        }

        public int DisplayColorIndex
        {
            get {
                return colorIndex;
            }
            set {
                Color _color = ColorIndex.instance.GetColor(value);
                colorIndex = value;
                displayRenderer.material.SetColor("_Color", _color);
            }
        }

        private void Start()
        {
            trackingObject = trackingObject; // trigger activation
            displayRenderer = GetComponentInChildren<Renderer>();
        }
    }
}
