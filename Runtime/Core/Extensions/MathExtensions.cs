using UnityEngine;

namespace VADE.DevTools.Extensions
{
    public static class MathExtensions
    {
        public static float Remap(this float value, float fromMin, float fromMax, float toMin, float toMax)
        {
            float t = Mathf.InverseLerp(fromMin, fromMax, value);
            return Mathf.Lerp(toMin, toMax, t);
        }

        public static bool Approximately(this float a, float b, float epsilon = 0.0001f) => Mathf.Abs(a - b) <= epsilon;

        public static int RoundToNearest(this int value, int multiple)
        {
            if (multiple == 0) return value;
            return Mathf.RoundToInt(value / (float)multiple) * multiple;
        }

        public static float PercentOf(this float value, float total) => total == 0f ? 0f : value / total * 100f;
    }

    public static class VectorExtensions
    {
        public static Vector3 With(this Vector3 v, float? x = null, float? y = null, float? z = null)
            => new Vector3(x ?? v.x, y ?? v.y, z ?? v.z);

        public static Vector2 With(this Vector2 v, float? x = null, float? y = null)
            => new Vector2(x ?? v.x, y ?? v.y);

        public static Vector2 ToVector2XZ(this Vector3 v) => new Vector2(v.x, v.z);

        public static Vector3 ToVector3XZ(this Vector2 v, float y = 0f) => new Vector3(v.x, y, v.y);
    }
}
