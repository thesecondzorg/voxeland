﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using Map;
using Mirror;
using Test;
using Test.Map;
using Test.Netowrker;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Debug = UnityEngine.Debug;

namespace Client
{
    public class ChunkViewRenderer : MonoBehaviour
    {
        public ChunkData ChunkData;
        private ChunkData right;
        private ChunkData left;
        private ChunkData forward;
        private ChunkData backward;

        [SerializeField] public RenderChunkPart ChunkPart;
        [SerializeField] public TerrainGenerator terrainGenerator;

        private Dictionary<BlockId, RenderChunkPart> parts = new Dictionary<BlockId, RenderChunkPart>();
        private View view;
        private bool ready = false;
        public Action<Vector2Int, ChunkViewRenderer> ChunkReadyCallBack;
        public Nullable<bool> enableColliderTargetStatus;

        private float renderStartTime;

        private MeshFilter meshFilter;
        private MeshCollider collider;
        private MeshRenderer meshRenderer;
        public MeshBuilder meshBuilder;
        public bool Notify = false;


        // Start is called before the first frame update
        void Start()
        {
            meshFilter = GetComponent<MeshFilter>();

            name = $"Chunk {ChunkData.chunkPosition.x}, {ChunkData.chunkPosition.y}";
            transform.position = new Vector3(
                ChunkData.chunkPosition.x * GameSettings.CHUNK_SIZE,
                0,
                ChunkData.chunkPosition.y * GameSettings.CHUNK_SIZE);
            // Debug.Log("Submit processing " + ChunkData.chunkPosition);
            renderStartTime = Time.fixedTime;
        }

        void Update()
        {
            if (!ready && view != null)
            {
                ready = true;
                // Debug.Log("Start rendering " + ChunkData.chunkPosition + "  " + view.mesh.Count);
                float t = Time.fixedTime;
                RenderChunk(ChunkData.chunkPosition, view);
                Debug.Log("Chunk " + ChunkData.chunkPosition + " rendered in " + (Time.fixedTime - t) + " with delay " +
                          (t - renderStartTime));
            }

            // lock (ChunkData)
            {
                if (ready && enableColliderTargetStatus.HasValue)
                {
                    foreach (KeyValuePair<BlockId, RenderChunkPart> part in parts)
                    {
                        UpdateCollider updateCollider = part.Value.gameObject.AddComponent<UpdateCollider>();
                        updateCollider.State = enableColliderTargetStatus.Value;
                    }

                    enableColliderTargetStatus = null;
                }
            }

            if (Notify)
            {
                Notify = false;
                ChunkReadyCallBack.Invoke(ChunkData.chunkPosition, this);
            }
        }


        public void Initial(ChunkData chunk, ChunkData right, ChunkData left, ChunkData forward, ChunkData backward)
        {
            ChunkData = chunk;
            this.right = right;
            this.left = left;
            this.forward = forward;
            this.backward = backward;
            ready = false;
            view = null;
            ThreadPool.QueueUserWorkItem(GenObjectsViewAsync);
        }

