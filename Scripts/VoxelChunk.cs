using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelChunk : MonoBehaviour
{

    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private MeshCollider meshCollider;
    [SerializeField] private BlockUVHolder uvHolder;

    public static int mapVerts = MapGenerator.GetMapVerts();

    private MeshData meshData;

    public void GenerateChunkInEditor(MapData mapData, AnimationCurve heightCurve, int lod, int sqrChunkSize)
    {
        meshData = MeshGenerator.GenerateTerrainMesh(mapData, heightCurve, lod, sqrChunkSize, uvHolder);
        DisplayMesh(meshData);
    }

    /// <summary>
    /// Displays the mesh in unity. This sets the vertices, uvs, and normals
    /// to those calculated in meshData. 
    /// </summary>
    /// <param name="meshData"></param>
    private void DisplayMesh(MeshData meshData)
    {
        if (meshData.vertices.Count > 65535) {
            Debug.LogError("mesh vertices exceeding unity max vertices per mesh of 65,535");
        }

        Mesh mesh = new Mesh();
        mesh.SetVertices(meshData.vertices);
        mesh.SetNormals(meshData.normals);
        mesh.SetUVs(0, meshData.uvs);
        mesh.SetIndices(meshData.indices, MeshTopology.Triangles, 0);

        mesh.RecalculateTangents();
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;

        // set texture;
    }
}
