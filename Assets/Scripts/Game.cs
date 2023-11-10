using System;
using System.Collections.Generic;
using UnityEngine;

public class Tile
{
    public readonly Vector2[] UVs;
    private readonly int _texWidth = 4, _texHeight = 4;

    public Tile(Vector2 pos)
    {
        UVs = new Vector2[]
        {
            new(pos.x/_texWidth, pos.y/_texHeight),
            new(pos.x/_texWidth, (pos.y + 1)/_texHeight),
            new((pos.x + 1)/_texWidth, (pos.y + 1)/_texHeight),
            new((pos.x + 1)/_texWidth, pos.y/_texHeight)
        };
    }
}

public class Block
{
    public readonly int Id;

    private readonly Tile _top, _sides, _bottom;

    public Block() // air block
    {
        Id = 0;
    }
    
    public Block(int id, Tile allFaces)
    {
        Id = id;
        _top = _sides = _bottom = allFaces;
    }

    public Block(int id, Tile top, Tile sides, Tile bottom)
    {
        Id = id;
        _top = top;
        _sides = sides;
        _bottom = bottom;
    }

    public Vector2[] GetUVs(int faceIndex)
    {
        switch (faceIndex)
        {
            case 2: return _top.UVs;
            case 3: return _bottom.UVs;
            default: return _sides.UVs;
        }
    }
}

public class Game : MonoBehaviour
{
    public Texture atlas;
    [NonSerialized] public static float TickRate = 20;
    [NonSerialized] public static int Level = 0;
    [NonSerialized] public static int Seed = 69;

    static class Tiles
    {
        public static readonly Tile
            GrassTop = new(new Vector2(0, 3)),
            GrassSides = new(new Vector2(1, 3)),
            Dirt = new(new Vector2(2, 3)),
            Stone = new(new Vector2(0, 2)),
            Bedrock = new(new Vector2(3, 3)),
            Cobblestone = new(new Vector2(1, 2));
    }

    public static class Blocks
    {
        public static readonly Dictionary<int, Block> FromId = new Dictionary<int, Block>
        {
            {0, new Block()},
            {1, new Block(1, Tiles.GrassTop, Tiles.GrassSides, Tiles.Dirt)},
            {2, new Block(2, Tiles.Dirt)},
            {3, new Block(3, Tiles.Stone)},
            {4, new Block(4, Tiles.Bedrock)},
            {5, new Block(5, Tiles.Cobblestone)}
        };

        public static readonly int
            Air = 0,
            GrassBlock = 1,
            Dirt = 2,
            Stone = 3,
            Bedrock = 4,
            Cobblestone = 5;
    }

    void Start()
    {
        NoiseGen.Init();
    }
}