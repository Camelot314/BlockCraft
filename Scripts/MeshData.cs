using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Keeps track of all the data needed to make a single mesh.
/// </summary>
public class MeshData
{
    #region Fields
    public List<Vector3> vertices;
    public List<Vector3> normals;
    public List<Vector2> uvs;
    public List<int> indices;
    public Dictionary<Vector3Int, int> coordinateMap;
    #endregion

    #region Constructor
    /// <summary>
    /// Constructor
    /// </summary>
    public MeshData()
    {
        vertices = new List<Vector3>();
        normals = new List<Vector3>();
        uvs = new List<Vector2>();
        indices = new List<int>();
        coordinateMap = new Dictionary<Vector3Int, int>();
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Creates and returns a mesh object from the info in the mesh 
    /// data. It will set the vertices, uvs, indices, and normals.
    /// This method can only run in the MAIN UNITY THREAD.
    /// </summary>
    /// <returns>A mesh object</returns>
    public Mesh CreateMesh()
    {
        if (vertices.Count > 65535)
        {
            Debug.LogError("mesh vertices exceeding unity max vertices per mesh of 65,535");
        }

        Mesh mesh = new Mesh();
        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);
        mesh.SetIndices(indices, MeshTopology.Triangles, 0);

        mesh.RecalculateTangents();
        return mesh;
    }
    #endregion
}
