using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Map;
using Test.Biomes;
using Test.Map;
using Unity.Collections;
using UnityEngine;

namespace Map
{
    public class ChunkGeneratorSystem
    {
        private TerrainGenerator TerrainGenerator;
        private Action<ChunkData> receiveChunk;

        private List<Thread> chunkLoaders;
        private ManualResetEvent chunkLoadEvent = new ManualResetEvent(false);

        private bool Working = true;
        private ConcurrentQueue<Vector2Int> chunkPositions;
        private ParallelOptions parallelOptions;

        public ChunkGeneratorSystem(TerrainGenerator terrainGenerator, Action<ChunkData> receiveChunk)
        {
            parallelOptions = new ParallelOptions();
            parallelOptions.MaxDegreeOfParallelism = 3;
            this.TerrainGenerator = terrainGenerator;
            this.receiveChunk = receiveChunk;
            chunkPositions = new ConcurrentQueue<Vector2Int>();
        }

        public void Start()
        {
            chunkLoaders = new List<Thread>();
            for (int i = 0; i < 1; i++)
            {
                Thread thread = new Thread(LoadChunk);
                chunkLoaders.Add(thread);
                thread.Start();
            }
        }

        public void Stop()
        {
            // chunkPositions.Dispose();
            foreach (Thread chunkLoader in chunkLoaders)
            {
                chunkLoader.Abort();
            }

        }

        private void LoadChunk()
        {
            while (Working)
            {
                chunkLoadEvent.WaitOne();
                if (chunkPositions.TryDequeue(out Vector2Int chunkPosition ))
                {
                    receiveChunk.Invoke(TerrainGenerator.GenerateNewChunk(chunkPosition));
                    chunkLoadEvent.Set();
                }
                else
                {
                    chunkLoadEvent.Reset();
                }
            }
        }

        public void RequestChunk(Vector2Int pos)
        {
            // ThreadPool.QueueUserWorkItem(st => receiveChunk.Invoke(TerrainGenerator.GenerateNewChunk(pos)));
            // Parallel.Invoke(() => receiveChunk.Invoke(TerrainGenerator.GenerateNewChunk(pos)));
            chunkPositions.Enqueue(pos);
            chunkLoadEvent.Set();
        }
    }
}