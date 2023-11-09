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
                Debug.Log("hit " + hit.point);
                (Vector2 chunkPos, Vector3 pos, int i) = MapHandler.GetPosInfo(hit.point);
                Chunk chunk = MapHandler.Chunks[chunkPos.x + "." + chunkPos.y];

                if (left) chunk.Blocks[i] = 0;
                else chunk.Blocks[i] = 5;
                chunk.BuildMesh();
                
                // update nearby chunks if placed on a chunk border
                List<string> toCheck = new ();
                if (pos.x == 0) toCheck.Add(chunkPos.x - 1 + "." + chunkPos.y);
                else if ((int)pos.x == Chunk.ChunkSize - 1) toCheck.Add(chunkPos.x + 1 + "." + chunkPos.y);
                if (pos.z == 0) toCheck.Add(chunkPos.x + "." + (chunkPos.y - 1));
                else if ((int)pos.z == Chunk.ChunkSize - 1) toCheck.Add(chunkPos.x + "." + (chunkPos.y + 1));
                foreach (string chunkName in toCheck)
                    if (MapHandler.Chunks.ContainsKey(chunkName))
                        MapHandler.Chunks[chunkName].BuildMesh(true);
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
