using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

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

        private List<Target> targets;

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


    }

}
