using System.Collections.Generic;
using Server;
using Test.Map;
using UnityEngine;

namespace Test
{
    public class LoadedChunk
    {
        private readonly List<uint> registeredClients = new List<uint>();

        private int receivedParts = 0;
        private ChunkData tmpBatches;
        public bool IsLoaded => ChunkData != null;
        public bool IsReady => ChunkView != null;
        public bool IsEmpty => registeredClients.Count == 0;

        public ChunkData ChunkData;
        public ChunkViewRenderer ChunkView { get; set; }

        public void RegisterClient(uint connectionId)
        {
            registeredClients.Add(connectionId);
        }

        public bool IsRegistered(uint connectionId)
        {
            return registeredClients.Contains(connectionId);
        }

        public bool UnregisterClient(uint netIdentityNetId)
        {
            return registeredClients.Remove(netIdentityNetId);
        }

        public bool Merge(ChunkPartMessage part)
        {
            if (tmpBatches == null)
            {
                tmpBatches = new ChunkData {chunkPosition = part.chunkPosition, Slices = new ChunkSlice[part.height]};
            }

            for (int i = 0; i < part.slices.Length; i++)
            {
                tmpBatches.Slices[part.shift+i] = part.slices[i];
            }

            receivedParts++;
            if (receivedParts == 16)
            {
                ChunkData = tmpBatches;
                tmpBatches = null;
                return true;
            }
            return false;
        }
    }
    
    [CreateAssetMenu(fileName = "WorldHolder", menuName = "GameAssets/WorldHolder")]
    public class WorldHolder : ScriptableObject
    {
        private Dictionary<Vector2Int, LoadedChunk>
            loadedChunks = new Dictionary<Vector2Int, LoadedChunk>();

        public bool TryGet(Vector2Int pos, out LoadedChunk chunk)
        {
            return loadedChunks.TryGetValue(pos, out chunk);
        }

        public void Set(Vector2Int pos, LoadedChunk chunk)
        {
            loadedChunks[pos] = chunk;
        }

        public LoadedChunk Set(Vector2Int pos, ChunkData chunk)
        {
            if (!loadedChunks.ContainsKey(pos))
            {
                loadedChunks[pos] = new LoadedChunk();
            }

            loadedChunks[pos].ChunkData = chunk;
            return loadedChunks[pos];
        }

        public bool RegisterClient(Vector2Int pos, uint pid)
        {
            if (TryGet(pos, out LoadedChunk chunk))
            {
                chunk.RegisterClient(pid);
                return true;
            }

            return false;
        }

        public bool Contains(Vector2Int pos)
        {
            return loadedChunks.ContainsKey(pos);
        }
    }
}