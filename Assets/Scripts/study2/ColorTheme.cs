using System;
using System.Collections.Generic;
using UnityEngine;

namespace ubco.ovilab.hpuiInSituComparison.study2
{
    /// <summary>
    /// A simple color theme to use
    /// </summary>
    [CreateAssetMenu(fileName = "ColorTheme", menuName = "Study2/colortheme", order = 1)]
    public class ColorTheme : ScriptableObject
    {
        public List<ColorThemeData> values;
        public Color defaultColor;
    }

    [Serializable]
    public class ColorThemeData
    {
        public string stateName;
        public Color color;
    }
}
