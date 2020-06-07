using System;
using System.Collections.Generic;
using System.Threading;
using Map;
using Test.Biomes;
using Test.Map;
using Unity.Collections;
using UnityEngine;

namespace Test
{
    public class ChunkGeneratorSystem
    {
        private TerrainGenerator TerrainGenerator;
        private Action<ChunkData> receiveChunk;

        private Thread chunkLoad;
        private ManualResetEvent chunkLoadEvent = new ManualResetEvent(false);

        private bool Working = true;
        private Queue<Vector2Int> chunkPositions;

        public ChunkGeneratorSystem(TerrainGenerator terrainGenerator, Action<ChunkData> receiveChunk)
        {
            this.TerrainGenerator = terrainGenerator;
            this.receiveChunk = receiveChunk;
            chunkPositions = new Queue<Vector2Int>();

            chunkLoad = new Thread(LoadChunk);
        }

        public void Start()
        {
            chunkLoad.Start();
        }
        
        public void Stop()
        {
            // chunkPositions.Dispose();
            chunkLoad.Abort();
        }

        private void LoadChunk()
        {
            while (Working)
            {
                chunkLoadEvent.WaitOne();
                if (chunkPositions.Count > 0)
                {
                    Vector2Int chunkPosition = chunkPositions.Dequeue();
                    // int d = GameSettings.CHUNK_SIZE * GameSettings.CHUNK_SIZE;
                    ChunkSlice[] slices = new ChunkSlice[TerrainGenerator.WorldHeight];
                    for (int i = 0; i < TerrainGenerator.WorldHeight; i++)
                    {
                        slices[i] = new ChunkSlice(GameSettings.CHUNK_SIZE);
                    }
                    // uint[] blocks = new uint[TerrainGenerator.WorldHeight * d];
                    for (int x = 0; x < GameSettings.CHUNK_SIZE; x++)
                    {
                        for (int y = 0; y < GameSettings.CHUNK_SIZE; y++)
                        {
                            BiomeSpecification biomeSpecification = TerrainGenerator.resolveBiome(chunkPosition);
                            float heightVariation = biomeSpecification.MaxHeight - biomeSpecification.MinHeight;
                            float tx = (chunkPosition.x * GameSettings.CHUNK_SIZE + TerrainGenerator.Seed + x) / 50f;
                            float ty = (chunkPosition.y * GameSettings.CHUNK_SIZE + TerrainGenerator.Seed + y) / 50f;
                            // double height = Perlin.Fbm(tx, ty, 4) * heightVariation +
                            //                 biomeSpecification.MinHeight;
                            double height = biomeSpecification.MinHeight;
                            if (y == 0 && x == 0)
                            {
                                height++;
                            }
                            // Debug.Log($"{tx}, {ty} : {height}");
                            for (int h = 0; h < height; h++)
                            {
                                BlockId blockId = TerrainGenerator.Get(
                                    (float) height,
                                    new Vector3Int(chunkPosition.x * GameSettings.CHUNK_SIZE + x, h,
                                        chunkPosition.y * GameSettings.CHUNK_SIZE + y));
                                slices[h].Set(x, y, blockId);
                                // blocks[h * d + y * GameSettings.CHUNK_SIZE + x] = blockId.Id;
                            }
                        }
                    }

                    receiveChunk.Invoke(new ChunkData { Slices = slices, chunkPosition = chunkPosition });
                }
                else
                {
                    chunkLoadEvent.Reset();
                }
            }
        }


        public void RequestChunk(Vector2Int pos)
        {
            chunkPositions.Enqueue(pos);
            chunkLoadEvent.Set();
        }
    }
}