using System.Collections;
using System.Collections.Generic;
using Test;
using Test.Netowrker;
using UnityEngine;

namespace Client
{
    public class RenderChunkPart : MonoBehaviour
    {
        private MeshFilter meshFilter;
        private MeshCollider collider;
        private MeshRenderer meshRenderer;
        public Material Material;

        public ChunkViewRenderer.MeshBuilder meshBuilder;

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
        }

        public void Reload()
        {
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
        }
    }
}