using System;
using System.Collections.Generic;
using Mirror;
using Server;
using Test;
using Test.Map;
using Test.Netowrker;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Client
{
    public enum ChunkRenderState
    {
        None,
        Started,
        Done
    }

    public class Chunk
    {
        public ChunkData chunk;
        public ChunkViewRenderer view;
        private int receivedParts = 0;
        private ChunkData tmpBatches;

        public ChunkRenderState renderState = ChunkRenderState.None;
        public bool IsLoaded = false;

        public bool Merge(ChunkPartMessage part)
        {
            if (tmpBatches == null)
            {
                try
                {
                    tmpBatches = new ChunkData
                        {chunkPosition = part.chunkPosition, Slices = new ChunkSlice[part.height]};
                }
                catch (OverflowException e)
                {
                    Debug.LogError(part);
                }
            }

            for (int i = 0; i < part.slices.Length; i++)
            {
                tmpBatches.Slices[part.shift + i] = part.slices[i];
            }

            receivedParts++;
            for (int i = 0; i < tmpBatches.Slices.Length; i++)
            {
                if (tmpBatches.Slices[i] == null)
                {
                    return false;
                }
            }
            chunk = tmpBatches;
            tmpBatches = null;
            IsLoaded = true;

            return true;
            // if (receivedParts == 16)
            // {
                // chunk = tmpBatches;
                // tmpBatches = null;
                // return true;
            // }

            return false;
        }
    }

    public class ChunksHolder
    {
        public Dictionary<Vector2Int, Chunk> loadedChunks = new Dictionary<Vector2Int, Chunk>();

        public bool TryGet(Vector2Int position, out Chunk chunk)
        {
            return loadedChunks.TryGetValue(position, out chunk);
        }

        public void Set(Vector2Int chunkPosition, Chunk chunk)
        {
            loadedChunks[chunkPosition] = chunk;
        }

        public void Delete(Vector2Int position)
        {
            if (loadedChunks.ContainsKey(position))
            {
                loadedChunks.Remove(position);
            }
        }
    }

    public class ChunkLoaderSystem : NetworkBehaviour
    {
        private int playerVisibility = 5;
        [SerializeField] private WorldRenderSystem renderSystem;

        // private WorldHolder worldHolder;
        private ChunksHolder worldHolder = new ChunksHolder();

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
                lock (worldHolder)
                {
                    RequestChunks(newChunkPos);
                }
                oldChunkPos = newChunkPos;
            }

            if (playerPosToSet.HasValue)
            {
                //playerInput.SetPosition(playerPosToSet.Value);
                transform.position = playerPosToSet.Value;
                playerPosToSet = null;
            }
        }

        private void UnloadChunk(Vector2Int pos)
        {
            if (worldHolder.TryGet(pos, out Chunk chunk))
            {
                if (chunk.renderState == ChunkRenderState.Done)
                {
                    chunk.view.Unload();
                }

                worldHolder.Delete(pos);
            }
        }

        private void RequestChunks(Vector2Int centerChunk)
        {
            if (!isLocalPlayer)
            {
                return;
            }

            List<Vector2Int> chunksToLoad = new List<Vector2Int>();
            Debug.Log("Client requested chunks");
            chunksToLoad.Add(centerChunk);

            for (int x = 1; x < playerVisibility; x++)
            {
                for (int i = 0; i <= x; i++)
                {
                    chunksToLoad.Add(centerChunk + new Vector2Int(x, i));
                    chunksToLoad.Add(centerChunk + new Vector2Int(-x, i));
                    chunksToLoad.Add(centerChunk + new Vector2Int(i, x));
                    chunksToLoad.Add(centerChunk + new Vector2Int(i, -x));
                    chunksToLoad.Add(centerChunk + new Vector2Int(x, -i));
                    chunksToLoad.Add(centerChunk + new Vector2Int(-x, -i));
                    chunksToLoad.Add(centerChunk + new Vector2Int(-i, x));
                    chunksToLoad.Add(centerChunk + new Vector2Int(-i, -x));
                }
            }

            foreach (Vector2Int pos in chunksToLoad)
            {
                RequestChunk(pos);
            }

            List<Vector2Int> toDelete = new List<Vector2Int>();
            foreach (Vector2Int pos in worldHolder.loadedChunks.Keys)
            {
                if (!chunksToLoad.Contains(pos))
                {
                    toDelete.Add(pos);
                }
            }

            foreach (Vector2Int pos in toDelete)
            {
                UnloadChunk(pos);
            }
        }

        private void RequestChunk(Vector2Int chunkPosition)
        {
            if (worldHolder.TryGet(chunkPosition, out Chunk chunk))
            {
                if (chunk.IsLoaded && chunk.renderState == ChunkRenderState.None)
                {
                    renderSystem.ReceiveChunk(chunk.chunk);
                }
            }
            else
            {
                Debug.Log("Request chunk " + chunkPosition);
                worldHolder.Set(chunkPosition, new Chunk());
                CmdLoadChunk(chunkPosition);
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
                if (!worldHolder.TryGet(part.chunkPosition, out Chunk loadedChunk))
                {
                    loadedChunk = new Chunk();
                    worldHolder.Set(part.chunkPosition, loadedChunk);
                }

                try
                {
                    bool done = loadedChunk.Merge(part);
                    if (!done)
                    {
                        return;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    RequestChunk(part.chunkPosition);
                    return;
                }

                if (loadedChunk.renderState == ChunkRenderState.None)
                {
                    loadedChunk.renderState = ChunkRenderState.Started;
                    renderSystem.ReceiveChunk(loadedChunk.chunk);
                }

                if (loadedChunk.chunk.chunkPosition == oldChunkPos)
                {
                    // TODO spawn player prefab
                    int height = loadedChunk.chunk.GetHeight((int) playerPos.x % GameSettings.CHUNK_SIZE,
                        (int) playerPos.z % GameSettings.CHUNK_SIZE);
                    playerPosToSet = new Vector3(playerPos.x, height + 2, playerPos.z);
                }
            }
        }

        private void ChunkReadyCallBack(Vector2Int chunkPosition, ChunkViewRenderer view)
        {
            if (worldHolder.TryGet(chunkPosition, out Chunk chunk))
            {
                chunk.view = view;
                chunk.renderState = ChunkRenderState.Done;
            }

            if (oldChunkPos.HasValue && Vector2Int.Distance(chunkPosition, oldChunkPos.Value) < 1.9f)
            {
                view.enableColliderTargetStatus = true;
            }

            if (chunkPosition == oldChunkPos)
            {
                Debug.Log("Enable gravity");
                GetComponent<PlayerInputSystem>().EnableGravity();
            }
        }
    }
}