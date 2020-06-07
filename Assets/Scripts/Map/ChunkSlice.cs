using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Test.Map
{
    public class ChunkSlice : MessageBase
    {
        public uint[] blocks;
        public int size;
        public uint singleBlock = BlockId.AIR.Id;
        public bool isSingleBlock = true;
        
        public ChunkSlice()
        {
        }

        public ChunkSlice(int chunkSize)
        {
            size = chunkSize;
            
            // bits = new Dictionary<BlockId, BitArray>();
            // bits[BlockId.AIR] = new BitArray(size * size);
        }

        // public ChunkSlice(BlockId[,] blocks)
        // {
        //     this.bits = new Dictionary<BlockId, BitArray>();
        //     size = blocks.GetLength(0);
        //     for (int x = 0; x < size; x++)
        //     {
        //         for (int y = 0; y < blocks.GetLength(1); y++)
        //         {
        //             BlockId blockId = blocks[x, y];
        //             if (!bits.ContainsKey(blockId))
        //             {
        //                 bits[blockId] = new BitArray(size * size);
        //             }
        //
        //             bits[blockId].Set(y * size + x, true);
        //         }
        //     }
        // }

        public BlockId GetId(int posX, int posY)
        {
            if (isSingleBlock)
            {
                return new BlockId {Id = singleBlock};
            }
            int shift = posY * size + posX;
            try
            {
                uint id = blocks[shift];
                return new BlockId {Id = id};
            }
            catch ( Exception e)
            {
                Debug.LogError(shift);
                throw e;
            }
        }

        public BlockId Set(int x, int y, BlockId blockId)
        {
            if (isSingleBlock && blockId.Id == singleBlock)
            {
                return blockId;
            }

            isSingleBlock = false;
            if (blocks == null)
            {
                blocks = new uint[size*size];
            }
            int shift = y * size + x;
            uint oldBlock = blocks[shift];
            blocks[shift] = blockId.Id;
            return new BlockId {Id = oldBlock};
        }
    }
}