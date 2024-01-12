using System;
using UnityEngine;
using UnityEditor;
using ubco.ovilab.ViconUnityStream;

namespace ubco.ovilab.hpuiInSituComparison.study1
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Slider), true)]
    public class SliderEditor: UnityEditor.Editor
    {
        Slider t;
        float scale;

        void OnEnable()
        {
            t = target as Slider;
        }
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Slider value");

            float newval = EditorGUILayout.Slider(scale, 0, 1);
            if (newval != scale)
            {
                scale = newval;
                t._InvokeSliderEvent(scale, t);
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
