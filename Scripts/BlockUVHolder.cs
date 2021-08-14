using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class to keep track of the user inputted block types. 
/// This is not a static class purely because it cannot be
/// edited in the inspector if it was a static class.
/// </summary>
public class BlockUVHolder : MonoBehaviour
{
    #region Private Classes
    [System.Serializable]
    private struct BlockLevel {
        public string name;
        public float maxHeight;
    }
    #endregion

    #region Fields
    [SerializeField] [Tooltip("The first x elements must match the BlockLevel names")] 
    private BlockType[] blockTypes;

    [SerializeField] [Tooltip("Enter the name of the block and the max height generation value")]
    private BlockLevel[] terrainLevels;

    [SerializeField]
    private int dirtLayerHeight = 4;

    private float maxMapHeight = -1;
    #endregion

    #region Public Methods

    /// <summary>
    /// Given a height value and a max height this method will return 
    /// a short that corresponds to the block that should be generated at
    /// this specific terrain. 
    /// 
    /// The short is in the range 1 to terrainLevels.Length.
    /// This is because a short value of 0 indicates that the block is empty.
    /// </summary>
    /// <param name="height">height of the specified block</param>
    /// <param name="maxHeight">max chunk height as an int</param>
    /// <returns>short form 1 to terrainLevels.Length inclusive</returns>
    public short GetBlockID(float height, float maxHeight)
    {
        if (height <= 0)
        {
            return 1;
        }

        // mapping the height using max height to a value that is between 0 and the max height in the terrain levels. 
        if (maxMapHeight < 0)
        {
            maxMapHeight = terrainLevels[terrainLevels.Length - 1].maxHeight;
        }


        float blockHeightPercent = height / maxHeight;
        float mappedHeight = blockHeightPercent * maxMapHeight;

        for (int i = 0; i < terrainLevels.Length; i ++)
        {
            if (mappedHeight <= terrainLevels[i].maxHeight)
            {
                return (short)(i + 1);
            }
        }

        return 1;

    }

    /// <summary>
    /// From a surface level block such as snow and grass it will get the dirt block
    /// underneath if it is within 3 layers from the surface. Otherwise it will return
    /// the id of stone
    /// </summary>
    /// <param name="surfaceBlock">The type of surface block</param>
    /// <param name="remainingHeight">How far this block is from the surface</param>
    /// <returns></returns>
    public short GetSubSurfaceBlock(short surfaceBlock, float remainingHeight)
    {

        if (surfaceBlock != 4 && surfaceBlock != 3)
        {
            return surfaceBlock;
        }

        if (remainingHeight >= dirtLayerHeight)
        {
            return 2;                                                               // id associated with dirt
        }
        return 5;                                                                   // id for stone
    }

    /// <summary>
    /// This gets the short that corresponds to a treetrunk or leaf. 
    /// If stump is true then it returns the short that corresponds to the log
    /// otherwise it will return a short that corresponds to a leaf.
    /// </summary>
    /// <param name="stump">bool true if want log</param>
    /// <returns>short</returns>
    public short GetTree(bool stump)
    {
        if (stump)
        {
            return 6;                                                               // id associated with a stump
        }
        return 7;                                                                   // id of leaf
    }

    /// <summary>
    /// Returns true if the given block id is a block that is transparent.
    /// These are the glass and leaf blocks.
    /// </summary>
    /// <param name="blockID">short blockID</param>
    /// <returns>true if the block is transparent</returns>
    public bool IsTransparent(short blockID)
    {
        return blockID == 7 || blockID == 8;                                        // block ids of the transparent glass and leaf
    }

    /// <summary>
    /// Given a short greater than 0 it will return a blockType.
    /// The short will correspond to the index in the blockType array + 1.
    /// The offset is due to a short of 0 being used to indicate an empty block.
    /// </summary>
    /// <param name="id">short greater than 0</param>
    /// <returns>BlockType object</returns>
    public BlockType GetBlockFromID(short id)
    {
        id--;
        if (id < 0)
        {
            return blockTypes[0];
        }

        if (id >= blockTypes.Length)
        {
            return blockTypes[blockTypes.Length - 1];
        }

        return blockTypes[id];
    }

    #endregion


    /// <summary>
    /// Makes sure that there are enough block types to accomidate the terrain levels.
    /// It also makes sure that the terrain levels are all found in the same order in
    /// the block types array.
    /// </summary>
    private void OnValidate()
    {
        if (terrainLevels.Length > blockTypes.Length)
        {
            Debug.LogError("Terrain levels do not match block types array");
        }
        for (int i = 0; i < terrainLevels.Length; i ++)
        {
            if (!terrainLevels[i].name.Equals(blockTypes[i].GetName(), System.StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogError("Terrain level names do not match block type names at element " + i);
            }
        }
    }

    
}
