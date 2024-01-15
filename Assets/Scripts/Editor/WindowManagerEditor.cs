using System;
using System.Collections.Generic;
using System.Linq;
using ubco.ovilab.hpuiInSituComparison.common;
using UnityEditor;
using UnityEngine;

namespace ubco.ovilab.hpuiInSituComparison.study2
{
    [CustomEditor(typeof(WindowManager), true)]
    public class WindowManagerEditor: UnityEditor.Editor
    {
        WindowManager t;
        float scale;

        void OnEnable()
        {
            t = target as WindowManager;
        }
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GUI.enabled = EditorApplication.isPlaying;
            if (GUILayout.Button("Setup frames"))
            {
                t.SetupFrames();
            }
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Shift left"))
            {
                t.ShiftWindowsLeft();
            }
            if (GUILayout.Button("Shift right"))
            {
                t.ShiftWindowsRight();
            }
            EditorGUILayout.EndHorizontal();
            GUI.enabled = true;
        }
    }
}
