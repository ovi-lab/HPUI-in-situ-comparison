using System;
using UnityEngine;
using UnityEditor;
using ubc.ok.ovilab.ViconUnityStream;

namespace ubc.ok.ovilab.hpuiInSituComparison.study1
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
