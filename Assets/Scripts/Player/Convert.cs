using System;
using UnityEngine;

namespace Wolfheat
{
    public static class Convert
    {
        public const float GridSize = 1f;
        public static Vector3 Align(this Vector3 pos) => new Vector3(Mathf.RoundToInt(pos.x / GridSize) * GridSize, Mathf.RoundToInt(pos.y / GridSize) * GridSize, Mathf.RoundToInt(pos.z / GridSize) * GridSize);

        public static Vector3 AlignCardinal(Vector3 dir)
        {
            if (Math.Abs(dir.x) >= Math.Abs(dir.y) && Math.Abs(dir.x) >= Math.Abs(dir.z)) return new Vector3(dir.x > 0 ? 1 : -1, 0, 0);
            if (Math.Abs(dir.y) >= Math.Abs(dir.x) && Math.Abs(dir.y) >= Math.Abs(dir.z)) return new Vector3(0, dir.y > 0 ? 1 : -1, 0);
            return new Vector3(0,0,dir.z > 0 ? 1 : -1);
        }

        public static Vector3 Away(Vector3 dir)
        {
            Debug.Log("Away = "+dir+ " (Math.Abs(dir.x) >= Math.Abs(dir.z)) "+ (Math.Abs(dir.x) >= Math.Abs(dir.z)));
            if (Math.Abs(dir.x) >= Math.Abs(dir.z)) return new Vector3(0, 0, -dir.z).normalized;
            return new Vector3(-dir.x, 0, 0).normalized;
        }
        public static Vector3 VeticalAway(Vector3 dir) => dir.y <= 0 ? Vector3.up : Vector3.down;
        public static Vector3 PlaneTowards(Vector3 dir) => (Math.Abs(dir.x) >= Math.Abs(dir.z)) ? new Vector3(dir.x != 0 ? dir.x:1,0,0) : new Vector3(0, 0, dir.z != 0 ? dir.z : 1);
    }
}
