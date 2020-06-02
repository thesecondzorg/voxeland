using System;
using System.Collections.Generic;
using Mirror;
using Test.Map;
using Unity.Collections;
using UnityEngine;

namespace Test
{
    public class WorldRenderSystem : MonoBehaviour
    {
        [SerializeField] private TerrainGenerator terrainGenerator;
        [Header("Map")] [SerializeField] private GameObject ChunkPrefab;

        private Queue<Vector2Int> chunkToRender;
        public WorldHolder worldHolder;
        public Action<Vector2Int> ChunkReadyCallBack;
        private void Awake()
        {
            chunkToRender = new Queue<Vector2Int>();
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
            if (worldHolder.TryGet(chunkPosition + Vector2Int.right, out LoadedChunk right) && right.IsLoaded &&
                worldHolder.TryGet(chunkPosition + Vector2Int.left, out LoadedChunk left) && left.IsLoaded &&
                worldHolder.TryGet(chunkPosition + Vector2Int.up, out LoadedChunk forward) && forward.IsLoaded &&
                worldHolder.TryGet(chunkPosition + Vector2Int.down, out LoadedChunk back) && back.IsLoaded &&
                worldHolder.TryGet(chunkPosition, out LoadedChunk chunk )&& chunk.IsLoaded)
            {
                 Debug.Log("Spawn " + chunkPosition);
                GameObject go = Instantiate(ChunkPrefab);
                // go.transform.SetParent(transform);
                go.transform.position = new Vector3(
                    chunkPosition.x * GameSettings.CHUNK_SIZE,
                    0,
                    chunkPosition.y * GameSettings.CHUNK_SIZE);
                ChunkViewRenderer viewRenderer = go.GetComponent<ChunkViewRenderer>();
                viewRenderer.Initial(chunk.ChunkData, right.ChunkData, left.ChunkData, forward.ChunkData, back.ChunkData);
                viewRenderer.ChunkReadyCallBack = ChunkReadyCallBack;
                // go.AddComponent<NetworkIdentity>();
                // chunks.Add(go);
                chunk.ChunkView = viewRenderer;
                //NetworkServer.Spawn(go);
                return true;
            }

            return false;
        }
    }
}