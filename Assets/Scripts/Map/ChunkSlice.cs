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
        public readonly int size;
        public BlockId singleBlock = BlockId.AIR;
        public bool isSingleBlock = true;

        public ChunkSlice()
        {
        }

        public ChunkSlice(int chunkSize)
        {
            size = chunkSize;
        }
        
        public BlockId GetId(int posX, int posY)
        {
            if (isSingleBlock)
            {
                return singleBlock;
            }

            int shift = posY * size + posX;
            try
            {
                ushort id = blocks[shift];
                return BlockId.of(id);
            }
            catch (Exception e)
            {
                Debug.LogError(shift);
                throw e;
            }
        }

        public BlockId Set(int x, int y, BlockId blockId)
        {
            if (isSingleBlock && blockId.Equals(singleBlock))
            {
                return blockId;
            }

            isSingleBlock = false;
            if (blocks == null)
            {
                blocks = new ushort[size * size];
            }

            int shift = y * size + x ;
            ushort oldBlock = blocks[shift];
            blocks[shift] = (ushort) blockId.Id;
            return BlockId.of(oldBlock);
        }

        public override void Deserialize(NetworkReader reader)
        {
            try
            {
                isSingleBlock = reader.ReadBoolean();
                if (isSingleBlock)
                {
                    singleBlock = BlockId.of(reader.ReadUInt16());
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
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public override void Serialize(NetworkWriter writer)
        {
            try
            {
                writer.WriteBoolean(isSingleBlock);
                if (isSingleBlock)
                {
                    writer.WriteUInt16((ushort) singleBlock.Id);
                }
                else
                {
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
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}