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
        private bool active;

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

        public bool Visible
        {
            get {
                return this.gameObject.activeSelf;
            }
            set {
                this.gameObject.SetActive(value);
            }
        }

        public bool Active
        {
            get {
                return active;
            }
            set {
                active = value;
                if (!active)
                {
                    DisplayColorIndex = -1;
                }
            }
        }

        private void Start()
        {
            linker = GetComponent<TransformLinker>();
            trackingObject = null; // trigger activation
            displayRenderer = GetComponentInChildren<Renderer>();
        }
    }
}
