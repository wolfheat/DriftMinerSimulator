using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public enum CubeType {DefaultFilled, Cube, Sphere,Pyramid}

[ExecuteInEditMode]
public class Chunk : MonoBehaviour
{
    [SerializeField] CubeType cubetype = CubeType.Cube;
    
    [Header("Size Definition")]
    public int GridSize;
    [SerializeField] float GridScale = 0.5f;

    [Header("Mesh Stuff")]
    [SerializeField] MeshFilter meshFilter;
    [SerializeField] MeshCollider meshCollider;
    
    [Header("Additional visualization")]
    [SerializeField] GameObject gridPointGroundPrefab;
    [SerializeField] GameObject gridPointAirPrefab;
    [SerializeField] GameObject gridPointBorrowPrefab;

    [Tooltip("Show visualization when this value is above Grid Size")]
    [SerializeField] private int GridShowLimit;

    public float GridScaling => GridScale;
    public float ScaleDown => 0.5f * GridScale;

    public Vector3Int GridIndex { get; set;}

    Mesh mesh;

    Vector3Int[] cubes;
    List<Triangle> triangles = new List<Triangle>();
    Vector3[] vertices;
    Vector2[] uv;

    int[][][] grid;
    int[] tris;




    void Awake()
    {
        SetMesh();
        StartCoroutine(InitiateDelayed());
    }

    private IEnumerator InitiateDelayed()
    {
        // Added this so I can set the Grid Index before creating the mesh
        yield return null;
        yield return null;
        if (triangles.Count == 0)
            Recalculate();
    }

    public int GetGridValue(Vector3Int pos) => grid[pos.x][pos.y][pos.z];

    public void SetGridValue(Vector3Int pos, int val)
    {
        grid[pos.x][pos.y][pos.z] = val;
        markedForUpdate = true;
    }

    // Update after changed values
    bool markedForUpdate = false;
    private void FixedUpdate()
    {
        if (markedForUpdate)
        {
            CompleteMesh();
            markedForUpdate = false;
        }
    }

    [ContextMenu("Set Mesh")]
    private void SetMesh() => mesh = meshFilter.sharedMesh = new Mesh();


    public void Carve(Vector3Int start, Vector3Int end)
    {
        Debug.Log("Carve pixels "+start+" to "+end);




        // Carve out the box
        if (CarveFromGrid(start,end) == 0)
            return;
        CompleteMesh();
    }

    public void Carve(CarvingBox box)
    {
        // Initial Carving process - happens at the Raycasted Chunk
        // - recived the entire bounding box here and it separates into different boxes for different chunks
        Vector3 StartBounds = box.StartBounds - transform.position;
        Vector3 EndBounds = box.EndBounds - transform.position;

        Vector3Int StartBoundsInt = Vector3Int.RoundToInt(StartBounds / box.SetScale);
        Vector3Int EndBoundsInt = Vector3Int.RoundToInt(EndBounds / box.SetScale);

        // For the active chunk only carve pixels inside chunk
        Vector3Int ChunkStartBoundsInt = StartBoundsInt;
        ChunkStartBoundsInt.Clamp(Vector3Int.zero, Vector3Int.one * GridSize);

        Vector3Int ChunkEndBoundsInt = Vector3Int.RoundToInt(EndBounds / box.SetScale);
        ChunkEndBoundsInt.Clamp(Vector3Int.zero, Vector3Int.one * GridSize);

        // Send Carve command to all neighbor Chunks
        ChunkGridSpawner.NotifyCarveNeighbors(StartBoundsInt, EndBoundsInt, GridIndex);

        // Carve this Chunk
        Carve(StartBoundsInt, EndBoundsInt);

        CompleteMesh();
    }

    private void CompleteMesh()
    {
        CubesToTriangles();
        TrianglesToVertices();
        FinalizeMesh();
    }

    [ContextMenu("Recalculate")]
    public void Recalculate()
    {
        CreateGrid(GridSize);
        CompleteMesh();
    }

    [ContextMenu("Clear Mesh")]
    private void ClearMesh()
    {
        triangles.Clear();
        meshFilter.sharedMesh.Clear(false);
    }

