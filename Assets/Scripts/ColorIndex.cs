using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ubc.ok.ovilab.hpuiInSituComparison.study1
{
    public class ColorIndex : MonoBehaviour
    {
        public static ColorIndex instance;
        public List<Color> colors;
        public List<Sprite> colorSprites;

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

        public Sprite GetSprite(int index)
        {
            return colorSprites[index];
        }

        public Color GetRandomColor(System.Random random)
        {
            return colors[random.Next(colors.Count)];
        }

        public int Count()
        {
            return colors.Count;
        }
    }
}
