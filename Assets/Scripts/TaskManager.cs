using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using ubc.ok.ovilab.HPUI.Core;

namespace ubc.ok.ovilab.hpuiInSituComparison.study1
{
    public class TaskManager : MonoBehaviour
    {
        // The min and max scale to set for the target
        public float minScale, maxScale;
        // At which scale values to show the display and how big is the window
        // Values in 0-1;
        [Range(0,1)]
        public float secondDisplayVisibleScaleWindow;
        [Range(0,1)]
        public float secondDisplayVisibleStartAtScale;

        public Transform workspace;
        [SerializeField]
        List<ButtonGroup> buttonGroups;

        private ButtonGroup activeButtonGroup;
        private List<Target> targets;

        public float Scale
        {
            get {
                return transform.localScale[0];
            }
            set {
                float scale = Math.Clamp(value, 0, 1);
                transform.localScale = Vector3.one * scale;
                bool visibilityState = scale > secondDisplayVisibleStartAtScale && scale < secondDisplayVisibleStartAtScale + secondDisplayVisibleScaleWindow;
                foreach (Target target in targets)
                {
                    target.SetMainAsActiveDisplayElement(!visibilityState);
                }
            }
        }

        private void Start()
        {
            targets = workspace.GetComponentsInChildren<Target>().ToList();
        }

        private void Update()
        {
            // NOTE: This is used for debugging purposes
            if (workspace.hasChanged)
            {
                Scale = workspace.localScale[0];
            }
        }

        /// <summary>
        /// Picks the first actve button group
        /// </summary>
        private void SetActiveButtonGroup()
        {
            activeButtonGroup = null;
            foreach (ButtonGroup bg in buttonGroups)
            {
                if (bg.IsActive())
                {
                    activeButtonGroup = bg;
                    break;
                }
            }
        }

        [Serializable]
        class ButtonGroup
        {
            public string name;
            public ButtonController zoomUpButton, zoomDownButton, acceptButton;
            public List<ButtonController> colorButtons;

            public bool IsActive()
            {
                // FIXME somewhere else ensure all of em have the same root and check once here?
                return
                    zoomUpButton.transform.root.gameObject.activeSelf &&
                    zoomDownButton.transform.root.gameObject.activeSelf &&
                    acceptButton.transform.root.gameObject.activeSelf &&
                    colorButtons.Aggregate(
                        true,
                        (active, current) => active && current.transform.root.gameObject.activeSelf);
            }

            public bool ButtonInGroup(ButtonController btn)
            {
                return
                    zoomUpButton == btn ||
                    zoomDownButton == btn ||
                    acceptButton == btn ||
                    colorButtons.Aggregate(
                        true,
                        (active, current) => active || current == btn);
            }
        }

    }
}
