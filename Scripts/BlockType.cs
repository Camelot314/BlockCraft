using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class to keep track of the uv information for a block.
/// </summary>
[System.Serializable]
public class BlockType
{

    #region Private Classes
    /// <summary>
    /// Class that keeps track of the info for each block face.
    /// This includes the face direction as well as the texture atlas
    /// coordiantes of the face.
    /// </summary>
    [System.Serializable]
    private struct BlockFace 
    {
        public BlockSide side;
        public Vector2Int atlasCoord;

    }

    #endregion

    #region Fields
    [SerializeField] private string name;
    [SerializeField] private bool repeat;
    [SerializeField] private BlockFace[] faces;
    #endregion

    #region Public Methods
    /// <summary>
    /// Returns the name of the block 
    /// </summary>
    /// <returns>String name. </returns>
    public string GetName()
    {
        return name;
    }

    /// <summary>
    /// Using the array of block faces it will 
    /// return a vector2Int for the coordinates on the 
    /// texture atlas for the texture of the specified blockside. 
    /// </summary>
    /// <param name="side">Enum blockside of the block (if Side = SIDE it it will grab the default side)</param>
    /// <returns>Vector2Int corresponding to the grid coordinate of the texture on the atlas </returns>
    public Vector2Int GetCoords(BlockSide side)
    {
        if (repeat)
        {
            return faces[0].atlasCoord;
        }

        if (side == BlockSide.FRONT || side == BlockSide.BACK 
            || side == BlockSide.LEFT || side == BlockSide.RIGHT)
        {
            side = BlockSide.SIDE;
        }

        foreach (BlockFace face in faces)
        {
            if (face.side == side)
            {
                return face.atlasCoord;
            }
        }

        return new Vector2Int(0, 0);

       
    }
    #endregion
}
