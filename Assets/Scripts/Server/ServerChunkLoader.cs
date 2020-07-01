using System;
using System.Collections.Generic;
using Client;
using Map;
using Mirror;
using Test;
using Test.Map;
using Map;
using Test.Netowrker;
using UnityEngine;

namespace Server
{
    public class ChunkPartMessage : MessageBase
    {
        public Vector2Int chunkPosition;
        public int shift;
        public ChunkSlice[] slices;
        public int height;
    }

    public class ServerChunkLoader
    {
        private ChunkSaveSystem chunkSaveSystem;
        private ChunkGeneratorSystem chunkGeneratorSystem;
        private TerrainGenerator terrainGenerator;
        private WorldHolder worldHolder;

        Dictionary<Vector2Int, List<NetworkConnection>> registered =
            new Dictionary<Vector2Int, List<NetworkConnection>>();

        public ServerChunkLoader(TerrainGenerator terrainGenerator)
        {
            this.terrainGenerator = terrainGenerator;
        }

        public void Start()
        {
            worldHolder = new WorldHolder();
            chunkSaveSystem = new ChunkSaveSystem("world1");
            chunkGeneratorSystem = new ChunkGeneratorSystem(terrainGenerator, OnReceiveGeneratedChunk);
            chunkGeneratorSystem.Start();
            NetworkServer.RegisterHandler<RequestChunkMessage>(OnRequestChunkMessage);
            NetworkServer.RegisterHandler<UnsubscribeChunk>(OnUnsubscribeChunk);
            NetworkServer.RegisterHandler<BlockUpdateRequest>(OnBlockUpdateRequest);
        }

        private void OnBlockUpdateRequest(NetworkConnection conn, BlockUpdateRequest msg)
        {
            if (worldHolder.TryGet(msg.chunkPosition, out LoadedChunk chunk))
            {
                chunk.ChunkData.slices[msg.inChunkPosition.y]
                    .Set(msg.inChunkPosition.x, msg.inChunkPosition.z, msg.blockId);
                lock (this)
                {
                    NetworkServer.SendToAll(msg);
                }

                //OnReceiveGeneratedChunk(chunk.ChunkData);
            }
        }

        private void OnUnsubscribeChunk(NetworkConnection conn, UnsubscribeChunk msg)
        {
            // TODO

            if (worldHolder.TryGet(msg.pos, out LoadedChunk chunk))
            {
                chunk.UnregisterClient(conn.connectionId);
                if (chunk.IsEmpty)
                {
                    worldHolder.Remove(msg.pos);
                    chunkSaveSystem.Save(chunk.ChunkData);
                }
            }
        }

        private void OnRequestChunkMessage(NetworkConnection conn, RequestChunkMessage msg)
        {
            // Debug.Log(msg.pos);
            if (worldHolder.TryGet(msg.pos, out LoadedChunk chunk) && chunk.IsLoaded)
            {
                try
                {
                    BatchProcess(chunk.ChunkData, message =>
                    {
                        lock (this)
                        {
                            conn.Send(message);
                        }
                    });
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            else
            {
                chunk = new LoadedChunk();
                worldHolder.Set(msg.pos, chunk);
                chunkSaveSystem.RequestChunk(msg.pos, OnReceiveGeneratedChunk, chunkGeneratorSystem.RequestChunk);
            }

            if (!registered.ContainsKey(msg.pos))
            {
                registered[msg.pos] = new List<NetworkConnection>();
            }

            registered[msg.pos].Add(conn);
            worldHolder.RegisterClient(msg.pos, conn.connectionId);
        }

        private void OnReceiveGeneratedChunk(ChunkData obj)
        {
            if (worldHolder.TryGet(obj.chunkPosition, out LoadedChunk chunk))
            {
                chunk.ChunkData = obj;
                SendChunk(chunk);
            }
        }

        private void SendChunk(LoadedChunk chunk)
        {
            BatchProcess(chunk.ChunkData, message =>
            {
                for (int i = 0; i < chunk.registeredClients.Count; i++)
                {
                    if (NetworkServer.connections.TryGetValue(chunk.registeredClients[i],
                        out NetworkConnectionToClient conn))
                    {
                        lock (this)
                        {
                            try
                            {
                                conn.Send(message);
                            }
                            catch (Exception e)
                            {
                                Debug.LogError(e);
                            }
                        }
                    }
                }
            });
        }

        private void BatchProcess(ChunkData obj, Action<ChunkPartMessage> handler)
        {
            int batchSize = 16;
            List<ChunkSlice> slices = new List<ChunkSlice>();
            int b = 0;
            for (var i = 0; i < obj.slices.Length; i++)
            {
                slices.Add(obj.slices[i]);
                if (slices.Count == batchSize)
                {
                    ChunkPartMessage message = new ChunkPartMessage
                    {
                        chunkPosition = obj.chunkPosition,
                        height = obj.slices.Length,
                        shift = b,
                        slices = slices.ToArray()
                    };
                    handler.Invoke(message);
                    b = i + 1;
                    slices.Clear();
                }
            }

            if (slices.Count > 0)
            {
                handler.Invoke(new ChunkPartMessage
                {
                    chunkPosition = obj.chunkPosition,
                    height = obj.slices.Length,
                    shift = b,
                    slices = slices.ToArray()
                });
            }
        }

        public void Stop()
        {
            chunkGeneratorSystem?.Stop();
            worldHolder.SaveAll(chunkSaveSystem.Save);
        }
    }
}