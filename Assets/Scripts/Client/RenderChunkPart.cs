using System.Collections;
using System.Collections.Generic;
using Test;
using Test.Netowrker;
using UnityEngine;

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

        collider = GetComponent<MeshCollider>();
        collider.sharedMesh = mesh;

        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = Material;
        gameObject.layer = 5;
    }

    // Update is called once per frame
    void Update()
    {
    }
}