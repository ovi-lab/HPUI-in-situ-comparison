using UnityEditor;
using UnityEngine;

namespace ubco.ovilab.hpuiInSituComparison.study2
{
    [CustomEditor(typeof(WindowManager), true)]
    public class WindowManagerEditor: UnityEditor.Editor
    {
        WindowManager t;
        float scale;

        protected virtual void OnEnable()
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
            GUI.enabled = true;
        }
    }
}
