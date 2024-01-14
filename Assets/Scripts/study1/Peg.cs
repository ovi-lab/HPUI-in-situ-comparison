using UnityEngine;
using ubco.ovilab.HPUI.Legacy;
using ubco.ovilab.hpuiInSituComparison.common;

namespace ubco.ovilab.hpuiInSituComparison.study1
{
    public class Peg : MonoBehaviour, IPeg
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
                if (displayRenderer == null)
                {
                    displayRenderer = GetComponentInChildren<Renderer>();
                }
                Color _color = ColorIndex.instance.GetColor(DisplayColorGroupIndex, value);
                colorIndex = value;
                displayRenderer.material.SetColor("_Color", _color);
            }
        }

        public int DisplayColorGroupIndex { get; set; }

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

        public float Scale
        {
            get {
                return transform.localScale[0];
            }
            set {
                Vector3 scale = transform.localScale;
                scale.x = value * scale.y;
                scale.z = value * scale.y;
                transform.localScale = scale;
            }
        }

        private void Start()
        {
            linker = GetComponent<TransformLinker>();
            trackingObject = null; // trigger activation
        }
    }
}
