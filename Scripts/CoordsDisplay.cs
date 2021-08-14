using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Class that displays the coordiantes in the top left corner.
/// </summary>
public class CoordsDisplay : MonoBehaviour
{
    #region fields
    [SerializeField] private Transform playerTransform;

    private TextMeshProUGUI textMesh;
    private float yOffset;
    #endregion

    #region Unity Methods
    /// <summary>
    /// Gets the textMesh components.
    /// </summary>
    void Start()
    {
        yOffset = MeshGenerator.GetChunkHeight() / 2;
        textMesh = GetComponentInChildren<TextMeshProUGUI>();
        textMesh.text = "x: 0 y: 0 z: 0";
    }

    /// <summary>
    /// Updates the coordinates to the player coordinates with a y offset that 
    /// is half the chunkHeight. 
    /// </summary>
    void Update()
    {
        Vector3 playerPos = playerTransform.position;
        textMesh.text = "x: " + playerPos.x.ToString("0.0") 
            + " y:" + (playerPos.y + yOffset).ToString("0.0")
            + " z:" + playerPos.z.ToString("0.0");
    }
    #endregion
}
