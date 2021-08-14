using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Class that controls the item bar and which item is highlighed. 
/// </summary>
public class ItemBar : MonoBehaviour
{

    #region Fields
    [SerializeField] private Color normalColor, highlightedColor;
    [SerializeField] private GameObject[] items;
    [SerializeField] private BlockUVHolder uvHolder;

    
    private Image[] itemHolderBacks;
    private RawImage[] blockFaces;
    private int highLightedIndex = 0;
    #endregion

    #region Public Methods
    /// <summary>
    /// Changes the highlited index
    /// </summary>
    /// <param name="blockID">blockID = index in the list + 2</param>
    public void ChangeHighlighted(int blockID)
    {
        if (highLightedIndex != blockID - 2)
        {
            HighLightMenuItem(blockID - 2, highLightedIndex);
            highLightedIndex = blockID - 2;
        }
    }
    #endregion

    #region Unity Methods
    /// <summary>
    /// Gets the image references for each of the menu items
    /// </summary>
    private void Awake()
    {
        itemHolderBacks = new Image[items.Length];
        blockFaces = new RawImage[items.Length];
        for (int i = 0; i < items.Length; i ++)
        {
            itemHolderBacks[i] = items[i].GetComponent<Image>();
            blockFaces[i] = items[i].GetComponentInChildren<RawImage>();
            blockFaces[i].uvRect = UVCalculator.GetUVs(uvHolder.GetBlockFromID((short)(i + 2)), BlockSide.FRONT);
            
        }
        HighLightMenuItem(highLightedIndex, -1);


    }


    /// <summary>
    /// Applies the highlight to the new index and removes it from the previous index.
    /// </summary>
    /// <param name="index">new index to highlight</param>
    /// <param name="prevIndex">old index to remove the highlight</param>
    private void HighLightMenuItem(int index, int prevIndex)
    {
        itemHolderBacks[index].color = highlightedColor;
        if (prevIndex > -1 )
        {
            itemHolderBacks[prevIndex].color = normalColor;
        }
       
    }
    #endregion
}
