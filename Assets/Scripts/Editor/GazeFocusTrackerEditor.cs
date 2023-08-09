using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UXF;
using ubc.ok.ovilab.ViconUnityStream;

namespace ubc.ok.ovilab.hpuiInSituComparison.study1
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(GazeFocusTracker), true)]
    public class GazeFocusTrackerEditor: UnityEditor.Editor
    {
        GazeFocusTracker t;
        Transform targetTransform;

        void OnEnable()
        {
            t = target as GazeFocusTracker;
        }
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            targetTransform = (Transform)EditorGUILayout.ObjectField("Target Transform", targetTransform, typeof(Transform), true);
            GUI.enabled = EditorApplication.isPlaying;
            if (GUILayout.Button("Look at transform") && targetTransform != null)
            {
                t.transform.rotation = Quaternion.LookRotation(targetTransform.position - t.transform.position);
            }
        }
    }
}
