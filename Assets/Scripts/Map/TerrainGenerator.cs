using System;
using System.Collections.Generic;
using Map;
using Noise;
using Test;
using Test.Biomes;
using Test.Map;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Random = UnityEngine.Random;

namespace Map
{
    [CreateAssetMenu(fileName = "TerrainGenerator", menuName = "GameAssets/TerrainGenerator", order = 1)]
    public class TerrainGenerator : ScriptableObject
    {
        [SerializeField] private List<BlockSpecification> Blocks;
        [SerializeField] private List<BiomeSpecification> Biomes;

        [SerializeField] public int Seed;

        [SerializeField] public int WorldHeight = 256;
        [SerializeField] public Material TerrainMaterial;
        public Texture2DArray texture2DArray;
        float3 caveRate = new float3(1, 2, 1);
        private OpenSimplex2S noiseGen; 
        public void InitAwake()
        {
            Seed = Random.Range(10, 10000);
            noiseGen = new OpenSimplex2S(Seed);
            texture2DArray = new Texture2DArray(128, 128, Blocks.Count, TextureFormat.RGB24, false);
            for (int i = 0; i < Blocks.Count; i++)
            {
                Blocks[i].blockId = (uint) (i);
                if (Blocks[i].ViewType == ViewType.Block)
                {
                    texture2DArray.SetPixels(Blocks[i].Texture.GetPixels(0), i);
                }
            }

            texture2DArray.Apply();
            TerrainMaterial.SetTexture("_TextureArray", texture2DArray);
        }

        public ChunkData GenerateNewChunk(Vector2Int chunkPosition)
        {
            ChunkSlice[] slices = new ChunkSlice[GameSettings.WORLD_HEIGHT];
            for (int i = 0; i < GameSettings.WORLD_HEIGHT; i++)
            {
                slices[i] = new ChunkSlice(GameSettings.CHUNK_SIZE);
            }

            for (int x = 0; x < GameSettings.CHUNK_SIZE; x++)
            {
                float tx = (chunkPosition.x * GameSettings.CHUNK_SIZE  + x ) ;
                for (int y = 0; y < GameSettings.CHUNK_SIZE; y++)
                {
                    BiomeSpecification biomeSpecification = resolveBiome(chunkPosition);
                    float heightVariation = biomeSpecification.MaxHeight - biomeSpecification.MinHeight;
                    float ty = (chunkPosition.y * GameSettings.CHUNK_SIZE  + y)  ;
                    double height = Map.Perlin.Fbm(tx / 50, ty / 50, 5) *heightVariation+ biomeSpecification.MinHeight;
                    for (int h = 0; h < height; h++)
                    {
                        BlockId blockId = Get(
                            (float) height,
                            new Vector3Int(chunkPosition.x * GameSettings.CHUNK_SIZE + x, h,
                                chunkPosition.y * GameSettings.CHUNK_SIZE + y));
                        slices[h].Set(x, y, blockId);
                        // blocks[h * d + y * GameSettings.CHUNK_SIZE + x] = blockId.Id;
                    }
                }
            }

            return new ChunkData {slices = slices, chunkPosition = chunkPosition};
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
            Vector2Int chunkPosition = GameSettings.ToChunkPos(blockPosition);
            return Get(chunkPosition, ToBlockInChunkPos(chunkPosition, blockPosition));
        }

        public BlockId Get(float height, Vector3Int blockGlobalPosition)
        {
            BiomeSpecification biomeSpecification =
                resolveBiome(new Vector2Int(blockGlobalPosition.x, blockGlobalPosition.z));
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
                return new BlockId {Id = 1};
            }

            if (blockGlobalPosition.y <= height)
            {
                // float airNoise = Mathf.Abs(Perlin.Noise(blockGlobalPosition.x / 20f, blockGlobalPosition.y / 30f,
                //     blockGlobalPosition.z / 20f));
                //float airNoise = Mathf.Abs(noise.cnoise(new float3(blockGlobalPosition.x, blockGlobalPosition.y, blockGlobalPosition.z) / caveRate  + Seed));
                // double airNoise = Fbm(blockGlobalPosition, 5);
                double airNoise = 1;// noiseGen.Noise3_Classic(blockGlobalPosition.x, blockGlobalPosition.y, blockGlobalPosition.z);
                if (airNoise > 0.05335f)
                {
                    if (height - blockGlobalPosition.y < 5)
                    {
                        return new BlockId {Id = biomeSpecification.topBlock.blockId};
                    }
                    else
                    {
                        return new BlockId {Id = 2};
                    }
                }
            }

            return BlockId.AIR;
        }

        public double Fbm(Vector3Int coord, int octave)
        {
            double f = 0.0;
            float w = 0.5f;
            for (int i = 0; i < octave; i++)
            {
                f += w * noiseGen.Noise3_XZBeforeY(coord.x, coord.y, coord.z);
                coord *= 2;
                w *= 0.5f;
            }

            return f;
        }
        
        
        
        float noiseFbm(Vector3 pos)
        {
            Vector3 p = pos * caveRate;
            float r = 0;
            for (int i = 1; i <= 5; i++)
            {
                r += Perlin.Noise(new Vector3(1 / (p.x + Seed / i), 1 / (p.y + Seed / i), 1 / (p.z + Seed / i)));
            }

            return Mathf.Abs(r / 5);
        }

        float Noise2Fbm(float x, float y)
        {
            float r = 0;
            for (float i = 1; i <= 5; i++)
            {
                r += Perlin.Noise(x/i + Seed, y / i + Seed);
            }

            return Mathf.Abs(r / 5);
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

        public BlockSpecification GetBlockData(BlockId id)
        {
            return Blocks[(int) id.Id];
        }
    }
}