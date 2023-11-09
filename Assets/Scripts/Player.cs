using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Camera camera;
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
                Chunk chunk = hit.transform.gameObject.GetComponent<Chunk>();
                hit.point += 0.01f * (right ? 1 : -1) * hit.normal;
                int i = ((int)hit.point.x * Chunk.ChunkSize + (int)hit.point.y) * Chunk.ChunkSize + (int)hit.point.z;
                if (left) chunk.Blocks[i] = 0;
                else chunk.Blocks[i] = 5;
                chunk.BuildMesh();
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
