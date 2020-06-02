using System.Collections.Generic;
using Map;
using Test.Biomes;
using Test.Map;
using Unity.Jobs;
using UnityEngine;

namespace Test
{
    [CreateAssetMenu(fileName = "TerrainGenerator", menuName = "GameAssets/TerrainGenerator", order = 1)]
    public class TerrainGenerator : ScriptableObject
    {
        [SerializeField] private List<BlockSpecification> Blocks;
        [SerializeField] private List<BiomeSpecification> Biomes;

        [SerializeField] public int Seed;

        [SerializeField] public int WorldHeight = 256;


        public Vector2Int ToChunkPos(Vector3 pos)
        {
            return new Vector2Int((int) pos.x / GameSettings.CHUNK_SIZE, (int) pos.z / GameSettings.CHUNK_SIZE);
        }

        public Vector3Int ToBlockInChunkPos(Vector2Int chunkPos, Vector3 pos)
        {
            return new Vector3Int((int) pos.x % chunkPos.x, (int) pos.y, (int) pos.z % chunkPos.y);
        }

        public BiomeSpecification resolveBiome(Vector2Int chunkPosition)
        {
            // float flatness = Perlin.Noise(Seed + chunkPosition.x, Seed + chunkPosition.y);
            // float temperature = Perlin.Noise(Seed + chunkPosition.x, Seed + chunkPosition.y);
            // float humidity = Perlin.Noise(Seed + chunkPosition.x, Seed + chunkPosition.y);

            int biomeIdentifier =
                (int) ((Biomes.Count - 1) * Perlin.Noise(Seed + chunkPosition.x, Seed + chunkPosition.y));

            return Biomes[biomeIdentifier];
        }

        public BlockInfo Get(Vector3Int blockPosition)
        {
            Vector2Int chunkPosition = ToChunkPos(blockPosition);
            return Get(chunkPosition, ToBlockInChunkPos(chunkPosition, blockPosition));
        }

        public BlockId Get(float height, Vector3Int blockGlobalPosition)
        {
            // BiomeSpecification biomeSpecification = resolveBiome(chunkPosition);
            // float heightVariation = biomeSpecification.MaxHeight - biomeSpecification.MinHeight;
            // float height = Mathf.PerlinNoise(
            //                    Seed + chunkPosition.x * ChunkSize + blockInChunkPosition.x,
            //                    Seed + chunkPosition.y * ChunkSize + blockInChunkPosition.z)
            //                * heightVariation
            //                + biomeSpecification.MinHeight;
            // Debug.Log(height);
            
            if (blockGlobalPosition.y == 0)
            {
                // TODO badrock
                return new BlockId { Id = 1 };
            }

            if (blockGlobalPosition.y <= height)
            {
                if (Perlin.Noise(blockGlobalPosition.x / 10f, blockGlobalPosition.y / 10f, blockGlobalPosition.z / 10f) > 1.0 / blockGlobalPosition.y)
                {
                    return new BlockId { Id = 1 };
                }
            }

            return BlockId.AIR;
        }

        public BlockInfo Get(Vector2Int chunkPosition, Vector3Int blockInChunkPosition)
        {
            BiomeSpecification biomeSpecification = resolveBiome(chunkPosition);
            float heightVariation = biomeSpecification.MaxHeight - biomeSpecification.MinHeight;
            float height = Mathf.PerlinNoise(
                               Seed + chunkPosition.x * GameSettings.CHUNK_SIZE + blockInChunkPosition.x,
                               Seed + chunkPosition.y * GameSettings.CHUNK_SIZE + blockInChunkPosition.z)
                           * heightVariation
                           + biomeSpecification.MinHeight;
            
            Debug.Log(height);
            if ((float) blockInChunkPosition.y <= height)
            {
                return new BlockInfo
                {
                    BlockId = new BlockId {Id = 1},
                    BiomeId = new BiomeId(biomeSpecification.BiomeId)
                };
            }

            return new BlockInfo
            {
                BlockId = BlockId.AIR,
                BiomeId = new BiomeId(biomeSpecification.BiomeId)
            };
        }
        //
        // public ChunkData GetChunk(Vector2Int chunkPosition)
        // {
        //     ChunkSlice[] slices = new ChunkSlice[WorldHeight];
        //     List<BlockId[,]> tmpSlices = new List<BlockId[,]>();
        //     for (int h = 0; h < WorldHeight; h++)
        //     {
        //         tmpSlices.Add(new BlockId[GameSettings.CHUNK_SIZE, GameSettings.CHUNK_SIZE]);
        //     }
        //
        //     for (int x = 0; x < GameSettings.CHUNK_SIZE; x++)
        //     {
        //         for (int y = 0; y < GameSettings.CHUNK_SIZE; y++)
        //         {
        //             BiomeSpecification biomeSpecification = resolveBiome(chunkPosition);
        //             float heightVariation = biomeSpecification.MaxHeight - biomeSpecification.MinHeight;
        //             float height = Perlin.Noise(
        //                                (chunkPosition.x * GameSettings.CHUNK_SIZE + (float) x) / Seed,
        //                                (chunkPosition.y * GameSettings.CHUNK_SIZE + (float) y) / Seed)
        //                            * heightVariation
        //                            + biomeSpecification.MinHeight;
        //             Debug.Log(" " + new Vector2((chunkPosition.x * GameSettings.CHUNK_SIZE + (float) x) / Seed,
        //                 (chunkPosition.y * GameSettings.CHUNK_SIZE + (float) y) / Seed) + " : " + height);
        //             for (int h = 0; h < height; h++)
        //             {
        //                 tmpSlices[h][x, y] = Get(height,
        //                     new Vector3Int(chunkPosition.x * GameSettings.CHUNK_SIZE + x, h, chunkPosition.y * GameSettings.CHUNK_SIZE + y));
        //
        //                 // blockIds[x, y] = 
        //             }
        //         }
        //     }
        //
        //     for (var i = 0; i < tmpSlices.Count; i++)
        //     {
        //         slices[i] = new ChunkSlice(tmpSlices[i]);
        //     }
        //
        //     return new ChunkData {Slices = slices};
        // }

        public BlockSpecification GetBlockData(BlockId id)
        {
            return Blocks[(int) id.Id - 1];
        }
    }
}