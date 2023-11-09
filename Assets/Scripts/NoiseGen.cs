using System;
using UnityEngine;

public static class NoiseGen
{
    public static int GetBlockAt(Vector3 pos)
    {
        if (Game.Level == -1) // test level
        {
            if (pos.y == 0) return Game.Blocks.Stone;
            return Game.Blocks.Air;
        }
        if (Game.Level == 0) // overworld (temporary test)
        {
            if (pos.y == 0) return Game.Blocks.Bedrock;
            float height = Mathf.PerlinNoise(pos.x / 10, pos.z / 10) * 2 + 3;
            if (pos.y > height) return Game.Blocks.Air;
            if (pos.y + 1 > height) return Game.Blocks.GrassBlock;
            if (pos.y + 2 < height) return Game.Blocks.Stone;
            return Game.Blocks.Dirt;
        }

        throw new ArgumentException("Incorrect level: " + Game.Level);
    }
}
