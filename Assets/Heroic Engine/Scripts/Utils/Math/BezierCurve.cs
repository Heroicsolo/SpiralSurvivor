using UnityEngine;

namespace HeroicEngine.Utils.Math
{
    public static class BezierCurve
    {
        private static readonly float[] _factorial = new float[]
        {
            1.0f, 1.0f, 2.0f, 6.0f
        };

        private static float Binomial(int n, int i)
        {
            var a1 = _factorial[n];
            var a2 = _factorial[i];
            var a3 = _factorial[n - i];
            var ni = a1 / (a2 * a3);
            return ni;
        }

        private static float Bernstein(int n, int i, float t)
        {
            var t_i = Mathf.Pow(t, i);
            var t_n_minus_i = Mathf.Pow(1 - t, n - i);

            var basis = Binomial(n, i) * t_i * t_n_minus_i;
            return basis;
        }

        public static Vector3 GetCurvedPosition(float t, Vector3 startPos, Vector3 midPos, Vector3 endPos)
        {
            switch (t)
            {
                case <= 0:
                    return startPos;
                case >= 1:
                    return endPos;
            }

            var p = new Vector3();

            var bn = Bernstein(2, 0, t) * startPos;
            p += bn;
            bn = Bernstein(2, 1, t) * midPos;
            p += bn;
            bn = Bernstein(2, 2, t) * endPos;
            p += bn;

            return p;
        }
    }
}
