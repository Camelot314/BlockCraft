using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainChunk
{
    #region Variables
    private GameObject mesh;                                // Game object that is rendered
    private Vector2 position;                               // Position of the object in 2d space. 
    private Vector3 positionV3;                             // Position in 3D space.
    private Bounds bounds;                                  // bounds object to tell how far the player is from the object.
    private LODInfo[] detailLevels;                         // Array of LODInfo objects so the terrain can dynamically switch detail levels. 
    private LODMesh[] lodMeshes;                            // Array of LODMesh objects that correspond to the detail level infos.

    private MapData mapData;                                // Map data object that carries all the map data for this specific chunk.
    private bool mapDataReceived;                           // bool that will be true when the data has been recieved. 
    private int prevLODIndex = -1;                          // the previous level of detai. It is -1 when there is no previous. 
    private MeshRenderer meshRenderer;                      // Reference to the renderer of the chunk. For materials
    private MeshFilter meshFilter;                          // Reference to the filter of the chunk to change the mesh.
    private MeshCollider meshCollider;                      // collider for the mesh so it can do collisions
    private System.Action updateVisibleChunks;              // Lambda expression to the EndlessTerrain update visible chunks so it can call it when there is a change. 

    #endregion

    #region Constructor
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="chunkCoord">2d Vector corresponding to the location of the chunk in chunk space.</param>
    /// <param name="size">xz size of the chunk.</param>
    /// <param name="detailLevels">Array of LODInfo object that correspond to the level of detail that will be used</param>
    /// <param name="transform">transform of the endless terrain object so that objects are parented in inspector</param>
    /// <param name="material">material to use when painting uvs.</param>
    /// <param name="updateVisibleChunks">Lamda expression that is called when the mesh data has been generated.</param>
    public TerrainChunk(Vector2 chunkCoord, int size, LODInfo[] detailLevels, Transform transform, Material material, System.Action updateVisibleChunks)
    {
        this.detailLevels = detailLevels;
        this.updateVisibleChunks = updateVisibleChunks;

        position = chunkCoord * size;
        bounds = new Bounds(position, Vector2.one * size);
        positionV3 = new Vector3(position.x, 0, position.y);

        lodMeshes = new LODMesh[detailLevels.Length];
        for (int i = 0; i < detailLevels.Length; i++)
        {
            lodMeshes[i] = new LODMesh(detailLevels[i].lod, updateVisibleChunks);
        }

        mesh = new GameObject("Terrain Chunk");
        meshRenderer = mesh.AddComponent<MeshRenderer>();
        meshRenderer.material = material;
        meshFilter = mesh.AddComponent<MeshFilter>();
        meshCollider = mesh.AddComponent<MeshCollider>();

        mesh.transform.position = positionV3;
        mesh.transform.parent = transform;
        mesh.transform.localScale = Vector3.one;
        SetVisible(false);
        EndlessTerrain.threadHandler.RequtestMapData(position, OnMapDataReceived);
    }

    /// <summary>
    /// Constructor that takes in a premade mapData object. This is for when the data has been saved to a file. 
    /// </summary>
    /// <param name="chunkCoord"></param>
    /// <param name="size"></param>
    /// <param name="detailLevels"></param>
    /// <param name="transform"></param>
    /// <param name="material"></param>
    /// <param name="updateVisibleChunks"></param>
    public TerrainChunk(Vector2 chunkCoord, int size, LODInfo[] detailLevels, Transform transform, Material material, System.Action updateVisibleChunks, MapData mapData)
    {
        this.detailLevels = detailLevels;
        this.updateVisibleChunks = updateVisibleChunks;

        position = chunkCoord * size;
        bounds = new Bounds(position, Vector2.one * size);
        positionV3 = new Vector3(position.x, 0, position.y);

        lodMeshes = new LODMesh[detailLevels.Length];
        for (int i = 0; i < detailLevels.Length; i++)
        {
            lodMeshes[i] = new LODMesh(detailLevels[i].lod, updateVisibleChunks);
        }

        mesh = new GameObject("Terrain Chunk");
        meshRenderer = mesh.AddComponent<MeshRenderer>();
        meshRenderer.material = material;
        meshFilter = mesh.AddComponent<MeshFilter>();
        meshCollider = mesh.AddComponent<MeshCollider>();

        mesh.transform.position = positionV3;
        mesh.transform.parent = transform;
        mesh.transform.localScale = Vector3.one;
        SetVisible(false);
        this.mapData = mapData;
        mapDataReceived = true;
    }
    #endregion

    #region Public Methods

    /// <summary>
    /// Given a coordinate point it will locate the cube at the specified point.
    /// If fill is true then it will edit map data so that the specified location
    /// in the map (when generating the mesh) will be filled. Otherwise it will
    /// make sure that the specified location is emptied. 
    /// 
    /// It needs to look at the direction of the camera so that it can select the
    /// correct block (when adding you look at the surface when deleting you look through
    /// to block underneath). 
    /// 
    /// It also will not add a block if the player feet is occupying the location of the block.
    /// This is to prevent the player from adding a block and getting stuck inside the mesh.
    /// 
    /// 
    /// </summary>
    /// <param name="rayDirection">Direction the player is looking in</param>
    /// <param name="hitPoint">World space coordinate of the point where the ray hit (exact point where looking)</param>
    /// <param name="feetPos">Vector3 which is the coordinate of the player feet</param>
    /// <param name="fill">whether to fill the block or delete</param>
    /// <param name="blockType">The type of block to fill if fill is true</param>
    public void EditCube(Vector3 rayDirection, Vector3 hitPoint, Vector3 feetPos, bool fill, short blockType)
    {
        Vector3 localPoint = hitPoint - positionV3;
        Vector3 arraySize, arrayFloat, feetFloat;
        Vector3Int arrayIndex, feetIndex;
        
        arraySize = new Vector3(mapData.heightMap.GetLength(0), MeshGenerator.GetChunkHeight(), mapData.heightMap.GetLength(1));


        arrayFloat = localPoint + arraySize * 0.5f;
        feetFloat = feetPos + arraySize * 0.5f;

        arrayIndex = new Vector3Int(Mathf.FloorToInt(arrayFloat.x), Mathf.FloorToInt(arrayFloat.y), Mathf.FloorToInt(arrayFloat.z));
        arrayIndex = CorrectCoords(rayDirection, localPoint, arrayIndex, fill, true);

        if (!ValidCoords(arrayIndex))
        {
            return;
        }

        feetIndex = new Vector3Int(Mathf.FloorToInt(feetFloat.x), Mathf.FloorToInt(feetFloat.y), Mathf.FloorToInt(feetFloat.z));
        feetIndex = CorrectCoords(rayDirection, localPoint, feetIndex, fill, false);

        if (fill && Utilities.Equal(feetIndex, arrayIndex))
        {
            return;
        }


        mapData.EditCube(arrayIndex, fill, blockType);
        lodMeshes[0].hasMesh = false;
        lodMeshes[0].hasRequestedMesh = false;
        prevLODIndex = -1;
        UpdateChunk();

    }

    /// <summary>
    /// It will check to see if the there are any points on the mesh
    /// that are within the max view distance from the player position.
    /// If it is visible then it will loop through the detail levels to pick
    /// the lowest one that corresonds to that detailLevels distance metric. 
    /// If this LOD is different than the old one then it will request a new mesh
    /// and set it if the mesh is available. 
    /// </summary>
    public void UpdateChunk()
    {
        if (!mapDataReceived)
        {
            return;
        }
        float viewerSqrDistanceFromNearestPoint = bounds.SqrDistance(EndlessTerrain.playerPos);
        bool visible = viewerSqrDistanceFromNearestPoint <= (EndlessTerrain.maxViewDist * EndlessTerrain.maxViewDist);

        if (visible)
        {
            int lodIndex = 0;
            for (int i = 0; i < detailLevels.Length - 1; i++)
            {
                float levelDist = detailLevels[lodIndex].visibleDistThreashold;
                if (viewerSqrDistanceFromNearestPoint > levelDist * levelDist)
                {
                    lodIndex++;
                }
                else
                {
                    break;
                }
            }

            if (lodIndex != prevLODIndex)
            {
                LODMesh lodMesh = lodMeshes[lodIndex];
                if (lodMesh.hasMesh)
                {
                    prevLODIndex = lodIndex;
                    meshFilter.mesh = lodMesh.mesh;
                    meshCollider.sharedMesh = lodMesh.mesh;
                }
                else if (!lodMesh.hasRequestedMesh)
                {
                    lodMesh.RequestMesh(mapData);
                }
            }
            EndlessTerrain.visibileLastUpdate.Add(this);
        }
        SetVisible(visible);
    }

    /// <summary>
    /// makes the mesh visible or not depending on the parameter. 
    /// </summary>
    /// <param name="visible">bool true if chunk should be visible</param>
    public void SetVisible(bool visible)
    {
        mesh.SetActive(visible);
    }

    /// <summary>
    /// Returns true if the map has ever been changed by the player. 
    /// </summary>
    /// <returns>bool</returns>
    public bool HasChanged()
    {
        return mapData.HasChanged();
    }

    /// <summary>
    /// Returns the map data. This is used to save player changes. 
    /// </summary>
    /// <returns>mapData</returns>
    public MapData GetMapData()
    {
        return mapData;
    }

    
    #endregion

    #region Private Methods

    /// <summary>
    /// Checks the valilidy of the array coordinates. If they are valid coordinates then it will return true.
    /// </summary>
    /// <param name="coords">Vector3Int the array coordinates</param>
    /// <returns>true if valid index.</returns>
    private bool ValidCoords(Vector3Int coords)
    {
        if (coords.x < 0 || coords.x > MapGenerator.GetMapVerts() - 1)
        {
            return false;
        }
        if (coords.z < 0 || coords.z > MapGenerator.GetMapVerts() - 1)
        {
            return false;
        }
        if (coords.y < 0 || coords.y > MeshGenerator.GetChunkHeight() - 1)
        {
            return false;
        }
        return true;
    }


    /// <summary>
    /// Method that is called when the map data is recieved. 
    /// It will call the onMeshDataReceived. It will also 
    /// generate the Texture from the colorMap that is in
    /// the mapData class that it recieves. 
    /// </summary>
    /// <param name="mapData">Map data object</param>
    private void OnMapDataReceived(MapData mapData)
    {
        this.mapData = mapData;
        mapDataReceived = true;
        updateVisibleChunks();
    }




    /// <summary>
    /// Because when hitting a box you are technically hitting the boundary
    /// of the empty box that is visible. To get coordinates of a box that 
    /// shares this boundary it will adjust the hitpoint. It also needs the ray direction 
    /// so it can distinguish the top of the box from the bottom etc. If noBounds is true
    /// then the vector3Int will possibly have values that are not inside the array indicies.
    /// This is used to determine if the player is currently standing on the an index in the chunk.
    /// </summary>
    /// <param name="rayDirection">The direction the camera is looking</param>
    /// <param name="local">the local worldspace float coordinate</param>
    /// <param name="coords">the initial coordinates in the array</param>
    /// <param name="addBlock">whether the block is to be added</param>
    /// <param name="bouded">whether to use the bounds</param>
    /// <returns>Vector3Int corresponding to the indices of the chunk</returns>
    private Vector3Int CorrectCoords(Vector3 rayDirection, Vector3 local, Vector3Int coords, bool addBlock, bool bouded)
    {
        float x, y, z;
        x = local.x;
        y = local.y;
        z = local.z;

        if (Utilities.Equal(y % 0.5f, 0f))
        {
            if (!bouded)
            {
                coords.y = GetAdjusted(coords.y, rayDirection.y, addBlock);
            } else
            {
                coords.y = GetAdjusted(coords.y, MeshGenerator.GetChunkHeight(), rayDirection.y, addBlock);
            }
            
        }
        if (Utilities.Equal(x % 0.5f, 0f))
        {
            if (!bouded)
            {
                coords.x = GetAdjusted(coords.x, rayDirection.x, addBlock);
            } else
            {
                coords.x = GetAdjusted(coords.x, mapData.heightMap.GetLength(0), rayDirection.x, addBlock);
            }
            
        }
        if (Utilities.Equal(z % 0.5f, 0f))
        {
            if (!bouded)
            {
                coords.z = GetAdjusted(coords.z, rayDirection.z, addBlock);
            } else
            {
                coords.z = GetAdjusted(coords.z, mapData.heightMap.GetLength(1), rayDirection.z, addBlock);
            }
        }

        return coords;
    }

    #region Private static methods
    /// <summary>
    /// This method does the addition needed to adjust the array index.
    /// </summary>
    /// <param name="prior">the previous coordinate</param>
    /// <param name="max">the max array index possible</param>
    /// <param name="axisDirection">the float reprsenting the direction of the camera in the specific axis</param>
    /// <param name="add">whether or not the block is being added.</param>
    /// <returns>the adjusted coordinate of teh chunk BOUNDED</returns>
    private static int GetAdjusted(int prior, int max, float axisDirection, bool add)
    {
        int adjust;

        // offset is opposite when adding vs removing
        if (!add)
        {
            adjust = (axisDirection > 0) ? 0 : -1;     
        }
        else
        {
            adjust = (axisDirection > 0) ? -1 : 0;
        }

        int coordTemp = prior + adjust;

        if (coordTemp >= 0 && coordTemp < max)
        {
            return coordTemp;
        }

        return prior;

    }
    /// <summary>
    /// This method does the addition needed to adjust the array index. This will
    /// return a int that is not bounded by the array sizes.
    /// </summary>
    /// <param name="prior">the previous coordinate</param>
    /// <param name="axisDirection">the float reprsenting the direction of the camera in the specific axis</param>
    /// <param name="add">whether or not the block is being added.</param>
    /// <returns>int the adjusted coordinate of the chunk UNBOUNDED</returns>
    private static int GetAdjusted(int prior, float axisDirection, bool add)
    {
        int adjust;

        // offset is opposite when adding vs removing
        if (!add)
        {
            adjust = (axisDirection > 0) ? 0 : -1;     
        }
        else
        {
            adjust = (axisDirection > 0) ? -1 : 0;
        }

        int coordTemp = prior + adjust;

        return coordTemp;

    }
    #endregion
    #endregion

}