    private void TrianglesToVertices()
    {
        int amtVertices = triangles.Count * 3;

        vertices = new Vector3[amtVertices];
        uv = new Vector2[amtVertices];
        tris = new int[amtVertices];

        int v = 0;
        // UV direction is used to make UV mapping better, still not calculating from normals due to it being more costly
        int UVdirection;



        foreach(var t in triangles)
        {
            UVdirection = 0;
            
            // If normals are 90° to Z axis use other
            Vector3 normal = Vector3.Cross(t.triB - t.triA, t.triC - t.triA).normalized;
            
            // Check if normal is perpendicular to Z axis
            bool isPerpendicularToZ = Mathf.Abs(Vector3.Dot(normal, Vector3.forward)) < 0.1f;
            //bool isPerpendicularToZ = Mathf.Approximately(Vector3.Dot(normal, Vector3.forward), 0);

            if (isPerpendicularToZ)
            {
                // Check if normal is perpendicular to X axis
                bool isPerpendicularToX = Mathf.Abs(Vector3.Dot(normal, Vector3.right)) > 0.9f;
                //bool isPerpendicularToX = Mathf.Approximately(Vector3.Dot(normal, Vector3.right), 1);

                // Set UV direction based on perpendicularity
                UVdirection = isPerpendicularToX ? 2 : 1;
            }

            // Define each triangle
            for (int i = 0; i < 3; i++)
            {
                tris[v] = v;
                uv[v] = t.GetTriUVMapping(UVdirection, i);
                vertices[v++] = i == 0 ? t.triA : (i == 1 ? t.triB : t.triC);
            }
        }

        foreach (var t in triangles)
        {
            UVdirection = 0;

            // Calculate the normal once
            Vector3 triBminusA = t.triB - t.triA;
            Vector3 triCminusA = t.triC - t.triA;
            Vector3 normal = Vector3.Cross(triBminusA, triCminusA).normalized;
        }
    }

    private void FinalizeMesh()
    {
        SetMesh();
        ClearMesh();
        mesh.vertices = vertices;
        mesh.triangles = tris;
        mesh.uv = uv;
        meshCollider.sharedMesh = mesh;
    }


