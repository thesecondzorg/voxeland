using System;

namespace Test.Map
{
    public enum ViewType
    {
        Block, Mesh
    }
    public struct BlockId : IEquatable<BlockId>
    {
        public static readonly BlockId AIR = new BlockId {Id = 0};
        public uint Id;

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
    }
}