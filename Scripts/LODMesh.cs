using System.Collections;
using UnityEngine;

/// <summary>
/// Private class used by TerrainChunk. 
/// Each terrain chunk has a array of these meshes so it can switch between the correct level of details 
/// </summary>
public class LODMesh
{
    #region Fields
    public Mesh mesh;
    public bool hasRequestedMesh, hasMesh;
    private int lod;
    System.Action updateCallBack;
    #endregion

    #region Public Methods
    /// <summary>
    /// Constructor. It creates the object and sets the int lod to the parameter
    /// as well as the callBack lambda expression to the parameter. 
    /// </summary>
    /// <param name="lod">Level of Detail of this mesh.</param>
    /// <param name="updateCallBack">Method to call to update the terrain when the mesh is made. </param>
    public LODMesh(int lod, System.Action updateCallBack)
    {
        this.lod = lod;
        this.updateCallBack = updateCallBack;
    }

    /// <summary>
    /// This method requests for the mesh from the MapGenerator of the EndlessTerrain
    /// class. This method is called when the Terrain chunk needs to update its mesh
    /// but this object does not currently have a mesh.
    /// </summary>
    /// <param name="mapData"></param>
    public void RequestMesh(MapData mapData)
    {
        hasRequestedMesh = true;
        EndlessTerrain.threadHandler.RequestMeshData(mapData, lod, OnMeshDataReceived);
    }

    #endregion

    #region Private Methods
    /// <summary>
    /// This is the method that is called when the meshData has been generated. It will
    /// be called from the MapGenerator update method and it will create the mesh
    /// from the mesh data.
    /// </summary>
    /// <param name="meshData"></param>
    private void OnMeshDataReceived(MeshData meshData)
    {
        mesh = meshData.CreateMesh();
        hasMesh = true;
        updateCallBack();
    }
    #endregion

}