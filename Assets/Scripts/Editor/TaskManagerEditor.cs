using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ubco.ovilab.ViconUnityStream;

namespace ubco.ovilab.hpuiInSituComparison.study1
{
    [CustomEditor(typeof(TaskManager), true)]
    public class TaskManagerEditor: UnityEditor.Editor
    {
        TaskManager t;

        void OnEnable()
        {
            t = target as TaskManager;
        }
        
        public override void OnInspectorGUI()
        {
            base.DrawDefaultInspector();
            GUI.enabled = EditorApplication.isPlaying;
            if (GUILayout.Button("Peg in the hole"))
            {
                t.PegInTheHole();
            }
            if (GUILayout.Button("Select correct color"))
            {
                t.SetCorrectColor();
            }
            if (GUILayout.Button("Set correct scale"))
            {
                t.SetCorrectScale();
            }
            if (GUILayout.Button("accept"))
            {
                t.AcceptCorrect();
            }
            GUILayout.Space(10);
            if (GUILayout.Button("Complete step"))
            {
                t.CompleteStep();
            }
        }
    }
}
