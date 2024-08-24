using System;
using UnityEngine;

public class ChunkGridSpawner : MonoBehaviour
{
    private const int GroundLevel = 7;
    [SerializeField] int Size = 2;
    [SerializeField] int Height = 2;
    [SerializeField] Chunk chunkPrefab;
    [SerializeField] Chunk chunkPrefabB;
    static Chunk chunkPrefabStatic;
    [SerializeField] GameObject chunkH;

    public static Chunk[][][] grid;

    void Start()
    {
        chunkPrefabStatic = chunkPrefab;
        CreateGrid();
    }
    
    public static bool InsideGrid(Vector3Int chunkID)
    {
        if(chunkID.x<0 || chunkID.y <0 || chunkID.z <0 || chunkID.x >= grid.Length || chunkID.y >= grid[0].Length ||  chunkID.z >= grid[0][0].Length)
            return false;
        return true;
    }

    public static int GetPixelAt(Vector3Int chunkID, Vector3Int pixelPos)
    {
        // Outside grid
        if(chunkID.x<0 || chunkID.y <0 || chunkID.z <0 || chunkID.x >= grid.Length || chunkID.y >= grid[0].Length ||  chunkID.z >= grid[0][0].Length)
        {
            // Return 1 here to have the outside count as Solid or 0 to count as Air

            // return 0 if above groundLevel

            if(chunkID.y>= GroundLevel)
                return 0;
            return 1;

        }

        // Inside - move to other side
        if(pixelPos.x == 0)
            pixelPos.x += chunkPrefabStatic.GridSize;
        if(pixelPos.y == 0)
            pixelPos.y += chunkPrefabStatic.GridSize;
        if(pixelPos.z == 0)
            pixelPos.z += chunkPrefabStatic.GridSize    ;

        //if (chunkID.x == 0 && chunkID.y == 0 && chunkID.z == 0)
            //Debug.Log("Reading chunk [0,0,0]: pixelPos =[" +pixelPos +"]");


        // Return the pixel
        return grid[chunkID.x][chunkID.y][chunkID.z].GetGridValue(pixelPos);
    }
    
    public static void SetPixelAt(Vector3Int chunkID, Vector3Int pixelPos, int val)
    {
        // Outside grid
        if(chunkID.x<0 || chunkID.y <0 || chunkID.z <0 || chunkID.x >= grid.Length || chunkID.y >= grid[0].Length ||  chunkID.z >= grid[0][0].Length)
            return;

        // Inside - move to other side
        if(pixelPos.x <= 0)
            pixelPos.x += chunkPrefabStatic.GridSize;
        if(pixelPos.y <= 0)
            pixelPos.y += chunkPrefabStatic.GridSize;
        if(pixelPos.z <= 0)
            pixelPos.z += chunkPrefabStatic.GridSize;

        // Set the pixel
        grid[chunkID.x][chunkID.y][chunkID.z].SetGridValue(pixelPos,val);
    }

    // Update is called once per frame
    private void CreateGrid()
    {
        grid = new Chunk[Size][][];
        for (int i = 0; i < Size; i++)
        {
            grid[i] = new Chunk[Height][];
            for (int j = 0; j < Height; j++)
            {
                grid[i][j] = new Chunk[Size];
                for (int k = 0; k < Size; k++)
                {
                    Vector3 pos = new Vector3(i/chunkPrefab.GridScaling, j / chunkPrefab.GridScaling, k / chunkPrefab.GridScaling);
                    // Fixes issue where Gridsize is defined one point to many
                    pos *= (chunkPrefab.GridSize-1f)/ chunkPrefab.GridSize;
                    bool checker = (i + j + k) % 2 == 0;
                    grid[i][j][k] = Instantiate(checker?chunkPrefab : chunkPrefabB, pos,Quaternion.identity,chunkH.transform);
                    grid[i][j][k].GridIndex = new Vector3Int(i,j,k);                    
                    //Debug.Log("Setting GridIndex for chunk to "+ grid[i][j][k].GridIndex);
                }
            }
        }
        grid[2][GroundLevel][0].DefaultFilled = false;
        grid[2][GroundLevel][1].DefaultFilled = false;
    }

