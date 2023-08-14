using System.Collections;
using System.Collections.Generic;
using ubc.ok.ovilab.HPUI.Core;
using UnityEngine;

namespace ubc.ok.ovilab.hpuiInSituComparison.study1
{
    public class MoveRange : MonoBehaviour
    {
        public InSituExperimentManager experimentManager;
        public SpriteRenderer sprite;

        public bool inRange {
            get {
                return _inRange;
            }
            set {
                _inRange = value;
                Material m = GetComponent<MeshRenderer>().material;
                if (value)
                {
                    Color _color = experimentManager.defaultHoverColor;
                    _color.a = 0.3f;
                    m.color = _color;

                    sprite.color = experimentManager.defaultHoverColor;
                }
                else
                {
                    Color _color = Color.white;
                    _color.a = 0.0f;
                    m.color = _color;

                    sprite.color = Color.white;
                }
            }
        }

        private bool _inRange = false;

        private void OnEnable()
        {
            inRange = false;
        }

        private void OnDisable()
        {
            inRange = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<ButtonTriggerCollider>() != null)
            {
                inRange = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.GetComponent<ButtonTriggerCollider>() != null)
            {
                inRange = false;
            }
        }

        public void SetSelected(bool selected)
        {
            Material m = GetComponent<MeshRenderer>().material;
            if (selected)
            {
                Color _color = experimentManager.defaultHighlightColor;
                _color.a = 0.3f;
                m.color = _color;

                sprite.color = experimentManager.defaultHighlightColor;
            }
            else
            {
                // trigger correct color change
                inRange = inRange;
            }
        }
    }
}
