using UnityEngine;

namespace Test
{
    public class GameSettings
    {
        public static int CHUNK_SIZE = 16;
        public static int WORLD_HEIGHT = 256;

        public static Vector2Int ToChunkPos(Vector3 pos)
        {
            float x = pos.x >= 0 ? pos.x : pos.x - CHUNK_SIZE; 
            float y = pos.z >= 0 ? pos.z : pos.z - CHUNK_SIZE; 
            return new Vector2Int((int) x / CHUNK_SIZE, (int) y / CHUNK_SIZE);
        }

        public static Vector3Int ToInChunkPos(Vector3 pos)
        {
            int x = (int) pos.x % CHUNK_SIZE;
            int z = (int) pos.z % CHUNK_SIZE;
            return new Vector3Int(x >= 0 ? x : CHUNK_SIZE + x, (int) pos.y, z >= 0 ? z : z + CHUNK_SIZE);
        }
    }
}