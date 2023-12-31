using System;
using System.Collections.Generic;
using UnityEngine;

public class CustomRigidBody
{
    public Vector3 Movement = Vector3.zero;
    public Vector3 MoveRelative;
    public bool OnFloor;

    private readonly Transform _transform;
    private readonly float _speed, _drag, _jumpForce, _gravity;

    private readonly float _width, _height;
    
    public CustomRigidBody(Transform transform, float speed, float drag, float jumpForce, float gravity, float width, float height)
    {
        _transform = transform;
        _speed = speed;
        _drag = drag;
        _jumpForce = jumpForce;
        _gravity = gravity;
        _width = width / 2;
        _height = height / 2;
    }

    private float Positive(float x)
    {
        return x > 0 ? x : 0;
    }
    
    private float Negative(float x)
    {
        return x < 0 ? x : 0;
    }

    private float Min3Abs(float a, float b, float c)
    {
        if (a < 0) a = -a;
        if (b < 0) b = -b;
        if (c < 0) c = -c;
        return a < b && b < c ? a : b < c ? b : c;
    }
    
    void CheckCollisions(Vector3 pos)
    {
        OnFloor = false;

        //Vector3 pos = _transform.position;
        Vector3 movement = pos - _transform.position;

        int chunkX = (int)MathF.Floor(pos.x / Chunk.ChunkSize);
        int chunkZ = (int)MathF.Floor(pos.z / Chunk.ChunkSize);

        // check collisions with chunks around
        for (int i = chunkX - 1; i < chunkX + 2; i++)
        for (int j = chunkZ - 1; j < chunkZ + 2; j++)
        {
            if (!MapHandler.Chunks.ContainsKey(i + "." + j)) // in an unloaded chunk: do not move
            {
                Movement = Vector3.zero;
                return;
            }

            Chunk chunk = MapHandler.Chunks[i + "." + j];
            
            // check collisions with blocks around
            // only calculate collisions with the block with the most depth
            Vector3 p = pos - new Vector3(i * Chunk.ChunkSize, 0, j * Chunk.ChunkSize); // pos in the chunk
            List<Quaternion> corrections = new List<Quaternion>();
            for (int x = (int)p.x - 1; x < (int)p.x + 2; x++)
            for (int y = (int)p.y - 3; y < (int)p.y + 3; y++)
            for (int z = (int)p.z - 1; z < (int)p.z + 2; z++)
            {
                if (x < 0 || x >= Chunk.ChunkSize || y < 0 || y >= Chunk.ChunkSize || z < 0 ||
                    z >= Chunk.ChunkSize) continue;
                if (chunk.Blocks[(x * Chunk.ChunkSize + y) * Chunk.ChunkSize + z] == 0) continue;
                if (x + 1 > p.x - _width && x < p.x + _width &&
                    y + 1 > p.y - _height && y < p.y + _height &&
                    z + 1 > p.z - _width && z < p.z + _width)
                {
                    // collision happens
                    float dx = x < p.x ? Positive(x + 1 - p.x + _width) : Negative(x - p.x - _width);
                    float dy = y <= p.y ? Positive(y + 1 - p.y + _height) : Negative(y - p.y - _height);
                    float dz = z < p.z ? Positive(z + 1 - p.z + _width) : Negative(z - p.z - _width);
                    float dist = Min3Abs(dx, dy, dz);
                    int k = 0;
                    for (; k < corrections.Count; k++)
                        if (corrections[k].w <= dist)
                            break;
                    corrections.Insert(k, new Quaternion(dx, dy, dz, dist));
                }
            }

            // calculate collisions for all colliding blocks, from most depth to least
            foreach (Quaternion correction in corrections)
            {
                // correct position in the direction with the least depth
                int toChange = -1; // 0: x, 1: y, 2: z
                float min = 1000;
                for (int k = 0; k < 3; k++)
                {
                    float coord = MathF.Abs(correction[k]);
                    if (coord != 0 && coord < min)
                    {
                        min = coord;
                        toChange = k;
                    }
                }
                if (toChange == 0) // x wall collision
                {
                    Movement.x = 0;
                    pos.x += correction.x;
                }
                else if (toChange == 1)
                {
                    pos.y += correction.y;
                    if (movement.y < 0) // floor collision
                    {
                        OnFloor = true;
                        Movement.y = 0;
                    }
                    else Movement.y *= -0.5f; // ceiling collision

                    break; // if calculated floor/ceiling collision, no need to check for other collisions
                }
                else if (toChange == 2) // z wall collision
                {
                    pos.z += correction.z;
                    Movement.z = 0;
                }
            }
        }

        _transform.position = pos;
    }
    
    public void Update()
    {
        // capped movement speed
        float delta = Time.deltaTime;
        if (delta > 0.1f) delta = 0.1f;

        // keys movement
        MoveRelative = new Vector3(Input.GetAxisRaw("Horizontal") * 0.8f, 0, Input.GetAxisRaw("Vertical")).normalized;
        Vector3 move = _transform.rotation * MoveRelative;
        float speed = Input.GetKey(KeyCode.LeftControl) ? 1.7f * _speed : _speed;
        Movement += move * (speed * delta);
        if (Input.GetKey("space") && OnFloor) Movement.y = _jumpForce;
        
        // move according to movement
        float drag = MathF.Pow(_drag, 100 * delta);
        Movement.x *= drag;
        Movement.y += _gravity * delta;
        Movement.z *= drag;

        Vector3 newPos = _transform.position + Movement * (delta * _speed);
        CheckCollisions(newPos);
    }
}