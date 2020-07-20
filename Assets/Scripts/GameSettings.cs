using UnityEngine;

namespace Test
{
    public class GameSettings
    {
        public static int CHUNK_SIZE = 24;
        public static int CHUNK_SIZE_N = CHUNK_SIZE - 1;
        public static int WORLD_HEIGHT = 256;

        public static Vector2Int ToChunkPos(Vector3 pos)
        {
            float x = pos.x >= 0 ? pos.x : pos.x - CHUNK_SIZE_N; 
            float y = pos.z >= 0 ? pos.z : pos.z - CHUNK_SIZE_N; 
            return new Vector2Int((int) x / CHUNK_SIZE, (int) y / CHUNK_SIZE);
        }

        public static Vector3Int ToInChunkPos(Vector3 pos)
        {
            int x = (int) pos.x % CHUNK_SIZE;
            int z = (int) pos.z % CHUNK_SIZE;
            // int x = (int) pos.x & 0xf;
            // int z = (int) pos.z & 0xf;
            return new Vector3Int(x >= 0 ? x : CHUNK_SIZE + x, (int) pos.y, z >= 0 ? z : z + CHUNK_SIZE);
        }
    }
}