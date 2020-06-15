using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Test.Map
{
    public class ChunkSlice : MessageBase
    {
        public ushort[] blocks;
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
                ushort id = blocks[shift];
                return new BlockId {Id = (uint) id};
            }
            catch (Exception e)
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
                blocks = new ushort[size * size];
            }

            int shift = y * size + x;
            ushort oldBlock = blocks[shift];
            blocks[shift] = (ushort) blockId.Id;
            return new BlockId {Id = (uint) oldBlock};
        }

        public override void Deserialize(NetworkReader reader)
        {
            isSingleBlock = reader.ReadBoolean();
            if (isSingleBlock)
            {
                singleBlock = reader.ReadUInt16();
            }
            else
            {
                blocks = new ushort[reader.ReadInt32()];
                int shift = 0;

                do
                {
                    ushort counter = reader.ReadUInt16();
                    ushort currentBlock = reader.ReadUInt16();
                    for (int i = 0; i < counter; i++)
                    {
                        blocks[shift + i] = currentBlock;
                    }
                    shift += counter;
                } while (shift < blocks.Length);
            }
        }

        public override void Serialize(NetworkWriter writer)
        {
            if (isSingleBlock)
            {
                writer.WriteBoolean(true);
                writer.WriteUInt32(singleBlock);
            }
            else
            {
                writer.WriteBoolean(false);
                writer.WriteInt32(blocks.Length);
                // ushort[] buffer = new ushort[blocks.Length];
                ushort counter = 0;
                ushort currentBlock = blocks[0];
                for (var i = 0; i < blocks.Length; i++)
                {
                    if (blocks[i] != currentBlock)
                    {
                        writer.WriteUInt16(counter);
                        writer.WriteUInt16(currentBlock);
                        counter = 0;
                        currentBlock = blocks[i];
                    }

                    counter++;
                }
            }
        }
    }
}