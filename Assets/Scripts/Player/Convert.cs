using UnityEngine;

namespace Wolfheat
{
    public static class Convert
    {
        public const float GridSize = 1f;
        public static Vector3 Align(this Vector3 pos) => new Vector3(Mathf.RoundToInt(pos.x / GridSize) * GridSize, Mathf.RoundToInt(pos.y / GridSize) * GridSize, Mathf.RoundToInt(pos.z / GridSize) * GridSize);
    }
}
