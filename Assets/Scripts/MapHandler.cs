using System;
using System.Collections.Generic;
using UnityEngine;

public class MapHandler : MonoBehaviour
{
    public GameObject chunkPlane;
    public GameObject chunkParent;
    public Material material;

    [NonSerialized] public static Dictionary<string, Chunk> Chunks;

    void Start()
    {
        Chunks = new Dictionary<string, Chunk>();
        transform.position = new Vector3(0, 0, 0);

        for (int x = -4; x < 5; x++)
            for (int z = -4; z < 5; z++)
            {
                Vector3 pos = new Vector3(x * Chunk.ChunkSize, 0, z * Chunk.ChunkSize);
                GameObject chunkObject = Instantiate(chunkPlane, pos, Quaternion.identity);
                chunkObject.transform.parent = chunkParent.transform;
                chunkObject.name = (pos.x / 8) + "." + (pos.z / 8);

                Chunk chunk = chunkObject.GetComponent<Chunk>();
                MeshRenderer meshRenderer = chunkObject.GetComponent<MeshRenderer>();
                meshRenderer.material = material;
                chunk.Init(pos);
                Chunks.Add(chunkObject.name, chunk);
            }
    }

    static int Floor(float x)
    {
        if (x < 0)
        {
            int offset = 1 - (int)x;
            return (int)(x + offset) - offset;
        }

        return (int)x;
    }

    public static (Vector2, Vector3, int) GetPosInfo(Vector3 pos)
    {
        // return the chunk name, block in chunk position and the block index in the chunk
        int chunkX = Floor(pos.x / Chunk.ChunkSize),
            chunkZ = Floor(pos.z / Chunk.ChunkSize);
        Vector2 chunkPos = new Vector2(chunkX, chunkZ);

        Debug.Log(pos);
        int x = Floor(pos.x) - chunkX * Chunk.ChunkSize,
            y = (int)pos.y,
            z = Floor(pos.z) - chunkZ * Chunk.ChunkSize;
        int i = (x * Chunk.ChunkSize + y) * Chunk.ChunkSize + z;
        
        return (chunkPos, new Vector3(x, y, z), i);
    }
    
    public static int GetBlockAt(Vector3 pos)
    {
        // returns the block at pos, if not generated return position from noise
        (Vector2 chunkPos, Vector3 dummy, int i) = GetPosInfo(pos);
        Debug.Log(chunkPos + " " + dummy + " " + i);
        if (Chunks.TryGetValue(chunkPos.x + "." + chunkPos.y, out Chunk chunk)) return chunk.Blocks[i];
        return NoiseGen.GetBlock(pos);
    }
}
