using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace HeroicEngine.Utils.Data
{
    [Serializable]
    public class ColorWrapper
    {
        [FormerlySerializedAs("r")]
        public float _r;
        [FormerlySerializedAs("g")]
        public float _g;
        [FormerlySerializedAs("b")]
        public float _b;
        [FormerlySerializedAs("a")]
        public float _a;

        public ColorWrapper()
        {
            _r = 0f;
            _g = 0f;
            _b = 0f;
            _a = 0f;
        }

        public ColorWrapper(Color color)
        {
            _r = color.r;
            _g = color.g;
            _b = color.b;
            _a = color.a;
        }

        public Color ToColor()
        {
            return new Color(_r, _g, _b, _a);
        }
    }
}