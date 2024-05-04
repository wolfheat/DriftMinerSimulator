using System.Collections.Generic;
using UnityEngine;
public class GridVisualizer : MonoBehaviour
{
    [SerializeField] GameObject gridPointGroundPrefab;
    [SerializeField] GameObject gridPointAirPrefab;
    [SerializeField] GameObject gridPointPillarPrefab;
    [SerializeField] GameObject gridHolder;
    [SerializeField] MeshFilter meshFilter;
    [SerializeField] MeshCollider meshCollider;
    [SerializeField] int GridSize;
    [SerializeField] bool UsePillars;
    [SerializeField] private int GridShowLimit;
    [SerializeField] private int SpacingToBorder;

    Mesh mesh;

    Vector3Int[] cubes;
    List<Triangle> triangles = new List<Triangle>();
    Vector3[] vertices;

    int[][][] grid;
    int[] tris;
    Vector2[] uv;

    void Start()
    {
        SetMesh();
        if (triangles.Count==0)
            Recalculate();
    }

    private void SetMesh()
    {
        if (mesh == null)
            mesh = meshFilter.sharedMesh;
    }

    public void Carve(CarvingBox box)
    {
        // Requesting to carve out the box
        Debug.Log("Requesting to carve out the box");

        int changesMade = CarveFromGrid(box);

        if(changesMade == 0)
        {
            Debug.Log("Nothing carved from the grid, dont update");
            return;
        }

        Debug.Log("Carved away "+changesMade+" vertices");

        UpdateCubes();

        TrianglesToVertices();

        MakeMesh();



    }

    [ContextMenu("Recalculate")]
    public void Recalculate()
    {
        ClearGrid();
        CreateGrid(GridSize);
        UpdateCubes();

        TrianglesToVertices();

        MakeMesh();
    }

    [ContextMenu("Clear Mesh")]

    private void ClearMesh()
    {
        triangles.Clear();
        meshFilter.sharedMesh.Clear(false);
    }

    private void ClearGrid()
    {
        Debug.Log("Clear Grid");
        foreach (Transform t in gridHolder.GetComponentsInChildren<Transform>()) 
            if(t != gridHolder.transform) 
                DestroyImmediate(t.gameObject);
    }

    private void TrianglesToVertices()
    {
        int amtVertices = triangles.Count * 3;

        vertices = new Vector3[amtVertices];
        uv = new Vector2[amtVertices];
        tris = new int[amtVertices];

        int v = 0;
        int direction = 0;
        foreach(var t in triangles)
        {
            direction = 0;
            // If All triangles points Y position is the same - Dont use the XY plane

            // Fixes top and bottom
            //if (Equals(t.triA.y,t.triB.y) && Equals(t.triA.y, t.triC.y)) XYDirection = false;
            //if ((Equals(t.triA.x,t.triB.x) && Equals(t.triA.y, t.triC.y) )|| (Equals(t.triA.x, t.triC.x) && Equals(t.triA.y, t.triB.y))) XYDirection = false;

            // If normals are 90° to Z axis use other
            Vector3 normal = Vector3.Cross(t.triB - t.triA, t.triC - t.triA).normalized;
            float dotProduct = Vector3.Dot(normal, Vector3.forward);
            bool isPerpendicularToZ = Mathf.Abs(dotProduct) < 0.1f;
            if( isPerpendicularToZ)
            {
                float dotProductY = Vector3.Dot(normal, Vector3.right);
                bool isPerpendicularToX = Mathf.Abs(dotProductY) > 0.9f;                
                direction = isPerpendicularToX ? 2 : 1;
                if (isPerpendicularToX) Debug.Log("Triangel is perpendicular to Z and in line of X Axis: "+dotProductY);
                else Debug.Log("Triangel is perpendicular to Z" + dotProductY);
            }

            tris[v] = v;
            uv[v] = t.TriAsVector2(direction, 0);
            vertices[v++] = t.triA;
            tris[v] = v;
            uv[v] = t.TriAsVector2(direction, 1); 
            vertices[v++] = t.triB;
            tris[v] = v;
            uv[v] = t.TriAsVector2(direction, 2); 
            vertices[v++] = t.triC;
            //Debug.Log("Painting Triangle: ["+t.triA+","+t.triB+","+t.triC+"]");
        }
    }


    private void MakeMesh()
    {
        SetMesh();
        ClearMesh();
        mesh.vertices = vertices;
        mesh.triangles = tris;
        mesh.uv = uv;
        meshCollider.sharedMesh = mesh;
    }

    private void ProcessCube(Vector3Int pos)
    {

    }