        private void RenderChunk(Vector2Int chunkPos, View ch)
        {
            meshBuilder = ch.mesh;
            Reload();
            // for (int i = 0; i < ch.mesh.Count; ++i)
            // {
            //     BlockId blockId = ch.mesh[i].blockId;
            //     if (!parts.TryGetValue(blockId, out RenderChunkPart go))
            //     {
            //         go = Instantiate(ChunkPart, transform, false);
            //         parts[blockId] = go;
            //     }
            //
            //     BlockSpecification blockData = terrainGenerator.GetBlockData(blockId);
            //     go.name = $"Chunk {chunkPos.x}:{chunkPos.y} [{blockData.Name}]";
            //     go.meshBuilder = ch.mesh[i];
            //     go.Reload();

            // go.gameObject.layer = 8;
            // go.GetComponent<MeshRenderer>().material.SetTexture("_TextureArray" , terrainGenerator.texture2DArray);
            // go.layer = 5;
            //     // new GameObject(
            //         // 
            //     // {
            //         // layer = 5
            //     // };
            // //go.AddComponent<ChunkCleanUp>();
            // // go.transform.SetParent(transform, false);
            // Mesh mesh = new Mesh
            // {
            //     vertices = ch.mesh[i].vertices,
            //     normals = ch.mesh[i].normals,
            //     uv = ch.mesh[i].uv,
            //     triangles = ch.mesh[i].triangles
            // };
            // mesh.RecalculateBounds();
            // mesh.UploadMeshData(true); //Finalize
            //
            // MeshFilter meshFilter = go.GetComponent<MeshFilter>();
            // meshFilter.mesh = mesh;
            //
            // MeshCollider collider = go.GetComponent<MeshCollider>();
            // collider.sharedMesh = meshFilter.sharedMesh;
            //
            // MeshRenderer meshRenderer = go.GetComponent<MeshRenderer>();
            // meshRenderer.material = Material;
            // ChunkMesh.TextureIndex texture = ch.mesh[i].texture;

            // if (texture.Material == BlockDef.MaterialType.Fluid)
            // {
            //     meshRenderer.material = fluidMaterial;
            // }
            // else
            // {
            //     meshRenderer.material = m_material;
            //     Vector2 textureOffset = new Vector2Int(texture.Id % 16, texture.Id / 16);
            //     meshRenderer.material.SetVector(mainTextureOffsetId, textureOffset);
            //     if (texture.Destroy > 0)
            //     {
            //         Vector2 textureDestroyOffset = new Vector2(texture.Destroy, 0);
            //         meshRenderer.material.SetVector(detailedTextureOffsetId, textureDestroyOffset);
            //     }
            // }

            //go.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

            // go.transform.position =
            //     new Vector3(chunkPos.x * ChunkGenerator.g_size, 0.0f, chunkPos.y * ChunkGenerator.g_size);
            // }

            // foreach (ChunkMesh.ObjectView view in ch.objects)
            // {
            //     GameObject go = GameObject.Instantiate(
            //         Resources.Load<GameObject>("Prefabs/" + view.Id),
            //         m_root_object.transform);
            //     Destroy(go.GetComponent<MeshCollider>());
            //     Vector3 pos = new Vector3(
            //         chunkPos.x * ChunkGenerator.g_size + view.Position.x,
            //         ChunkGenerator.g_size + chunkPos.y * ChunkGenerator.g_size + view.Position.z - 0.5f,
            //         -ChunkGenerator.g_size + view.Position.y);
            //     go.name = "Object " + pos;
            //     go.transform.localScale = Vector3.one;
            //     go.transform.position = pos;
            //     Vector3 eulerAngles = go.transform.eulerAngles;
            //     go.transform.localRotation = Quaternion.Euler(eulerAngles.x, view.Rotation.y * 90 - 180, eulerAngles.z);
            //     go_list.Add(go);
            // }
        }

        private void Reload()
        {
            Stopwatch timer = Stopwatch.StartNew();
            Mesh mesh = new Mesh
            {
                vertices = meshBuilder.vertices,
                normals = meshBuilder.normals,
                uv = meshBuilder.uv,
                uv2 = meshBuilder.tiles,
                triangles = meshBuilder.triangles
            };
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.UploadMeshData(false); //Finalize
            meshFilter.mesh = mesh;

            foreach (MeshCollider meshCollider in gameObject.GetComponents<MeshCollider>())
            {
                Destroy(meshCollider);
            }
            
            collider = gameObject.AddComponent<MeshCollider>();
            Notify = true;
            timer.Stop();
            Debug.Log("Render chunk took " + timer.ElapsedMilliseconds);
        }

        private bool TestSide(BlockSpecification spec, int x, int h, int y)
        {
            if (h < 0 || h >= terrainGenerator.WorldHeight)
            {
                return false;
            }

            BlockId neighborId;
            if (x >= GameSettings.CHUNK_SIZE)
            {
                neighborId = right.GetId(0, h, y);
            }
            else if (x < 0)
            {
                neighborId = left.GetId(GameSettings.CHUNK_SIZE - 1, h, y);
            }
            else if (y >= GameSettings.CHUNK_SIZE)
            {
                neighborId = forward.GetId(x, h, 0);
            }
            else if (y < 0)
            {
                neighborId = backward.GetId(x, h, GameSettings.CHUNK_SIZE - 1);
            }
            else
            {
                neighborId = ChunkData.GetId(x, h, y);
            }

            return neighborId.Equals(BlockId.AIR);
        }