    private void CubesToTriangles()
    {
        foreach (var c in cubes)
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

            // Read all points for this cube and leftshift to form the bitmask
            int lookUpBitmask = grid[c.x][c.y][c.z];
            lookUpBitmask |= grid[c.x+1][c.y][c.z] << 1;
            lookUpBitmask |= grid[c.x+1][c.y][c.z+1] << 2;
            lookUpBitmask |= grid[c.x][c.y][c.z+1] << 3;
            lookUpBitmask |= grid[c.x][c.y + 1][c.z] << 4;
            lookUpBitmask |= grid[c.x+1][c.y + 1][c.z] << 5;
            lookUpBitmask |= grid[c.x+1][c.y + 1][c.z+1] << 6;
            lookUpBitmask |= grid[c.x][c.y + 1][c.z+1] << 7;

            // Make triangles   
            for(int i=0; i<16; i+=3)
            {
                // -1 defines end of readable data
                if (Table.LookUp[lookUpBitmask][i] == -1) break;

                int a0 = Table.AFromEdge[Table.LookUp[lookUpBitmask][i]];
                int a1 = Table.BFromEdge[Table.LookUp[lookUpBitmask][i]];

                int b0 = Table.AFromEdge[Table.LookUp[lookUpBitmask][i+1]];
                int b1 = Table.BFromEdge[Table.LookUp[lookUpBitmask][i+1]];

                int c0 = Table.AFromEdge[Table.LookUp[lookUpBitmask][i+2]];
                int c1 = Table.BFromEdge[Table.LookUp[lookUpBitmask][i+2]];

                // The triangle is placed in world coordinates
                Triangle tri;
                tri.triA = (cornPos[a0] + cornPos[a1]) * ScaleDown;
                tri.triB = (cornPos[b0] + cornPos[b1]) * ScaleDown;
                tri.triC = (cornPos[c0] + cornPos[c1]) * ScaleDown;

                triangles.Add(tri);
            }

        }
    }

    public void UpdateFromDependingNeighbors()
    {
        // Update all neighbor points
        // Only dependant on lower IDs

        for (int i = 0; i <= GridSize; i++)
        {
            for (int j = 0; j <= GridSize; j++)
            {
                for (int k = 0; k <= GridSize; k++)
                {
                    if (i == 0 || j == 0 || k == 0)
                    {
                        Vector3Int offset = Vector3Int.zero;

                        if (i == 0)
                            offset.x = -1;
                        if (j == 0)
                            offset.y = -1;
                        if (k == 0)
                            offset.z = -1;

                        Vector3Int gridIndex = GridIndex + offset;
                        grid[i][j][k] = ChunkGridSpawner.GetPixelAt(gridIndex, new Vector3Int(i, j, k));
                        //if(k==0)
                            //Debug.Log("Updated pixel at "+i+","+j + "," + k + "," + " for "+ GridIndex + " to "+ grid[i][j][k]);
                    }
                }
            }
        }
        CompleteMesh();
    }

    private int CarveFromGrid(Vector3Int StartBoundsInt, Vector3Int EndBoundsInt)
    {
        // When carving at a border
        // Carve different boxes on all affected chunks 
        // Update affected Chunks in correct order

        Debug.Log("Carving Box "+StartBoundsInt+" to "+EndBoundsInt);

        int changes = 0;
        for (int i = StartBoundsInt.x; i <= EndBoundsInt.x; i++)
        {
            for (int j = StartBoundsInt.y; j <= EndBoundsInt.y; j++)
            {
                for (int k = StartBoundsInt.z; k <= EndBoundsInt.z; k++)
                {
                    // Used to still set the 0 values
                    if (i < 0 || i >= grid.Length || j < 0 || j >= grid[i].Length || k < 0 || k >= grid[i][j].Length)
                    {
                        //Debug.LogWarning("Carving outside Chunk, should never happen");
                        continue;
                    }
                    
                    //if (i <= 0 || i >= grid.Length || j <= 0 || j >= grid[i].Length || k <= 0 || k >= grid[i][j].Length) continue;

                    grid[i][j][k] = 0;

                    // If hitting 0 side carv from neighbor?

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

        Vector3Int pos;
        for (int i = 0; i <= dim; i++)
        {
            grid[i]= new int[dim+1][];
            for (int j = 0; j <=dim ; j++)
            {
                grid[i][j] = new int[dim+1];
                for (int k = 0; k <= dim; k++)
                {
                    pos = new Vector3Int(i, j, k);
                    if (i != dim && j != dim && k != dim)
                        cubes[i * dim * dim + j * dim + k] = pos;

                    int type = GetType(i,j,k);
                                        
                    grid[i][j][k] = type;

                    // Only to show grid points
                    if(dim <= GridShowLimit)
                    {   
                        GameObject gridPoint;
                        if (i == 0 || j == 0 || k == 0)
                        {
                            //gridPoint = Instantiate(gridPointBorrowPrefab, transform);
                        }
                        else
                        {
                            gridPoint = type == 1 ? Instantiate(gridPointGroundPrefab, transform) : Instantiate(gridPointAirPrefab, transform);
                            gridPoint.transform.localPosition = (Vector3)pos*GridScale;
                        }
                    }

                }
            }
        }

        int GetType(int i, int j, int k)
        {
            switch (cubetype)
            {
                case CubeType.Cube:
                    return (i == 0 || j == 0 || k == 0 || i == dim || j == dim || k == dim) ? 0 : 1;
                case CubeType.Sphere:
                    float dist = Vector3Int.Distance(pos, new Vector3Int(dim / 2, dim / 2, dim / 2));
                    return (dist <= dim / 2 - 1) ? 1 : 0;
                case CubeType.Pyramid:
                    break;
                case CubeType.DefaultFilled:
                    if (i == 0 || j == 0 || k == 0)
                    {

                        // Read from other blocks
                        Vector3Int offset = Vector3Int.zero;

                        if (i == 0)
                            offset.x = -1;
                        if (j == 0)
                            offset.y = -1;
                        if (k == 0)
                            offset.z = -1;
                        Vector3Int gridIndex = GridIndex + offset;
                        return ChunkGridSpawner.GetPixelAt(gridIndex, new Vector3Int(i, j, k));
                    }
                    else if (i == dim || j == dim || k == dim)
                    {
                        Vector3Int offset = Vector3Int.zero;
                        if (i == dim)
                            offset.x = 1;
                        if (j == dim)
                            offset.y = 1;
                        if (k == dim)
                            offset.z = 1;
                        Vector3Int gridIndex = GridIndex + offset;
                        return ChunkGridSpawner.InsideGrid(gridIndex) ? 1 : 0;
                    }
                    return 1;

            }
            return 0;
        }

    }
}
