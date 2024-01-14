using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;

namespace ubco.ovilab.hpuiInSituComparison.common
{
    public class ColorIndex : MonoBehaviour
    {
        public static ColorIndex instance;
        public Color defaultColor;
        public List<GroupList<Sprite>> colorSprites;

        public List<GroupList<Color>> colors;

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

#if UNITY_EDITOR
        public void GetColors()
        {
            colors = new List<GroupList<Color>>();
            Texture2D newTexture = null;
            foreach (GroupList<Sprite> sprites in colorSprites)
            {
                GroupList<Color> _colors = new GroupList<Color>();
                foreach (Sprite sprite in sprites)
                {
                    Texture2D t = sprite.texture;

                    if (newTexture == null || newTexture.width != t.width || newTexture.height != t.height)
                    {
                        newTexture = new Texture2D(t.width, t.height, t.format, t.mipmapCount, false);
                    }

                    Graphics.CopyTexture(t, newTexture);
                    _colors.Add(newTexture.GetPixel(0, 0));
                }
                colors.Add(_colors);
            }
        }
#endif

        public Color GetColor(int groupIndex, int subindex = -1)
        {
            if (groupIndex == -1 || subindex == -1)
            {
                return defaultColor;
            }
            return colors[groupIndex][subindex];
        }

        public Sprite GetSprite(int groupIndex, int subindex)
        {
            return colorSprites[groupIndex][subindex];
        }

        public Color GetRandomColor(System.Random random)
        {
            GroupList<Color> c = colors[random.Next(colors.Count)];
            return c[random.Next(c.Count)];
        }

        public int Count(int groupIndex = -1)
        {
            if (groupIndex < 0)
            {
                return colors.Count;
            }
            return colors[groupIndex].Count;
        }
    }

    [Serializable]
    public class GroupList<T>: IEnumerable<T>
    {
        public List<T> l = new List<T>();

        public int Count
        {
            get => l.Count;
            private set { }
        }

        public void Add(T el)
        {
            l.Add(el);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return l.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return l.GetEnumerator();
        }

        public T this[int index]
        {
            get => l[index];
            set => l[index] = value;
        }
    }
}
