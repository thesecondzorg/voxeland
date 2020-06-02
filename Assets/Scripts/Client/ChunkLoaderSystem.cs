using System;
using System.Collections.Generic;
using Mirror;
using Server;
using Test.Map;
using Test.Netowrker;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Test
{
    public class ChunkLoaderSystem : NetworkBehaviour
    {
        private int playerVisibility = 5;
        [SerializeField] private TerrainGenerator terrainGenerator;
        
        [SerializeField] private WorldRenderSystem renderSystem;

        private WorldHolder worldHolder;

        // private Dictionary<uint, PlayerConnectionInfo> playerInfos = new Dictionary<uint, PlayerConnectionInfo>();
        [SerializeField] private PlayerInputSystem playerInput;

        private Nullable<Vector2Int> oldChunkPos;
        private Vector3 playerPos;
        private Nullable<Vector3> playerPosToSet;

        public void OnDestroy()
        {
            if (renderSystem != null)
            {
                renderSystem.Stop();
            }
        }

        public void Awake()
        {
            worldHolder = new WorldHolder();
            renderSystem.worldHolder = worldHolder;
            renderSystem.ChunkReadyCallBack = ChunkReadyCallBack;
            NetworkClient.RegisterHandler<ChunkPartMessage>(OnReceiveChunk);
            Debug.Log("Start client load map");
            // RequestChunks(0, Vector2Int.one);
        }

        private void Update()
        {
            playerPos = transform.position;
            Vector2Int newChunkPos = new Vector2Int(
                (int) (playerPos.x / GameSettings.CHUNK_SIZE),
                (int) (playerPos.z / GameSettings.CHUNK_SIZE));
            if (!oldChunkPos.HasValue || newChunkPos != oldChunkPos.Value)
            {
                RequestChunks(0, newChunkPos);
                oldChunkPos = newChunkPos;
            }

            if (playerInput != null && playerPosToSet.HasValue)
            {
                //playerInput.SetPosition(playerPosToSet.Value);
                playerPosToSet = null;
            }
        }

        private void RequestChunks(uint netId, Vector2Int centerChunk)
        {
            if (!isLocalPlayer)
            {
                return;
            }
            Debug.Log("Client requested chunks");
            RequestChunk(netId, centerChunk, 0);

            for (int x = 1; x < playerVisibility; x++)
            {
                for (int i = 0; i <= x; i++)
                {
                    RequestChunk(netId, centerChunk + new Vector2Int(x, i), x);
                    RequestChunk(netId, centerChunk + new Vector2Int(-x, i), x);
                    RequestChunk(netId, centerChunk + new Vector2Int(i, x), x);
                    RequestChunk(netId, centerChunk + new Vector2Int(i, -x), x);
                    RequestChunk(netId, centerChunk + new Vector2Int(x, -i), x);
                    RequestChunk(netId, centerChunk + new Vector2Int(-x, -i), x);
                    RequestChunk(netId, centerChunk + new Vector2Int(-i, x), x);
                    RequestChunk(netId, centerChunk + new Vector2Int(-i, -x), x);
                }
            }
        }

        private void RequestChunk(uint netId, Vector2Int chunkPosition, int d)
        {
            lock (worldHolder)
            {
                if (worldHolder.TryGet(chunkPosition, out LoadedChunk chunk))
                {
                    if (chunk.IsLoaded && !chunk.IsRegistered(netId))
                    {
                        renderSystem.ReceiveChunk(chunk.ChunkData);
                    }
                }
                else
                {
                    if (chunkPosition == Vector2Int.right)
                    {
                        Debug.Log("");
                    }

                    CmdLoadChunk(chunkPosition);
                    worldHolder.Set(chunkPosition, new LoadedChunk());
                }

                worldHolder.RegisterClient(chunkPosition, netId);
            }
        }

        private void CmdLoadChunk(Vector2Int chunkPosition)
        {
            connectionToServer.Send(new RequestChunkMessage
            {
                pos = chunkPosition
            });
        }

        private void OnReceiveChunk(NetworkConnection arg1, ChunkPartMessage part)
        {
            lock (worldHolder)
            {
                LoadedChunk loadedChunk;
                if (!worldHolder.TryGet(part.chunkPosition, out loadedChunk))
                {
                    loadedChunk = new LoadedChunk {};
                    worldHolder.Set(part.chunkPosition, loadedChunk);
                }

                bool done = loadedChunk.Merge(part);
                if (!done)
                {
                    return;
                }
                renderSystem.ReceiveChunk(loadedChunk.ChunkData);

                if (loadedChunk.ChunkData.chunkPosition == oldChunkPos)
                {
                    // TODO spawn player prefab
                    int height = loadedChunk.ChunkData.GetHeight((int) playerPos.x % GameSettings.CHUNK_SIZE,
                        (int) playerPos.z % GameSettings.CHUNK_SIZE);
                    playerPosToSet = new Vector3(playerPos.x, height, playerPos.z);
                }
            }
        }

        private void ChunkReadyCallBack(Vector2Int chunkPosition)
        {
            // if (chunkPosition == playerChunkPosition)
            // {
            //     
            //     Debug.Log("Enable gravity");
            //     GetComponent<PlayerInput>().EnableGravity();
            // }

            if (oldChunkPos.HasValue && Vector2Int.Distance(chunkPosition, oldChunkPos.Value) < 1.9f)
            {
                worldHolder.TryGet(chunkPosition, out LoadedChunk chunk);
                chunk.ChunkView.enableCollider = true;
            }

            if (chunkPosition == oldChunkPos)
            {
                Debug.Log("Enable gravity");
                GetComponent<PlayerInputSystem>().EnableGravity();
            }
        }
       
    }
}