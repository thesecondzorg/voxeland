using System.Collections.Generic;
using Mirror;
using Test.Map;
using UnityEngine;

namespace Test.Map
{
    public class ChunkData : MessageBase
    {
        public Vector2Int chunkPosition;
        
        public ChunkSlice[] Slices;
        // public Dictionary<Vector3Int, Metadata> BlocksMetadata = new Dictionary<Vector3Int, Metadata>();
        public int Height => Slices.Length;

        public BlockId GetId(Vector3Int pos)
        {
            return Slices[pos.y].GetId(pos.x, pos.z);
        }

        public BlockId GetId(int x, int y, int z)
        {
            return Slices[y].GetId(x, z);
        }
        
        public int GetHeight(int x, int y)
        {
            for (int i = Height - 1; i > 0; i--)
            {
                BlockId blockId = GetId(x, i, y);
                if (blockId.Id != BlockId.AIR.Id)
                {
                    return i;
                }
            }

            return Height;
        }

        // public bool GetMetadata(int x, int y, int z, out Metadata metadata)
        // {
        //     return BlocksMetadata.TryGetValue(new Vector3Int(x, y, z), out metadata);
        // }
    }
}