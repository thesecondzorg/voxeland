using System;
using System.Collections.Generic;
using System.Timers;
using Map;
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
    public class ChunkLoaderSystem : NetworkBehaviour
    {
        private int playerVisibility = 5;
        [SerializeField] private WorldRenderSystem renderSystem;

        public ChunksHolder worldHolder = new ChunksHolder();

        private Nullable<Vector2Int> oldChunkPos;
        private Vector3 playerPos;
        private Nullable<Vector3> playerPosToSet;

        public void OnDisable()
        {
            if (renderSystem != null)
            {
                renderSystem.Stop();
            }

            foreach (KeyValuePair<Vector2Int, Chunk> pair in worldHolder.loadedChunks)
            {
                if (pair.Value.renderState == ChunkRenderState.Done)
                {
                    pair.Value.view.Unload();
                }
            }
        }

        public override void OnStartClient()
        {
            if (!isLocalPlayer)
            {
                return;
            }

            renderSystem.worldHolder = worldHolder;
            renderSystem.ChunkReadyCallBack = ChunkReadyCallBack;
            NetworkClient.RegisterHandler<ChunkPartMessage>(OnReceiveChunk);
            NetworkClient.RegisterHandler<BlockUpdateRequest>(OnBlockUpdateRequest);
            Debug.Log("Start client load map");
        }

        public void OnBlockUpdateRequest(BlockUpdateRequest obj)
        {
            Debug.Log("Update block");
            lock (worldHolder)
            {
                if (!worldHolder.TryGet(obj.chunkPosition, out Chunk chunk)) return;
                Vector3Int inChunkPosition = obj.inChunkPosition;
                chunk.chunk.slices[inChunkPosition.y].Set(inChunkPosition.x, inChunkPosition.z, obj.blockId);
                chunk.Reload(true);
                renderSystem.ReceiveChunk(chunk.chunk);
                TestSide(obj.inChunkPosition, obj.chunkPosition);
            }
        }

        private void TestSide(Vector3Int inChunkPosition, Vector2Int chunkPosition)
        {
            if (inChunkPosition.x == GameSettings.CHUNK_SIZE_N)
            {
                renderSystem.ReloadChunk(chunkPosition +  Vector2Int.right);
            }

            if (inChunkPosition.x == 0)
            {
                renderSystem.ReloadChunk(chunkPosition +  Vector2Int.left);
            }

            if (inChunkPosition.z == GameSettings.CHUNK_SIZE_N)
            {
                renderSystem.ReloadChunk(chunkPosition + Vector2Int.up);
            }

            if (inChunkPosition.z == 0)
            {
                renderSystem.ReloadChunk(chunkPosition + Vector2Int.down);
            }
        }

        private bool TestOnEdge(int x, int y)
        {
            return x >= GameSettings.CHUNK_SIZE || x < 0 || y >= GameSettings.CHUNK_SIZE || y < 0;
        }

        private void Update()
        {
            if (!isLocalPlayer)
            {
                return;
            }
            playerPos = transform.position;
            Vector2Int newChunkPos = new Vector2Int(
                (int) (playerPos.x / GameSettings.CHUNK_SIZE),
                (int) (playerPos.z / GameSettings.CHUNK_SIZE));
            if (!oldChunkPos.HasValue || Vector2Int.Distance(newChunkPos, oldChunkPos.Value) > 2)
            {
                if (connectionToServer.isReady)
                {
                    lock (worldHolder)
                    {
                        RequestChunks(newChunkPos);
                    }

                    oldChunkPos = newChunkPos;
                }
                else
                {
                    Debug.Log("Connection is not ready");
                }
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
                lock(this)
                {
                    connectionToServer.Send(new UnsubscribeChunk {pos = pos});
                }
            }
        }

        private void RequestChunks(Vector2Int centerChunk)
        {
            if (!isLocalPlayer)
            {
                return;
            }

            List<Vector2Int> chunksToLoad = new List<Vector2Int>();
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
                if (chunk.isLoaded && chunk.renderState == ChunkRenderState.None)
                {
                    renderSystem.ReceiveChunk(chunk.chunk);
                }
            }
            else
            {
                // Debug.Log("Request chunk " + chunkPosition);
                worldHolder.Set(chunkPosition, new Chunk());
                CmdLoadChunk(chunkPosition);
            }
        }

        private void CmdLoadChunk(Vector2Int chunkPosition)
        {
            try
            {
                lock (this)
                {
                    connectionToServer.Send(new RequestChunkMessage
                    {
                        pos = chunkPosition
                    });
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        private void OnReceiveChunk(NetworkConnection conn, ChunkPartMessage part)
        {
            lock (worldHolder)
            {
                if (!worldHolder.TryGet(part.chunkPosition, out Chunk loadedChunk))
                {
                    loadedChunk = new Chunk();
                    worldHolder.Set(part.chunkPosition, loadedChunk);
                }

                if (loadedChunk.renderState == ChunkRenderState.Done)
                {
                    loadedChunk.Reload();
                    // Chunk chunk;
                    // if (worldHolder.TryGet(loadedChunk.chunk.chunkPosition + Vector2Int.left, out chunk))
                    // {
                    //     renderSystem.ReceiveChunk(chunk.chunk);
                    // }
                    //
                    // if (worldHolder.TryGet(loadedChunk.chunk.chunkPosition + Vector2Int.right, out chunk))
                    // {
                    //     renderSystem.ReceiveChunk(chunk.chunk);
                    // }
                    //
                    // if (worldHolder.TryGet(loadedChunk.chunk.chunkPosition + Vector2Int.up, out chunk))
                    // {
                    //     renderSystem.ReceiveChunk(chunk.chunk);
                    // }
                    //
                    // if (worldHolder.TryGet(loadedChunk.chunk.chunkPosition + Vector2Int.down, out chunk))
                    // {
                    //     renderSystem.ReceiveChunk(chunk.chunk);
                    // }
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

                loadedChunk.chunkFullyLoad = DateTime.Now;

                if (loadedChunk.renderState == ChunkRenderState.None)
                {
                    loadedChunk.renderState = ChunkRenderState.Started;
                    loadedChunk.putInQueue = DateTime.Now;
                    renderSystem.ReceiveChunk(loadedChunk.chunk);
                }

                if (loadedChunk.chunk.chunkPosition == oldChunkPos)
                {
                    // TODO spawn player prefab
                    Vector3Int inChunkPos = GameSettings.ToInChunkPos(playerPos);
                    int height = loadedChunk.chunk.GetHeight(inChunkPos.x, inChunkPos.z);
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

    public class UnsubscribeChunk : MessageBase
    {
        public Vector2Int pos;
    }

    public enum ChunkRenderState
    {
        None,
        Started,
        Done,
        Reload
    }

    public class Chunk
    {
        public ChunkData chunk;
        public ChunkViewRenderer view;
        private ChunkData tmpBatches;
        public DateTime startChunkLoad = DateTime.Now;
        public DateTime chunkFullyLoad;
        public DateTime putInQueue;
        public DateTime meshGenStart;
        public DateTime meshGenEnd;

        public ChunkRenderState renderState = ChunkRenderState.None;
        public bool isLoaded = false;

        public bool Merge(ChunkPartMessage part)
        {
            if (tmpBatches == null)
            {
                try
                {
                    tmpBatches = new ChunkData
                        {chunkPosition = part.chunkPosition, slices = new ChunkSlice[part.height]};
                }
                catch (OverflowException e)
                {
                    Debug.LogError(part);
                    return false;
                }
            }

            for (int i = 0; i < part.slices.Length; i++)
            {
                tmpBatches.slices[part.shift + i] = part.slices[i];
            }

            for (int i = 0; i < tmpBatches.slices.Length; i++)
            {
                if (tmpBatches.slices[i] == null)
                {
                    return false;
                }
            }

            chunk = tmpBatches;
            tmpBatches = null;
            isLoaded = true;

            return true;
        }

        public void Reload(bool loaded = false)
        {
            Debug.Log("Reload chunk " + chunk.chunkPosition);
            renderState = ChunkRenderState.None;
            isLoaded = loaded;
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
}