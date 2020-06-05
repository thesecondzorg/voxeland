using UnityEngine;

namespace Test
{
    public class GameSettings
    {
        public static int CHUNK_SIZE = 16;
        public static int WORLD_HEIGHT = 256;
        
        public static Vector2Int ToChunkPos(Vector3 pos)
        {
            return new Vector2Int((int) pos.x / CHUNK_SIZE, (int) pos.z / CHUNK_SIZE);
        }
        
        public static Vector3Int ToInChunkPos(Vector3 pos)
        {
            return new Vector3Int((int) pos.x % CHUNK_SIZE, (int)pos.y, (int) pos.z % CHUNK_SIZE);
        }
    }
}