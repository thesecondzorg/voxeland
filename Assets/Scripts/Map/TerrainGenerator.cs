using System;
using System.Collections.Generic;
using Map;
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

        public void InitAwake()
        {
            Seed = Random.Range(10, 10000);
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
                if (noiseFbm(blockGlobalPosition) > 0.00035f)
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

        float noise2Fbm(Vector3 pos)
        {
            float3 p = new float3(pos.x, pos.y, pos.z) / caveRate;
            float r = 0;
            for (int i = 1; i <= 5; i++)
            {
                r += noise.snoise(p + Seed / i);
            }

            return Mathf.Abs(r);
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

        public BlockSpecification GetBlockData(BlockId id)
        {
            return Blocks[(int) id.Id];
        }
    }
}