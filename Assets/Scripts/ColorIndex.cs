using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ubc.ok.ovilab.hpuiInSituComparison.study1
{
    public class ColorIndex : MonoBehaviour
    {
        public static ColorIndex instance;
        public List<Color> colors;

        private void OnEnable()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Debug.LogError($"There are more than one ColorIndex's active in the scene");
            }
        }

        private void OnDisable()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        public Color GetColor(int index)
        {
            return colors[index];
        }
    }
}
