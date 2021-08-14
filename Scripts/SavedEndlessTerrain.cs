using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Saves all the information from all the terrainChunks.
/// </summary>
[System.Serializable]
public class SavedEndlessTerrain
{
    #region Fields
    private SavedTerrainChunk[] savedChunks;
    #endregion

    #region Construtor
    /// <summary>
    /// Constructor. Takes the endless terrain script and gets
    /// the list of saved chunks.
    /// </summary>
    /// <param name="data">EndlessTerrain object to save</param>
    public SavedEndlessTerrain(EndlessTerrain data)
    {
        savedChunks = data.GetChangedChunks();
    }

    /// <summary>
    /// Default constructor for when there is no data to load in.
    /// It creates a savedTerrainChunk array of size 0.
    /// </summary>
    public SavedEndlessTerrain()
    {
        savedChunks = new SavedTerrainChunk[0];
    }
    #endregion

    #region Public methods
    /// <summary>
    /// Gets the array of saved chunks
    /// </summary>
    /// <returns>Array of saved chunk objects</returns>
    public SavedTerrainChunk[] GetSavedChunks()
    {
        return savedChunks;
    }

    #endregion

}
