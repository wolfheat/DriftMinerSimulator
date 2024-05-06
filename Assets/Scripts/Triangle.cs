using UnityEngine;

public struct Triangle
{
    public Vector3 triA;
    public Vector3 triB;
    public Vector3 triC;

    public Vector2 GetTriUVMapping(int dir, int point)
    {
        Vector3 tri;
        switch (point)
        {
            case 0:
                tri = triA;
                break;
            case 1:
                tri = triB;
                break;
            case 2:
                tri = triC;
                break;
            default: 
                tri = triA;
                break;
        }



        switch (dir)
        {
            case 0:
                return new Vector2(tri.x, tri.y);
            case 1:
                return new Vector2(tri.x, tri.z);
            case 2:
                return new Vector2(tri.z, tri.y);
            default: 
                return new Vector2(tri.x, tri.y);
        }

    }

}