    //Remove ???
    public static void Notify(Vector3Int notify)
    {
        if (notify.x < 0 || notify.y < 0 || notify.z < 0 || notify.x >= grid.Length || notify.y >= grid[0].Length || notify.z >= grid[0][0].Length)
            return;  
        Debug.Log("Notify "+notify+" to update.");
        grid[notify.x][notify.y][notify.z].UpdateFromDependingNeighbors();
    }

    internal static void NotifyCarve(Vector3Int notify, Vector3Int start, Vector3Int end, int set = 0)
    {
        if (notify.x < 0 || notify.y < 0 || notify.z < 0 || notify.x >= grid.Length || notify.y >= grid[0].Length || notify.z >= grid[0][0].Length)
            return;
        grid[notify.x][notify.y][notify.z].Carve(start, end, set);  
    }

    internal static void NotifyCarveNeighbors(Vector3Int startBoundsInt, Vector3Int endBoundsInt, Vector3Int gridIndex, int set = 0)
    {
        int GridSize = grid[0][0][0].GridSize;
        // For the active chunk only carve pixels inside chunk
        Vector3Int ChunkStartBoundsInt = startBoundsInt;
        Vector3Int ChunkEndBoundsInt = endBoundsInt;

        // LEFT SLIDE
        if (startBoundsInt.x <= 0)
        {
            // LEFT
            ChunkStartBoundsInt = startBoundsInt;
            ChunkEndBoundsInt = endBoundsInt;
            // Same Y and Z different X
            ChunkStartBoundsInt.x = startBoundsInt.x + GridSize;
            ChunkEndBoundsInt.x = GridSize;
            NotifyCarve(gridIndex + new Vector3Int(-1, 0, 0), ChunkStartBoundsInt, ChunkEndBoundsInt, set);

            if (startBoundsInt.y <= 0)
            {
                // LEFT - DOWN
                ChunkStartBoundsInt.y = startBoundsInt.y + GridSize;
                ChunkEndBoundsInt.y = GridSize;
                NotifyCarve(gridIndex + new Vector3Int(-1, -1, 0), ChunkStartBoundsInt, ChunkEndBoundsInt, set);

                if (startBoundsInt.z <= 0)
                {
                    // LEFT - DOWN - BACK
                    ChunkStartBoundsInt.z = startBoundsInt.z + GridSize;
                    ChunkEndBoundsInt.z = GridSize;
                    NotifyCarve(gridIndex + new Vector3Int(-1, -1, -1), ChunkStartBoundsInt, ChunkEndBoundsInt, set);
                }
                else if (endBoundsInt.z >= GridSize)
                {
                    // LEFT - DOWN - FORWARD
                    ChunkStartBoundsInt.z = 0;
                    ChunkEndBoundsInt.z = endBoundsInt.z - GridSize;
                    NotifyCarve(gridIndex + new Vector3Int(-1, -1, 1), ChunkStartBoundsInt, ChunkEndBoundsInt, set);
                }
            }
            else if (endBoundsInt.y >= GridSize)
            {
                // LEFT - UP
                ChunkStartBoundsInt.y = 0;
                ChunkEndBoundsInt.y = endBoundsInt.y - GridSize;
                NotifyCarve(gridIndex + new Vector3Int(-1, 1, 0), ChunkStartBoundsInt, ChunkEndBoundsInt, set);

                if (startBoundsInt.z <= 0)
                {
                    // LEFT - UP - BACK
                    ChunkStartBoundsInt.z = startBoundsInt.z + GridSize;
                    ChunkEndBoundsInt.z = GridSize;
                    NotifyCarve(gridIndex + new Vector3Int(-1, 1, -1), ChunkStartBoundsInt, ChunkEndBoundsInt, set);
                }
                else if (endBoundsInt.z >= GridSize)
                {
                    // LEFT - UP - FORWARD
                    ChunkStartBoundsInt.z = 0;
                    ChunkEndBoundsInt.z = endBoundsInt.z - GridSize;
                    NotifyCarve(gridIndex + new Vector3Int(-1, 1, 1), ChunkStartBoundsInt, ChunkEndBoundsInt, set);
                }
            }

            ChunkStartBoundsInt = startBoundsInt;
            ChunkEndBoundsInt = endBoundsInt;
            ChunkStartBoundsInt.x = startBoundsInt.x + GridSize;
            ChunkEndBoundsInt.x = GridSize;

            if (startBoundsInt.z <= 0)
            {
                // LEFT - BACK
                ChunkStartBoundsInt.z = startBoundsInt.z + GridSize;
                ChunkEndBoundsInt.z = GridSize;
                NotifyCarve(gridIndex + new Vector3Int(-1, 0, -1), ChunkStartBoundsInt, ChunkEndBoundsInt, set);
            }
            else if (endBoundsInt.z >= GridSize)
            {
                // LEFT - FORWARD
                ChunkStartBoundsInt.z = 0;
                ChunkEndBoundsInt.z = endBoundsInt.z - GridSize;
                NotifyCarve(gridIndex + new Vector3Int(-1, 0, 1), ChunkStartBoundsInt, ChunkEndBoundsInt, set);
            }
        }
        else if (endBoundsInt.x >= GridSize)
        {
            // RIGHT
            ChunkStartBoundsInt = startBoundsInt;
            ChunkEndBoundsInt = endBoundsInt;
            // Same Y and Z different X
            ChunkStartBoundsInt.x = 0;
            ChunkEndBoundsInt.x = endBoundsInt.x - GridSize;
            NotifyCarve(gridIndex + new Vector3Int(1, 0, 0), ChunkStartBoundsInt, ChunkEndBoundsInt, set);

            if (startBoundsInt.y <= 0)
            {
                // RIGHT - DOWN
                ChunkStartBoundsInt.y = startBoundsInt.y + GridSize;
                ChunkEndBoundsInt.y = GridSize;
                NotifyCarve(gridIndex + new Vector3Int(1, -1, 0), ChunkStartBoundsInt, ChunkEndBoundsInt, set);

                if (startBoundsInt.z <= 0)
                {
                    // RIGHT - DOWN - BACK
                    ChunkStartBoundsInt.z = startBoundsInt.z + GridSize;
                    ChunkEndBoundsInt.z = GridSize;
                    NotifyCarve(gridIndex + new Vector3Int(1, -1, -1), ChunkStartBoundsInt, ChunkEndBoundsInt, set);
                }
                else if (endBoundsInt.z >= GridSize)
                {
                    // RIGHT - DOWN - FORWARD
                    ChunkStartBoundsInt.z = 0;
                    ChunkEndBoundsInt.z = endBoundsInt.z - GridSize;
                    NotifyCarve(gridIndex + new Vector3Int(1, -1, 1), ChunkStartBoundsInt, ChunkEndBoundsInt, set);
                }
            }
            else if (endBoundsInt.y >= GridSize)
            {
                // RIGHT - UP
                ChunkStartBoundsInt.y = 0;
                ChunkEndBoundsInt.y = endBoundsInt.y - GridSize;
                NotifyCarve(gridIndex + new Vector3Int(1, 1, 0), ChunkStartBoundsInt, ChunkEndBoundsInt, set);

                if (startBoundsInt.z <= 0)
                {
                    // RIGHT - UP - BACK
                    ChunkStartBoundsInt.z = startBoundsInt.z + GridSize;
                    ChunkEndBoundsInt.z = GridSize;
                    NotifyCarve(gridIndex + new Vector3Int(1, 1, -1), ChunkStartBoundsInt, ChunkEndBoundsInt, set);
                }
                else if (endBoundsInt.z >= GridSize)
                {
                    // RIGHT - UP - FORWARD
                    ChunkStartBoundsInt.z = 0;
                    ChunkEndBoundsInt.z = endBoundsInt.z - GridSize;
                    NotifyCarve(gridIndex + new Vector3Int(1, 1, 1), ChunkStartBoundsInt, ChunkEndBoundsInt, set);
                }
            }

            ChunkStartBoundsInt = startBoundsInt;
            ChunkEndBoundsInt = endBoundsInt;
            ChunkStartBoundsInt.x = 0;
            ChunkEndBoundsInt.x = endBoundsInt.x - GridSize;

            if (startBoundsInt.z <= 0)
            {
                // RIGHT - BACK
                ChunkStartBoundsInt.z = startBoundsInt.z + GridSize;
                ChunkEndBoundsInt.z = GridSize;
                NotifyCarve(gridIndex + new Vector3Int(1, 0, -1), ChunkStartBoundsInt, ChunkEndBoundsInt, set);
            }
            else if (endBoundsInt.z >= GridSize)
            {
                // RIGHT - FORWARD
                ChunkStartBoundsInt.z = 0;
                ChunkEndBoundsInt.z = endBoundsInt.z - GridSize;
                NotifyCarve(gridIndex + new Vector3Int(1, 0, 1), ChunkStartBoundsInt, ChunkEndBoundsInt, set);
            }
        }

        // CENTER SLIDE
        if (startBoundsInt.y <= 0)
        {
            // DOWN
            ChunkStartBoundsInt = startBoundsInt;
            ChunkEndBoundsInt = endBoundsInt;

            ChunkStartBoundsInt.y = startBoundsInt.y + GridSize;
            ChunkEndBoundsInt.y = GridSize;
            NotifyCarve(gridIndex + new Vector3Int(0, -1, 0), ChunkStartBoundsInt, ChunkEndBoundsInt, set);

            if (startBoundsInt.z <= 0)
            {
                // DOWN - BACK
                ChunkStartBoundsInt.z = startBoundsInt.z + GridSize;
                ChunkEndBoundsInt.z = GridSize;
                NotifyCarve(gridIndex + new Vector3Int(0, -1, -1), ChunkStartBoundsInt, ChunkEndBoundsInt, set);
            }
            else if (endBoundsInt.z >= GridSize)
            {
                // DOWN - FORWARD
                ChunkStartBoundsInt.z = 0;
                ChunkEndBoundsInt.z = endBoundsInt.z - GridSize;
                NotifyCarve(gridIndex + new Vector3Int(0, -1, 1), ChunkStartBoundsInt, ChunkEndBoundsInt, set);
            }

        }
        else if (endBoundsInt.y >= GridSize)
        {
            // UP
            ChunkStartBoundsInt = startBoundsInt;
            ChunkEndBoundsInt = endBoundsInt;
            ChunkStartBoundsInt.y = 0;
            ChunkEndBoundsInt.y = endBoundsInt.y - GridSize;
            NotifyCarve(gridIndex + new Vector3Int(0, 1, 0), ChunkStartBoundsInt, ChunkEndBoundsInt, set);

            if (startBoundsInt.z <= 0)
            {
                // UP - BACK
                ChunkStartBoundsInt.z = startBoundsInt.z + GridSize;
                ChunkEndBoundsInt.z = GridSize;
                NotifyCarve(gridIndex + new Vector3Int(0, 1, -1), ChunkStartBoundsInt, ChunkEndBoundsInt, set);
            }
            else if (endBoundsInt.z >= GridSize)
            {
                // UP - FORWARD
                ChunkStartBoundsInt.z = 0;
                ChunkEndBoundsInt.z = endBoundsInt.z - GridSize;
                NotifyCarve(gridIndex + new Vector3Int(0, 1, 1), ChunkStartBoundsInt, ChunkEndBoundsInt, set);
            }
        }

        if (startBoundsInt.z <= 0)
        {
            // BACK
            ChunkStartBoundsInt = startBoundsInt;
            ChunkEndBoundsInt = endBoundsInt;

            ChunkStartBoundsInt.z = startBoundsInt.z + GridSize;
            ChunkEndBoundsInt.z = GridSize;
            NotifyCarve(gridIndex + new Vector3Int(0, 0, -1), ChunkStartBoundsInt, ChunkEndBoundsInt, set);
        }
        else if (endBoundsInt.z >= GridSize)
        {
            // FORWARD
            ChunkStartBoundsInt = startBoundsInt;
            ChunkEndBoundsInt = endBoundsInt;
            ChunkStartBoundsInt.z = 0;
            ChunkEndBoundsInt.z = endBoundsInt.z - GridSize;
            NotifyCarve(gridIndex + new Vector3Int(0, 0, 1), ChunkStartBoundsInt, ChunkEndBoundsInt, set);
        }

    }

    internal static (Vector3Int, Vector3Int) Limits(Vector3Int gridIndex)
    {
        Vector3Int lowerLimit = new Vector3Int(gridIndex.x == 0 ? 1 : 0, gridIndex.y == 0 ? 1 : 0, gridIndex.z == 0 ? 1 : 0);
        Vector3Int upperLimit = new Vector3Int(gridIndex.x == grid.Length-1 ? 1 : 0, gridIndex.y == grid[0].Length-1 ? 1 : 0, gridIndex.z == grid[0][0].Length-1 ? 1 : 0);
        return (lowerLimit, upperLimit);
    }
}
