using UnityEngine;
using UnityEditor;

namespace ubco.ovilab.hpuiInSituComparison.common
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
