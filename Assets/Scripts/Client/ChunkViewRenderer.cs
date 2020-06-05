using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using Map;
using Mirror;
using Test;
using Test.Map;
using Test.Netowrker;
using UnityEngine;

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

        private Dictionary<BlockId, Transform> parts = new Dictionary<BlockId, Transform>();
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
            Debug.Log("Submit processing " + ChunkData.chunkPosition);
            renderStartTime = Time.fixedTime;
            ThreadPool.QueueUserWorkItem(GenObjectsViewAsync);
        }

        // Update is called once per frame
        void Update()
        {
            lock (ChunkData)
            {
                if (ready && enableColliderTargetStatus.HasValue)
                {
                    foreach (KeyValuePair<BlockId, Transform> part in parts)
                    {
                        UpdateCollider updateCollider = part.Value.gameObject.AddComponent<UpdateCollider>();
                        updateCollider.State = enableColliderTargetStatus.Value;
                    }

                    enableColliderTargetStatus = null;
                }
            }

            if (!ready && view != null)
            {
                ready = true;
                Debug.Log("Start rendering " + ChunkData.chunkPosition + "  " + view.mesh.Count);
                RenderChunk(ChunkData.chunkPosition, view);
                ChunkReadyCallBack.Invoke(ChunkData.chunkPosition, this);
                Debug.Log("Chunk " + ChunkData.chunkPosition + " rendered in " + (Time.fixedTime - renderStartTime));
            }
        }

        public void Initial(ChunkData chunk, ChunkData right, ChunkData left, ChunkData forward, ChunkData backward)
        {
            ChunkData = chunk;
            this.right = right;
            this.left = left;
            this.forward = forward;
            this.backward = backward;
        }

        private void RenderChunk(Vector2Int chunkPos, View ch)
        {
            for (int i = 0; i < ch.mesh.Count; ++i)
            {
                BlockSpecification blockData = terrainGenerator.GetBlockData(ch.mesh[i].blockId);
                RenderChunkPart go = Instantiate(ChunkPart, transform, false);
                go.name = $"Chunk {chunkPos.x}:{chunkPos.y} [{blockData.Name}]";
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
                go.meshBuilder = ch.mesh[i];
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
                parts[ch.mesh[i].blockId] = go.transform;
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

            return !Equals(blockId, neighborId);
        }

        public void GenObjectsViewAsync(object state)
        {
            try
            {
                view = GenObjectsView();
                Debug.Log("Processing finished " + ChunkData.chunkPosition);
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

        private View GenObjectsView()
        {
            Dictionary<BlockId, Constructor> constructors = new Dictionary<BlockId, Constructor>();

            for (int hi = 0; hi < terrainGenerator.WorldHeight - 1; ++hi)
            {
                for (int yi = 0; yi < GameSettings.CHUNK_SIZE; ++yi)
                {
                    for (int xi = 0; xi < GameSettings.CHUNK_SIZE; ++xi)
                    {
                        BlockId blockId = ChunkData.GetId(xi, hi, yi);
                        if (Equals(blockId, BlockId.AIR))
                        {
                            continue;
                        }

                        BlockSpecification spec = terrainGenerator.GetBlockData(blockId);

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

            View chunk_view = new View
            {
                // objects = GenObjects(chunk)
            };
            foreach (KeyValuePair<BlockId, Constructor> pair in constructors)
            {
                if (pair.Value.vertices.Count == 0)
                    continue;
                MeshBuilder mesh = new MeshBuilder
                {
                    blockId = pair.Key,
                    vertices = pair.Value.vertices.ToArray(),
                    normals = pair.Value.normals.ToArray(),
                    uv = pair.Value.uv.ToArray(),
                    triangles = pair.Value.triangles.ToArray()
                };
                chunk_view.mesh.Add(mesh);
            }

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
            foreach (KeyValuePair<BlockId,Transform> keyValuePair in parts)
            {
                Destroy(keyValuePair.Value.gameObject);
            }
            Destroy(this.gameObject);
        }
    }
}