        private void GenObjectsViewAsync(object state)
        {
            try
            {
                Stopwatch t = Stopwatch.StartNew();
                view = GenObjectsView();
                t.Stop();
                Debug.Log($"Mesh generated in {t.ElapsedMilliseconds} for chunk {ChunkData.chunkPosition}");
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        Vector3Int LEFT = new Vector3Int(1, 0, 0);
        Vector3Int RIGHT = new Vector3Int(-1, 0, 0);
        Vector3Int FORWARD = new Vector3Int(0, 0, 1);
        Vector3Int BACKWARD = new Vector3Int(0, 0, -1);
        Vector3Int UP = new Vector3Int(0, 1, 0);
        Vector3Int DOWN = new Vector3Int(0, -1, 0);

        private View GenObjectsView()
        {
            Stopwatch timer = Stopwatch.StartNew();
            Constructor solidConstructor = new Constructor();
            Constructor liquidConstructor = new Constructor();
            for (int hi = 0; hi < GameSettings.WORLD_HEIGHT - 1; ++hi)
            {
                ChunkSlice slice = ChunkData.GetSlice(hi);
                if (slice == null) continue;
                for (int yi = 0; yi < GameSettings.CHUNK_SIZE; ++yi)
                {
                    for (int xi = 0; xi < GameSettings.CHUNK_SIZE; ++xi)
                    {
                        BlockId blockId = slice.GetId(xi, yi);
                        if (Equals(blockId, BlockId.AIR))
                        {
                            continue;
                        }

                        BlockSpecification spec = terrainGenerator.GetBlockData(blockId);
                        Constructor constructor = spec.IsSolid ? solidConstructor : liquidConstructor;

                        // Zpos
                        Vector2 material = new Vector2(blockId.Id, 0);
                        if (TestSide(spec, xi, hi + 1, yi))
                        {
                            ChunkMesh.AddSide_ZP(constructor, xi, hi, yi, material);
                        }

                        // Zneg
                        if (TestSide(spec, xi, hi - 1, yi))
                        {
                            ChunkMesh.AddSide_ZN(constructor, xi, hi, yi, material);
                        }

                        // Xpoz
                        if (TestSide(spec, xi + 1, hi, yi))
                        {
                            ChunkMesh.AddSide_XP(constructor, xi, hi, yi, material);
                        }

                        // Xneg
                        if (TestSide(spec, xi - 1, hi, yi))
                        {
                            ChunkMesh.AddSide_XN(constructor, xi, hi, yi, material);
                        }

                        // Ypoz
                        if (TestSide(spec, xi, hi, yi + 1))
                        {
                            ChunkMesh.AddSide_YP(constructor, xi, hi, yi, material);
                        }

                        // Ypoz
                        if (TestSide(spec, xi, hi, yi - 1))
                        {
                            ChunkMesh.AddSide_YN(constructor, xi, hi, yi, material);
                        }
                    }
                }
            }

            View chunk_view = new View
            {
                // objects = GenObjects(chunk)
            };

            MeshBuilder mesh = new MeshBuilder
            {
                blockId = solidConstructor.blockId,
                vertices = solidConstructor.vertices.ToArray(),
                normals = solidConstructor.normals.ToArray(),
                uv = solidConstructor.uv.ToArray(),
                tiles = solidConstructor.tiles.ToArray(),
                triangles = solidConstructor.triangles.ToArray()
            };
            chunk_view.mesh = mesh;

            timer.Stop();
            Debug.Log("Mesh generation finished in " + timer.ElapsedMilliseconds);
            return chunk_view;
        }

        public class Constructor : ChunkMesh.Constructor
        {
            public BlockId blockId;
        }

        public struct MeshBuilder
        {
            public BlockId blockId;
            public Vector3[] vertices;
            public Vector3[] normals;
            public Vector2[] uv;
            public Vector2[] tiles;
            public int[] triangles;
            public float damage;
        }

        public class View
        {
            public MeshBuilder mesh;
            // public List<ObjectView> objects = new List<ObjectView>();
        }

        public void Unload()
        {
            foreach (KeyValuePair<BlockId, RenderChunkPart> keyValuePair in parts)
            {
                Destroy(keyValuePair.Value.gameObject);
            }

            Destroy(this.gameObject);
        }
    }
}