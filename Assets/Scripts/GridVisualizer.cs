using System;
using System.Collections.Generic;
using UnityEngine;

public struct Triangle
{
    public Vector3 triA;
    public Vector3 triB;
    public Vector3 triC;

}
public class GridVisualizer : MonoBehaviour
{
    [SerializeField] GameObject gridPointGroundPrefab;
    [SerializeField] GameObject gridPointAirPrefab;
    [SerializeField] GameObject gridPointPillarPrefab;

    [SerializeField] GameObject gridHolder;


    [SerializeField] int GridSize;
    [SerializeField] bool UsePillars;

    Vector3Int[] cubes;
    int[][][] grid;
    List<Triangle> triangles = new List<Triangle>();
    Mesh mesh;
    Vector3[] vertices;
    int[] tris;
    [SerializeField] private int GridLimit;

    void Start()
    {
        if(triangles.Count==0)
            Recalculate();
    }

    [ContextMenu("Recalculate")]
    public void Recalculate()
    {
        cubes = new Vector3Int[GridSize * GridSize * GridSize];
        ClearGrid();
        CreateGrid(GridSize);
        UpdateCubes();

        TrianglesToVertices();

        MakeMesh();
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
        vertices = new Vector3[triangles.Count * 3];
        tris = new int[triangles.Count * 3];
        int v = 0;
        foreach(var t in triangles)
        {
            tris[v] = v;
            vertices[v++] = t.triA;
            tris[v] = v;
            vertices[v++] = t.triB;
            tris[v] = v;
            vertices[v++] = t.triC;
            //Debug.Log("Painting Triangle: ["+t.triA+","+t.triB+","+t.triC+"]");
        }
    }

    private void MakeMesh()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = tris;
    }

    private void ProcessCube(Vector3Int pos)
    {

    }

    private void UpdateCubes()
    {
        foreach(var c in cubes)
        {
            Debug.Log("Checking cube: "+c);

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
            Debug.Log("Cube config: "+ Convert.ToString(cubeConfig, 2).PadLeft(8,'0'));

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

    private void CreateGrid(int dim)
    {

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
                    int type = (dist <= dim/2-1) ? 1 : 0;
                    grid[i][j][k] = type;

                    if(dim <= GridLimit)
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


    void Update()
    {
                
    }
}
