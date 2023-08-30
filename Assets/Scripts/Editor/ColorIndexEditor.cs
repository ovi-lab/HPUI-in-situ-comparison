using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ubc.ok.ovilab.ViconUnityStream;

namespace ubc.ok.ovilab.hpuiInSituComparison.study1
{
    [CustomEditor(typeof(ColorIndex), true)]
    public class ColorIndexEditor: UnityEditor.Editor
    {
        ColorIndex t;

        void OnEnable()
        {
            t = target as ColorIndex;
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            base.DrawDefaultInspector();
            if (EditorGUI.EndChangeCheck())
            {
                t.GetColors();
                EditorUtility.SetDirty(t);
            }

            if (GUILayout.Button("Update colors from sprites"))
            {
                t.GetColors();
                EditorUtility.SetDirty(t);
            }
        }
    }
}
