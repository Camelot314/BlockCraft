using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class that keeps track of the data for a terrain chunk.
/// This is both the chunk coordinate as well as the mapData. 
/// </summary>
[System.Serializable]
public class SavedTerrainChunk
{
    #region Fields
    private float[] chunkCoordinate;
    private SavedMapData savedMap;
    #endregion

    #region Constructor
    /// <summary>
    /// Constructor. Saves the chunk coordinate and the mapData.
    /// </summary>
    /// <param name="chunkCoord">Vector2 chunk coordinate</param>
    /// <param name="chunk">TerrainChunk with the mapData.</param>
    public SavedTerrainChunk(Vector2 chunkCoord, TerrainChunk chunk)
    {
        chunkCoordinate = new float[2];

        chunkCoordinate[0] = chunkCoord.x;
        chunkCoordinate[1] = chunkCoord.y;

        savedMap = new SavedMapData(chunk.GetMapData());
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Returns the Chunk coordinate from the saved coordinate. 
    /// </summary>
    /// <returns>Vector2 chunk coordinate.</returns>
    public Vector2 GetChunkCoord()
    {
        return new Vector2(chunkCoordinate[0], chunkCoordinate[1]);
    }

    /// <summary>
    /// Returns the MapData object from savedMap.
    /// </summary>
    /// <returns>MapData</returns>
    public MapData GetMapData()
    {
        return savedMap.GetMapData();
    }
    #endregion
}
