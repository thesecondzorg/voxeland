using System;
using System.Collections.Generic;
using Server;
using Test.Map;
using UnityEngine;

namespace Test
{
    public class LoadedChunk
    {
        private readonly List<uint> registeredClients = new List<uint>();

     
        public bool IsLoaded => ChunkData != null;
        public bool IsEmpty => registeredClients.Count == 0;

        public ChunkData ChunkData;

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