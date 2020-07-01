using System;
using System.Collections.Generic;
using Map;
using UnityEngine;

namespace Test
{
    public class LoadedChunk
    {
        public readonly List<int> registeredClients = new List<int>();
        public bool IsLoaded => ChunkData != null;
        public bool IsEmpty => registeredClients.Count == 0;

        public ChunkData ChunkData;

        public void RegisterClient(int connectionId)
        {
            registeredClients.Add(connectionId);
        }

        public bool IsRegistered(int connectionId)
        {
            return registeredClients.Contains(connectionId);
        }

        public bool UnregisterClient(int netIdentityNetId)
        {
            return registeredClients.Remove(netIdentityNetId);
        }
    }
    
    public class WorldHolder 
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

        public bool RegisterClient(Vector2Int pos, int pid)
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

        public void Remove(Vector2Int msgPos)
        {
            loadedChunks.Remove(msgPos);
        }

        public void SaveAll(Action<ChunkData> save)
        {
            foreach (LoadedChunk loadedChunk in loadedChunks.Values)
            {
                save.Invoke(loadedChunk.ChunkData);
            }
        }
    }
}