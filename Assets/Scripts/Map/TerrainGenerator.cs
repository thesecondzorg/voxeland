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
        [SerializeField] public Material TerrainMaterial;

        [SerializeField] private List<BlockSpecification> Blocks;
        [SerializeField] private List<BiomeSpecification> Biomes;

        [SerializeField] public int Seed;

        [SerializeField] public int WorldHeight = 256;

        float3 caveRate = new float3(1, 2, 1);
        private OpenSimplex2S noiseSurface;
        private OpenSimplex2S noiseBottom;

        public void InitAwake(bool client)
        {
            Seed = Random.Range(10, 10000);
            noiseSurface = new OpenSimplex2S(Seed);
            noiseBottom = new OpenSimplex2S(Seed * 2);
            
            BlockSpecification[] blocks = Resources.LoadAll<BlockSpecification>("Blocks");
            for (int i = 0; i < blocks.Length; i++)
            {
                blocks[i].blockId = (uint) (i);
                BlockId.Blocks[i] = new BlockId(i);
            }

            if (client)
            {
                Texture2DArray texture2DArray = new Texture2DArray(128, 128, blocks.Length, TextureFormat.RGB24, true);
                for (int i = 0; i < blocks.Length; i++)
                {
                    if (blocks[i].ViewType == ViewType.Block)
                    {
                        texture2DArray.SetPixels(blocks[i].Texture.GetPixels(0), i);
                    }
                }

                texture2DArray.Apply();
                TerrainMaterial.SetTexture("_TextureArray", texture2DArray);
            }
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
                float tx = (chunkPosition.x * GameSettings.CHUNK_SIZE + x);
                for (int y = 0; y < GameSettings.CHUNK_SIZE; y++)
                {
                    BiomeSpecification biomeSpecification = resolveBiome(chunkPosition);
                    int heightVariation = biomeSpecification.MaxHeight - biomeSpecification.MinHeight;
                    float ty = (chunkPosition.y * GameSettings.CHUNK_SIZE + y);
                    
                    int top = biomeSpecification.MinHeight + (int) (Noise2Fbm(tx / 100, ty / 100, 5, noiseSurface) * heightVariation) ;
                    int bottom = 0;// (int)(biomeSpecification.MinHeight * Noise2Fbm(tx / 200, ty / 200, 10, noiseBottom)) + heightVariation;

                    for (int h = top; h >= bottom; h--)
                    {
                        BlockId blockId = Get(
                            top,
                            bottom,
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

        public BlockId Get(int top, int bottom, Vector3Int blockGlobalPosition)
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

            if (blockGlobalPosition.y <= top)
            {
                // float airNoise = Mathf.Abs(Perlin.Noise(blockGlobalPosition.x / 20f, blockGlobalPosition.y / 30f,
                //     blockGlobalPosition.z / 20f));
                //float airNoise = Mathf.Abs(noise.cnoise(new float3(blockGlobalPosition.x, blockGlobalPosition.y, blockGlobalPosition.z) / caveRate  + Seed));
                // double airNoise = Fbm(blockGlobalPosition, 5);
                double
                    airNoise = 1; // noiseGen.Noise3_Classic(blockGlobalPosition.x, blockGlobalPosition.y, blockGlobalPosition.z);
                if (airNoise > 0.05335f)
                {
                    if (top - blockGlobalPosition.y < 5)
                    {
                        return BlockId.of(biomeSpecification.topBlock.blockId);
                    }
                    else
                    {
                        return BlockId.of(2);
                    }
                }
            }

            return BlockId.AIR;
        }
        
        public float Noise2Fbm(float x, float y, int octave, OpenSimplex2S noiseGen)
        {
            double f = 0.0f;
            var w = 0.5f;
            for (var i = 0; i < octave; i++)
            {
                f += w * noiseGen.Noise2(x,y);
                x *= 2.0f;
                y *= 2.0f;
                w *= 0.5f;
            }

            return Mathf.Abs((float) f);
        }

        // double Noise2Fbm(float x, float y, int octaves, OpenSimplex2S noiseGen)
        // {
        //     double r = 0;
        //     for (float i = 1; i <= octaves; i++)
        //     {
        //         r += noiseGen.Noise2(x / i, y / i);
        //     }
        //
        //     return r / octaves;
        // }

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