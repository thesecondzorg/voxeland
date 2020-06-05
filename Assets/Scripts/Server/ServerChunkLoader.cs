using System;
using System.Collections.Generic;
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
        }

        private void OnRequestChunkMessage(NetworkConnection conn, RequestChunkMessage msg)
        {
            Debug.Log(msg.pos);
            if (worldHolder.TryGet(msg.pos, out LoadedChunk chunk))
            {
                conn.Send(chunk.ChunkData);
            }
            else
            {
                chunkGeneratorSystem.RequestChunk(msg.pos);
            }

            if (!registered.ContainsKey(msg.pos))
            {
                registered[msg.pos] = new List<NetworkConnection>();
            }

            registered[msg.pos].Add(conn);
            // worldHolder.RegisterClient(msg.pos, conn);
        }

        private void OnReceiveGeneratedChunk(ChunkData obj)
        {
            if (registered.TryGetValue(obj.chunkPosition, out List<NetworkConnection> list))
            {
                for (int i = 0; i < 16; i++)
                {
                    ChunkPartMessage message = new ChunkPartMessage
                    {
                        chunkPosition = obj.chunkPosition,
                        height = obj.Slices.Length,
                        shift = i * 16,
                        slices = new ChunkSlice [16]
                    };
                    for (int y = 0; y < 16; y++)
                    {
                        int index0 = i * 16 + y;
                        if (index0 >= obj.Height)
                        {
                            break;
                        }

                        message.slices[y] = obj.Slices[index0];
                    }

                    foreach (NetworkConnection connection in list)
                    {
                        try
                        {
                            connection.Send(message);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError("Unable to send " + obj.chunkPosition.ToString());
                            Debug.LogError(e);
                        }
                    }
                }
            }
        }

        public void Stop()
        {
            chunkGeneratorSystem?.Stop();
        }
    }
}