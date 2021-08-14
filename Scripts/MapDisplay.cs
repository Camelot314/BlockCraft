using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class that makes the texture for the black and white maps
/// </summary>
public class MapDisplay : MonoBehaviour
{
    #region Fields
    [SerializeField] private Renderer textureRenderer;
    #endregion

    #region Public Methods
    /// <summary>
    /// This just draws the noise map. the for loops is looping through the noise map
    /// and generating a color map that is between black and white and storing them in
    /// a 1 dimensional array which corresponds to the index of each pixel. The pixels
    /// are then applied and the renderer is scaled appropriately. 
    /// </summary>
    /// <param name="noiseMap"> the texture on which to draw.</param>
    public void DrawTexture(Texture2D texture)
    {

        textureRenderer.sharedMaterial.mainTexture = texture;
        textureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }
    #endregion

}
