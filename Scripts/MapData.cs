using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Class that holds the necessary info to make a map and mesh
/// </summary>
public class MapData
{

    #region Fields
    public readonly float[,] heightMap, treeMap;
    public readonly int treeLeafSeed; 
    private bool hasNoiseChunk, changed;
    private Dictionary<Vector3Int, short> adjustments;
    private short[,,] highResChunkArray;
    #endregion

    #region Constructor
    /// <summary>
    /// Constructor. 
    /// </summary>
    /// <param name="heightMap">HeightMap for the mesh.</param>
    /// <param name="treeMap">HeightMap for all the trees in the mesh</param>
    /// <param name="treeLeafSeed">Seed value for generating the destoryed leaves on each tree</param>
    public MapData(float[,] heightMap, float[,] treeMap, int treeLeafSeed)
    {
        this.heightMap = heightMap;
        this.treeMap = treeMap;
        this.treeLeafSeed = treeLeafSeed;
        adjustments = new Dictionary<Vector3Int, short>();
    }

    /// <summary>
    /// Constructor used to make the mapData for when the information has already been saved to a file.
    /// </summary>
    /// <param name="heightMap"></param>
    /// <param name="highResChunkArray"></param>
    /// <param name="treeLeafSeed"></param>
    public MapData(float[,] heightMap, short[,,] highResChunkArray, int treeLeafSeed)
    {
        this.heightMap = heightMap;
        this.highResChunkArray = highResChunkArray;
        this.treeLeafSeed = treeLeafSeed;
        hasNoiseChunk = true;
        changed = true;
        treeMap = new float[heightMap.GetLength(0), heightMap.GetLength(1)];
        adjustments = new Dictionary<Vector3Int, short>();
    }
    #endregion

    #region Public Methods

    /// <summary>
    /// This will add an element to the dictionary to specify
    /// whether a specific cube given by its index location is 
    /// filled.
    /// 
    /// It will only add the key value pair if it does not already
    /// exist in the dictinoary.
    /// </summary>
    /// <param name="location">indexed location used by BlockIds</param>
    /// <param name="fill">true if filled false otherwise</param>
    /// <param name="blockType">block type to fill with if fill is true</param>
    public void EditCube(Vector3Int location, bool fill, short blockType)
    {
        blockType = !fill ? (short) 0 : blockType;

        if (adjustments.ContainsKey(location))
        {

            if (!fill && adjustments[location] == 0)
            {
                return;
            }

            if (fill && adjustments[location] > 0)
            {
                return;
            }

            adjustments.Remove(location);
            adjustments.Add(location, blockType);
            changed = true;
            return;
        }
        changed = true;
        adjustments.Add(location, blockType);

    }

    /// <summary>
    /// This method returns the dictionary so the mesh 
    /// generator can make adjustments using the adjustments
    /// outlined in the table. 
    /// </summary>
    /// <returns>the ajdustment dictionary</returns>
    public Dictionary<Vector3Int, short> GetAdjustments()
    {
        return adjustments;
    }

    /// <summary>
    /// Sets the highResNoiseChunk 3d short array
    /// 
    /// Saves time in mesh generation.
    /// </summary>
    /// <param name="blocks"></param>
    public void SetHighResChunkArray(short [,,] blocks)
    {
        if (blocks.GetLength(0) == heightMap.GetLength(0) && blocks.GetLength(2) == heightMap.GetLength(1))
        {
            highResChunkArray = blocks;
            hasNoiseChunk = true;
        }
    }

    /// <summary>
    /// Gets a reference to the highResNoiseChunk
    /// 3d short array
    /// 
    /// saves time during mesh generation.
    /// </summary>
    /// <returns>reference to the chunk array</returns>
    public short[,,] GetHighResChunkArray()
    {
        return highResChunkArray;
    }

    /// <summary>
    /// Returns true if the mapData has a noise chunk
    /// </summary>
    /// <returns>true if the noise chunk has been created</returns>
    public bool HasNoiseChunk()
    {
        return hasNoiseChunk;
    }


    /// <summary>
    /// Returns true if the mapData has ever been changed by the player (to determine what to save).
    /// </summary>
    /// <returns>true if map has been changed. </returns>
    public bool HasChanged()
    {
        return changed;
    }
    #endregion
}

