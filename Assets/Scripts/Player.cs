using System;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public new Camera camera;
    public PlayerCamera playerCamera;

    [NonSerialized] public CustomRigidBody Body;
    [NonSerialized] public float GroundedHeight; // height at which the player was last grounded

    void Start()
    {
        Body = new CustomRigidBody(transform, 10, 0.8f, 1.3f, -5, 0.95f, 1.85f);
    }

    int Floor(float x)
    {
        if (x < 0)
        {
            int offset = 1 - (int)x;
            return (int)(x + offset) - offset;
        }

        return (int)x;
    }
    
    void PlaceBreak()
    {
        bool left = Input.GetMouseButtonDown(0), right = Input.GetMouseButtonDown(1);
        if (left || right)
        {
            RaycastHit hit;
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                // move into or out of the block to get the right targeted block
                hit.point += 0.01f * (right ? 1 : -1) * hit.normal;

                int chunkX = Floor(hit.point.x / Chunk.ChunkSize),
                    chunkZ = Floor(hit.point.z / Chunk.ChunkSize);
                Chunk chunk = MapHandler.Chunks[chunkX + "." + chunkZ];

                int x = Floor(hit.point.x) - chunkX * Chunk.ChunkSize,
                    z = Floor(hit.point.z) - chunkZ * Chunk.ChunkSize;
                int i = (x * Chunk.ChunkSize + Floor(hit.point.y)) * Chunk.ChunkSize + z;

                if (left) chunk.Blocks[i] = 0;
                else chunk.Blocks[i] = 5;
                chunk.BuildMesh();
                
                // update nearby chunks if placed on a chunk border
                List<string> toCheck = new ();
                if (x == 0) toCheck.Add(chunkX - 1 + "." + chunkZ);
                else if (x == Chunk.ChunkSize - 1) toCheck.Add(chunkX + 1 + "." + chunkZ);
                if (z == 0) toCheck.Add(chunkX + "." + (chunkZ - 1));
                else if (z == Chunk.ChunkSize - 1) toCheck.Add(chunkX + "." + (chunkZ + 1));
                foreach (string chunkName in toCheck)
                    if (MapHandler.Chunks.ContainsKey(chunkName))
                        MapHandler.Chunks[chunkName].BuildMesh();
            }
        }
    }
    
    void Update()
    {
        Body.Update();
        if (Body.OnFloor) GroundedHeight = transform.position.y;

        // rotate camera about the Y axis
        Vector3 rotation = transform.rotation.eulerAngles;
        bool change = false;
        if (Input.GetKeyDown(KeyCode.Q))
        {
            change = true;
            rotation.y -= 45;
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            change = true;
            rotation.y += 45;
        }
        if (change)
        {
            transform.rotation = Quaternion.Euler(rotation);
            playerCamera.TargetRot(rotation.y);
        }

        PlaceBreak();

        if (Input.GetKeyDown(KeyCode.R)) // reset
        {
            transform.position = new Vector3(0, 5, 0);
            Body.Movement = Vector3.zero;
        }
    }
}
