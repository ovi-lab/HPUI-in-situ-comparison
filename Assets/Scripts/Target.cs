using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ubco.ovilab.hpuiInSituComparison.study1
{
    public class Target : MonoBehaviour
    {
        public Renderer mainDisplayElement;
        public Renderer secondDisplayElement;

        public UnityEvent<Target> onSelectionStart;
        public UnityEvent<Target> onSelectionEnd;

        public int DisplayColorIndex
        {
            get {
                return colorIndex;
            }
            set {
                Color _color = ColorIndex.instance.GetColor(DisplayColorGroupIndex, value);
                colorIndex = value;
                mainDisplayElement.material.SetColor("_Color", _color);
                secondDisplayElement.material.SetColor("_Color", _color);
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
                    DisplayColorGroupIndex = -1;
                    DisplayColorIndex = -1;
                }
            }
        }

        public Vector3 Position
        {
            get {
                return transform.localPosition;
            }
            set {
                transform.localPosition = value;
            }
        }

        public bool IsSelected {
            get {
                return isSelected;
            }
            private set
            {
                isSelected = value;
                foreach (Outline outline in outlines)
                {
                    outline.enabled = value;
                }
            }
        }

        private int colorIndex;
        private bool active;
        private Collider _collider;
        private bool isSelected;
        private Outline[] outlines;

        private void Start()
        {
            _collider = GetComponent<Collider>();
            outlines = GetComponentsInChildren<Outline>();
        }

        // Main is the cylinder, second is the pipe
        public void SetMainAsActiveDisplayElement(bool value)
        {
            if (value)
            {
                if (secondDisplayElement.gameObject.activeSelf)
                {
                    _collider.enabled = false;
                    IsSelected = false;
                    secondDisplayElement.gameObject.SetActive(false);
                }
                if (!mainDisplayElement.gameObject.activeSelf)
                {
                    mainDisplayElement.gameObject.SetActive(true);
                }
            }
            else
            {
                if (!secondDisplayElement.gameObject.activeSelf)
                {
                    _collider.enabled = true;
                    IsSelected = false;
                    secondDisplayElement.gameObject.SetActive(true);
                }
                if (mainDisplayElement.gameObject.activeSelf)
                {
                    mainDisplayElement.gameObject.SetActive(false);
                }

            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<Peg>() != null)
            {
                IsSelected = true;
                onSelectionStart?.Invoke(this);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.GetComponent<Peg>() != null)
            {
                IsSelected = false;
                onSelectionEnd?.Invoke(this);
            }
        }
    }

}
