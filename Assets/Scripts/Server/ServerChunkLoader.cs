using System;
using System.Collections.Generic;
using Client;
using Map;
using Mirror;
using Test;
using Test.Map;
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
        private ChunkGeneratorSystem chunkGeneratorSystem;
        private TerrainGenerator terrainGenerator;
        private WorldHolder worldHolder = new WorldHolder();

        Dictionary<Vector2Int, List<NetworkConnection>> registered =
            new Dictionary<Vector2Int, List<NetworkConnection>>();

        private Dictionary<uint, PlayerConnectionInfo> playerInfos = new Dictionary<uint, PlayerConnectionInfo>();

        public ServerChunkLoader(TerrainGenerator terrainGenerator)
        {
            this.chunkGeneratorSystem = chunkGeneratorSystem;
            this.terrainGenerator = terrainGenerator;
            this.worldHolder = worldHolder;
        }

        public void Start()
        {
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
                chunk.ChunkData.Slices[msg.inChunkPosition.y]
                    .Set(msg.inChunkPosition.x, msg.inChunkPosition.z, msg.blockId);
                // FIXME we should send update request only
                OnReceiveGeneratedChunk(chunk.ChunkData);
            }
        }

        private void OnUnsubscribeChunk(NetworkConnection conn, UnsubscribeChunk msg)
        {
            // TODO

            if (worldHolder.TryGet(msg.pos, out LoadedChunk chunk))
            {
                chunk.UnregisterClient(conn.connectionId);
            }
        }

        private void OnRequestChunkMessage(NetworkConnection conn, RequestChunkMessage msg)
        {
            Debug.Log(msg.pos);
            if (worldHolder.TryGet(msg.pos, out LoadedChunk chunk) && chunk.IsLoaded)
            {
                try
                {
                    BatchProcess(chunk.ChunkData, message => conn.Send(message));
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            else
            {
                chunk = new LoadedChunk();
                worldHolder.Set(msg.pos, chunk);
                chunkGeneratorSystem.RequestChunk(msg.pos);
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

            // if (registered.TryGetValue(obj.chunkPosition, out List<NetworkConnection> list))
            // {
            //     SendChunk(obj, list);
            // }
        }

        private void SendChunk(LoadedChunk chunk)
        {
            BatchProcess(chunk.ChunkData, message =>
            {
                for (var i = 0; i < chunk.registeredClients.Count; i++)
                {
                    if (NetworkServer.connections.TryGetValue(chunk.registeredClients[i],
                        out NetworkConnectionToClient conn))
                    {
                        conn.Send(message);
                    }
                }
            });
        }

        private void BatchProcess(ChunkData obj, Action<ChunkPartMessage> handler)
        {
            int batchSize = 8;
            List<ChunkSlice> slices = new List<ChunkSlice>();
            int b = 0;
            for (var i = 0; i < obj.Slices.Length; i++)
            {
                slices.Add(obj.Slices[i]);
                if (slices.Count == batchSize)
                {
                    ChunkPartMessage message = new ChunkPartMessage
                    {
                        chunkPosition = obj.chunkPosition,
                        height = obj.Slices.Length,
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
                    height = obj.Slices.Length,
                    shift = b,
                    slices = slices.ToArray()
                });
            }
        }

        private void SendChunk(ChunkData obj, List<NetworkConnection> list)
        {
            BatchProcess(obj, message =>
            {
                foreach (NetworkConnection conn in list)
                {
                    conn.Send(message);
                }

                // NetworkServer.SendToAll(message);
            });
        }

        public void Stop()
        {
            chunkGeneratorSystem?.Stop();
        }
    }
}