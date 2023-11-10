using System;
using UnityEngine;

public static class NoiseGen
{
    private static FastNoiseLite _noise;
    public static void Init()
    {
        _noise = new FastNoiseLite();
        _noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        _noise.SetSeed(Game.Seed);
    }
    
    public static int GetBlock(Vector3 pos)
    {
        if (Game.Level == -1) // test level
        {
            if (pos.y == 0) return Game.Blocks.Stone;
            return Game.Blocks.Air;
        }
        if (Game.Level == 0) // overWorld (temporary test)
        {
            if (pos.y == 0) return Game.Blocks.Bedrock;
            float height = _noise.GetNoise(pos.x, pos.z) * 2 + 4 +
                           _noise.GetNoise(pos.x * 10 + 1000, pos.z * 10 + 1000) / 2;
            if (pos.y > height) return Game.Blocks.Air;
            if (pos.y + 1 > height) return Game.Blocks.GrassBlock;
            if (pos.y + 2 < height) return Game.Blocks.Stone;
            return Game.Blocks.Dirt;
        }

        throw new ArgumentException("Incorrect level: " + Game.Level);
    }
}
