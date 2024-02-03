using UnityEditor;
using UnityEngine;

namespace ubco.ovilab.hpuiInSituComparison.study2
{
    [CustomEditor(typeof(SwipeWindowManager), true)]
    public class SwipeWindowManagerEditor: WindowManagerEditor
    {
        SwipeWindowManager t;
        float scale;

        protected override void OnEnable()
        {
            base.OnEnable();
            t = target as SwipeWindowManager;
        }
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GUI.enabled = EditorApplication.isPlaying;
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
