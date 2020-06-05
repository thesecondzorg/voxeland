using System;
using System.Collections.Generic;
using Map;
using Mirror;
using Test;
using Test.Map;
using Unity.Collections;
using UnityEngine;

namespace Client
{
    public class WorldRenderSystem : MonoBehaviour
    {
        [Header("Map")] [SerializeField] private GameObject ChunkPrefab;

        private Queue<Vector2Int> chunkToRender = new Queue<Vector2Int>();
        public ChunksHolder worldHolder;
        public Action<Vector2Int, ChunkViewRenderer> ChunkReadyCallBack;
        private void Awake()
        {
        }

        public void ReceiveChunk(ChunkData chunk)
        {
            chunkToRender.Enqueue(chunk.chunkPosition);
        }

        public void Stop()
        {
            // chunkToRender.Dispose();
        }

        private void Update()
        {
            for (int i = 0; i < 5; i++)
            {
                if (chunkToRender.Count > 0)
                {
                    Vector2Int chunkPosition = chunkToRender.Dequeue(); 
                    // if no neighbors submit again
                    if (!SpawnChunk(chunkPosition))
                    {
                        chunkToRender.Enqueue(chunkPosition);
                    }
                }
                else
                {
                    break;
                }
            }
        }
        private bool SpawnChunk(Vector2Int chunkPosition)
        {
            if (worldHolder.TryGet(chunkPosition + Vector2Int.right, out Chunk right) && right.IsLoaded &&
                worldHolder.TryGet(chunkPosition + Vector2Int.left, out Chunk left) && left.IsLoaded &&
                worldHolder.TryGet(chunkPosition + Vector2Int.up, out Chunk forward) && forward.IsLoaded &&
                worldHolder.TryGet(chunkPosition + Vector2Int.down, out Chunk back) && back.IsLoaded &&
                worldHolder.TryGet(chunkPosition, out Chunk chunk )&& chunk.IsLoaded)
            {
                 Debug.Log("Spawn " + chunkPosition);
                GameObject go = Instantiate(ChunkPrefab);
                // go.transform.SetParent(transform);
                go.transform.position = new Vector3(
                    chunkPosition.x * GameSettings.CHUNK_SIZE,
                    0,
                    chunkPosition.y * GameSettings.CHUNK_SIZE);
                ChunkViewRenderer viewRenderer = go.GetComponent<ChunkViewRenderer>();
                viewRenderer.Initial(chunk.chunk, right.chunk, left.chunk, forward.chunk, back.chunk);
                viewRenderer.ChunkReadyCallBack = ChunkReadyCallBack;
                // go.AddComponent<NetworkIdentity>();
                // chunks.Add(go);
                chunk.view = viewRenderer;
                //NetworkServer.Spawn(go);
                return true;
            }

            return false;
        }
    }
}