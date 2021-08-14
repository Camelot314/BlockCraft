using System.Collections;
using UnityEngine;


/// <summary>
/// Structure that keeps track of the info 
/// related to each level of detail.
/// </summary>
[System.Serializable]
public struct LODInfo
{
    [Range(0, 4)]
    public int lod;
    public float visibleDistThreashold;

}