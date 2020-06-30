using System;
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

        // Start is called before the first frame update
        void Start()
        {
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
                Debug.Log("Chunk " + ChunkData.chunkPosition  + " rendered in " +(Time.fixedTime - t)+ " with delay " +  ( t - renderStartTime) );
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

            if (parts.Values.All(p => p.Notify))
            {
                foreach (RenderChunkPart part in parts.Values)
                {
                    part.Notify = false;
                }
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
            for (int i = 0; i < ch.mesh.Count; ++i)
            {
                BlockId blockId = ch.mesh[i].blockId;
                if (!parts.TryGetValue(blockId, out RenderChunkPart go))
                {
                    go = Instantiate(ChunkPart, transform, false);
                    parts[blockId] = go;
                }
                BlockSpecification blockData = terrainGenerator.GetBlockData(blockId);
                go.name = $"Chunk {chunkPos.x}:{chunkPos.y} [{blockData.Name}]";
                go.meshBuilder = ch.mesh[i];
                go.Reload();

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

            }

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

        private bool TestSide(BlockId blockId, int x, int h, int y)
        {
            if (h < 0 || h >= terrainGenerator.WorldHeight)
            {
                return true;
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

            return neighborId.Equals( BlockId.AIR) ;
        }
        
        private void GenObjectsViewAsync(object state)
        {
            try
            {
                Stopwatch t = Stopwatch.StartNew();
                view = GenObjectsView();
                t.Stop();
                Debug.Log("Mesh generated in " + t.ElapsedMilliseconds);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public static Constructor FindConstructor(Dictionary<BlockId, Constructor> constructors, BlockId blockId)
        {
            if (!constructors.ContainsKey(blockId))
            {
                constructors[blockId] = new Constructor {blockId = blockId};
            }

            return constructors[blockId];
        }
        
        public static Constructor FindConstructor(List<Constructor> constructors, BlockId blockId)
        {
            foreach (Constructor constructor in constructors)
            {
                if (constructor.blockId.Equals(blockId))
                {
                    return constructor;
                }
            }
            Constructor cstr = new Constructor {blockId = blockId};
            constructors.Add(cstr);
            return cstr;
        }

        private View GenObjectsView()
        {
            //Dictionary<BlockId, Constructor> constructors = new Dictionary<BlockId, Constructor>(3);
            Stopwatch timer = Stopwatch.StartNew();
            List<Constructor> constructors = new List<Constructor>();

            for (int hi = 0; hi < terrainGenerator.WorldHeight - 1; ++hi)
            {
                ChunkSlice slice = ChunkData.GetSlice(hi);
                if (slice != null)
                {

                    for (int yi = 0; yi < GameSettings.CHUNK_SIZE; ++yi)
                    {
                        for (int xi = 0; xi < GameSettings.CHUNK_SIZE; ++xi)
                        {
                            BlockId blockId = slice.GetId(xi, yi);
                            if (Equals(blockId, BlockId.AIR))
                            {
                                continue;
                            }

                            // BlockSpecification spec = terrainGenerator.GetBlockData(blockId);

                            // Zpos
                            if (TestSide(blockId, xi, hi + 1, yi))
                            {
                                // uint destroyAnim = FindDestroyAnim(chunk, id, xi, hi, yi);
                                ChunkMesh.AddSide_ZP(FindConstructor(constructors, blockId), xi, hi, yi);
                            }

                            //
                            // // Zneg
                            if (TestSide(blockId, xi, hi - 1, yi))
                            {
                                // uint destroyAnim = FindDestroyAnim(chunk, id, xi, hi, yi);
                                ChunkMesh.AddSide_ZN(FindConstructor(constructors, blockId), xi, hi, yi);
                            }

                            // // Xpoz
                            if (TestSide(blockId, xi + 1, hi, yi))
                            {
                                // uint destroyAnim = FindDestroyAnim(chunk, id, xi, hi, yi);
                                ChunkMesh.AddSide_XP(FindConstructor(constructors, blockId), xi, hi, yi);
                            }

                            // // Xneg
                            if (TestSide(blockId, xi - 1, hi, yi))
                            {
                                // uint destroyAnim = FindDestroyAnim(chunk, id, xi, hi, yi);
                                ChunkMesh.AddSide_XN(FindConstructor(constructors, blockId), xi, hi, yi);
                            }

                            // // Ypoz

                            if (TestSide(blockId, xi, hi, yi + 1))
                            {
                                // uint destroyAnim = FindDestroyAnim(chunk, id, xi, hi, yi);
                                ChunkMesh.AddSide_YP(FindConstructor(constructors, blockId), xi, hi, yi);
                            }


                            // // Ypoz

                            if (TestSide(blockId, xi, hi, yi - 1))
                            {
                                // uint destroyAnim = FindDestroyAnim(chunk, id, xi, hi, yi);
                                ChunkMesh.AddSide_YN(FindConstructor(constructors, blockId), xi, hi, yi);
                            }
                        }
                    }
                }
            }

            View chunk_view = new View
            {
                // objects = GenObjects(chunk)
            };
            foreach ( Constructor pair in constructors)
            {
                if (pair.vertices.Count == 0)
                    continue;
                MeshBuilder mesh = new MeshBuilder
                {
                    blockId = pair.blockId,
                    vertices = pair.vertices.ToArray(),
                    normals = pair.normals.ToArray(),
                    uv = pair.uv.ToArray(),
                    triangles = pair.triangles.ToArray()
                };
                chunk_view.mesh.Add(mesh);
            }
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
            public int[] triangles;
            public float damage;
        }

        public class View
        {
            public List<MeshBuilder> mesh = new List<MeshBuilder>();
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