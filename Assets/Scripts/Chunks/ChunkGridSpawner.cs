using System;
using UnityEditor;
using UnityEngine;

public class ChunkGridSpawner : MonoBehaviour
{
    [SerializeField] int Size = 10;
    [SerializeField] Chunk chunkPrefab;
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
            //Debug.Log("Chunk "+chunkID+" is outside of grid");
            return 0;
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

    // Update is called once per frame
    private void CreateGrid()
    {
        grid = new Chunk[Size][][];
        for (int i = 0; i < Size; i++)
        {
            grid[i] = new Chunk[Size][];
            for (int j = 0; j < Size; j++)
            {
                grid[i][j] = new Chunk[Size];
                for (int k = 0; k < Size; k++)
                {
                    Vector3 pos = new Vector3(i/chunkPrefab.GridScaling, j / chunkPrefab.GridScaling, k / chunkPrefab.GridScaling);
                    // Fixes issue where Gridsize is defined one point to many
                    pos *= (chunkPrefab.GridSize-1f)/ chunkPrefab.GridSize;

                    grid[i][j][k] = Instantiate(chunkPrefab,pos,Quaternion.identity,chunkH.transform);
                    grid[i][j][k].GridIndex = new Vector3Int(i,j,k);
                    Debug.Log("Setting GridIndex for chunk to "+ grid[i][j][k].GridIndex);
                }
            }
        }
    }

    public static void Notify(Vector3Int notify)
    {
        if (notify.x < 0 || notify.y < 0 || notify.z < 0 || notify.x >= grid.Length || notify.y >= grid[0].Length || notify.z >= grid[0][0].Length)
            return;  
        Debug.Log("Notify "+notify+" to update.");
        grid[notify.x][notify.y][notify.z].UpdateFromDependingNeighbors();
    }
}
