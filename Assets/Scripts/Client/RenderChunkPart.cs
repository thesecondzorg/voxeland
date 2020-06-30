using System;
using System.Diagnostics;
using System.Timers;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Client
{
    public class RenderChunkPart : MonoBehaviour
    {
        private MeshFilter meshFilter;
        private MeshCollider collider;
        private MeshRenderer meshRenderer;
        public Material Material;

        public ChunkViewRenderer.MeshBuilder meshBuilder;

        public bool Notify = false;

        public bool ready = false;
        // Start is called before the first frame update
        void Start()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            meshRenderer.material.SetVector("_TextureIndex",
                new Vector4((int) meshBuilder.blockId.Id, meshBuilder.damage));
        }

        // Update is called once per frame
        void Update()
        {
            if (ready)
            {
                Notify = true;
                ready = false;
            }
        }

        public void Reload()
        {
            Stopwatch timer = Stopwatch.StartNew();
            Mesh mesh = new Mesh
            {
                vertices = meshBuilder.vertices,
                normals = meshBuilder.normals,
                uv = meshBuilder.uv,
                triangles = meshBuilder.triangles
            };
            mesh.RecalculateBounds();
            mesh.UploadMeshData(false); //Finalize
            meshFilter = GetComponent<MeshFilter>();
            meshFilter.mesh = mesh;

            foreach (MeshCollider meshCollider in gameObject.GetComponents<MeshCollider>())
            {
                Destroy(meshCollider);
            }
            
            collider = gameObject.AddComponent<MeshCollider>();
            // collider.sharedMesh = mesh;
            ready = true;
            timer.Stop();
            Debug.Log("Render chunk took " + timer.ElapsedMilliseconds);
        }
    }
}