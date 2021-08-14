using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SavedMapData
{
    #region Fields
    private float[,] noiseMap;
    private short[,,] mapchunk;
    private int leafSeed;
    #endregion

    #region Constructor
    /// <summary>
    /// Constructor takes in a mapData object and saves
    /// the treeeLeafSeed, the highResChunk short array
    /// and the noiseMap float array. 
    /// </summary>
    /// <param name="data">MapDataObject to save</param>
    public SavedMapData(MapData data)
    {
        leafSeed = data.treeLeafSeed;
        mapchunk = data.GetHighResChunkArray();
        noiseMap = data.heightMap;
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Returns a Mapdata object from the saved parameters.
    /// This includes the float[,] array for the noiseMap, 
    /// the short[,,] array for the highResChunk,
    /// and the seedValue for the leafs.
    /// </summary>
    /// <returns>MapData object.</returns>
    public MapData GetMapData()
    {
        return new MapData(noiseMap, mapchunk, leafSeed);
    }
    #endregion
}
