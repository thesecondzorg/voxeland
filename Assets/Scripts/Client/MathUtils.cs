using UnityEngine;

namespace Client
{
    public static class MathUtils
    {
        public static Vector3Int Floor(this Vector3 point)
        {
            return new Vector3Int(Mathf.FloorToInt(point.x), Mathf.FloorToInt(point.y), Mathf.FloorToInt(point.z));
        }
    }
}