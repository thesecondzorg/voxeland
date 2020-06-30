using System;

namespace Test.Map
{
    public enum ViewType
    {
        None, Block, Mesh
    }
    public struct BlockId : IEquatable<BlockId>
    {
        public static readonly BlockId AIR = new BlockId {Id = 0};
        public static BlockId[] Blocks = new BlockId[1024]; 
        public uint Id;

        public BlockId(int i)
        {
            Id = (uint) i;
        }

        public static BlockId of(int id)
        {
            return Blocks[id];
        }
        public bool Equals(BlockId other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            return obj is BlockId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (int) Id;
        }

        public static BlockId of(uint id)
        {
            return Blocks[id];
        }
    }
}