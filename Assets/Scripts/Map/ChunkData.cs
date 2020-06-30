using System;
using System.Collections.Generic;
using Mirror;
using Test.Map;
using Test.Scripts.Map;
using UnityEngine;

namespace Map
{
    public class ChunkData : MessageBase
    {
        public Vector2Int chunkPosition;
        public ChunkSlice[] slices;
        // public Dictionary<Vector3Int, Metadata> blocksMetadata = new Dictionary<Vector3Int, Metadata>();
        public int Height => slices.Length;

        public BlockId GetId(Vector3Int pos)
        {
            return slices[pos.y].GetId(pos.x, pos.z);
        }

        public BlockId GetId(int x, int y, int z)
        {
            if (slices[y] == null)
            {
                return BlockId.AIR;
            }
            return slices[y].GetId(x, z);
        }

        public ChunkSlice GetSlice(int y)
        {
            return slices[y];
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

        // public override void Deserialize(NetworkReader reader)
        // {
        //     
        //     chunkPosition = new Vector2Int(reader.ReadInt32(), reader.ReadInt32());
        //     int i = reader.ReadInt32();
        // }
        //
        // public override void Serialize(NetworkWriter writer)
        // {
        //     writer.WriteInt32(chunkPosition.x);
        //     writer.WriteInt32(chunkPosition.y);
        //     
        //     writer.WriteInt32(slices.Length);
        //     
        //     foreach (ChunkSlice chunkSlice in slices)
        //     {
        //         
        //     }
        // }
    }
}