    private void UpdateCubes()
    {
        foreach(var c in cubes)
        {
            Vector3[] cornPos = new Vector3[8];
            cornPos[0] = c;
            cornPos[1] = c + new Vector3(1, 0, 0);
            cornPos[2] = c + new Vector3(1, 0, 1);
            cornPos[3] = c + new Vector3(0, 0, 1);
            cornPos[4] = c + new Vector3(0, 1, 0);
            cornPos[5] = c + new Vector3(1, 1, 0); 
            cornPos[6] = c + new Vector3(1, 1, 1);
            cornPos[7] = c + new Vector3(0, 1, 1);

            // CornerCoords
            int cubeConfig = grid[c.x][c.y][c.z];
            cubeConfig |= grid[c.x+1][c.y][c.z] << 1;
            cubeConfig |= grid[c.x+1][c.y][c.z+1] << 2;
            cubeConfig |= grid[c.x][c.y][c.z+1] << 3;
            cubeConfig |= grid[c.x][c.y + 1][c.z] << 4;
            cubeConfig |= grid[c.x+1][c.y + 1][c.z] << 5;
            cubeConfig |= grid[c.x+1][c.y + 1][c.z+1] << 6;
            cubeConfig |= grid[c.x][c.y + 1][c.z+1] << 7;
            //Debug.Log("Cube config: "+ Convert.ToString(cubeConfig, 2).PadLeft(8,'0'));

            // Make triangles   
            for(int i=0; i<16; i+=3)
            {
                if (Table.LookUp[cubeConfig][i] == -1) break;

                int a0 = Table.AFromEdge[Table.LookUp[cubeConfig][i]];
                int a1 = Table.BFromEdge[Table.LookUp[cubeConfig][i]];

                int b0 = Table.AFromEdge[Table.LookUp[cubeConfig][i+1]];
                int b1 = Table.BFromEdge[Table.LookUp[cubeConfig][i+1]];

                int c0 = Table.AFromEdge[Table.LookUp[cubeConfig][i+2]];
                int c1 = Table.BFromEdge[Table.LookUp[cubeConfig][i+2]];

                Triangle tri;

                tri.triA = (cornPos[a0] + cornPos[a1]) * 0.5f;
                tri.triB = (cornPos[b0] + cornPos[b1]) * 0.5f;
                tri.triC = (cornPos[c0] + cornPos[c1]) * 0.5f;
                triangles.Add(tri);
            }

        }
    }

    private int CarveFromGrid(CarvingBox box)
    {
        
        int changes = 0;

        for (int i = Mathf.RoundToInt(box.StartBounds.x); i <= Mathf.RoundToInt(box.EndBounds.x); i++)
        {
            for (int j = Mathf.RoundToInt(box.StartBounds.y); j <= Mathf.RoundToInt(box.EndBounds.y); j++)
            {
                for (int k = Mathf.RoundToInt(box.StartBounds.z); k <= Mathf.RoundToInt(box.EndBounds.z); k++)
                {
                    grid[i][j][k] = 0;
                    changes++;
                }
            }
        }
        return changes;
    }

    private void CreateGrid(int dim)
    {
        cubes = new Vector3Int[GridSize * GridSize * GridSize]; 
        grid = new int[dim + 1][][];

        Vector3Int center = new Vector3Int(dim/2, dim / 2, dim / 2);
        for (int i = 0; i <= dim; i++)
        {
            grid[i]= new int[dim+1][];
            for (int j = 0; j <=dim ; j++)
            {
                grid[i][j] = new int[dim+1];
                for (int k = 0; k <= dim; k++)
                {
                    Vector3Int pos = new Vector3Int(i, j, k);
                    if (i != dim && j != dim && k != dim)
                        cubes[i * dim * dim + j * dim + k] = pos;
                    float dist = Vector3Int.Distance(pos, center);
                    int type = (dist <= dim/2-SpacingToBorder) ? 1 : 0;
                    grid[i][j][k] = type;

                    if(dim <= GridShowLimit)
                    {
                        GameObject gridPoint = type == 1 ? Instantiate(gridPointGroundPrefab, gridHolder.transform) : Instantiate(gridPointAirPrefab, gridHolder.transform);
                        gridPoint.transform.position = pos;
                    }

                    if (UsePillars)
                    {
                        if (j == 0){
                            if ((i == 0 || i == dim) && (k == 0 || k == dim))
                            {
                                GameObject pillar = Instantiate(gridPointPillarPrefab, gridHolder.transform);
                                pillar.transform.position = new Vector3(i,dim/2,k);
                                pillar.transform.localScale =new Vector3(pillar.transform.localScale.x, dim, pillar.transform.localScale.z);
                            }
                        }

                    }

                }
            }
        }
    }
}
