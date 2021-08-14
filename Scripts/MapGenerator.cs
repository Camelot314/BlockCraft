using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{

    #region Private Classes
    /// <summary>
    /// Used to determine what mode to draw in.
    /// </summary>
    public enum DrawMode { NoiseMap, Mesh, TreeMap, Nothing };
    #endregion

    #region Fields
    private static int mapVerts = 16;

    [Header("Map Generator Settings")]
    [SerializeField] private bool autoUpdate;
    [SerializeField] [Range(8, 48)] private int editorMapSize;
    [SerializeField] private DrawMode drawMode;

    [Header("Map Generation Data")]
    [SerializeField] private int seed;
    [SerializeField] [Range(0,10)] private int octaves;
    [SerializeField] private float noiseScale, lacunarity;
    [SerializeField] [Range(0, 1)] private float persistance;
    [SerializeField] private Vector2 offset;
    [SerializeField] Noise.NormalizeMode normalizeMode;

    [Header("TreeMap Generator Settings")]
    [SerializeField] private int treeSeed;
    [SerializeField] private int treeLeafSeed;
    [SerializeField] [Range(0, 48)] private int treeX;
    [SerializeField] private Vector2 treeOffset;

    [Header("Mesh Generator settings")]
    [SerializeField] [Range(0, 4)] private int editorLOD = 0;
    [SerializeField] private AnimationCurve meshHeightCurve;

    [Header("Preview Game Objects")]
    [SerializeField] private VoxelChunk testChunk;
    [SerializeField] private GameObject editorChunk, editorPlane;

    private bool inEditor = true;
    
    #endregion

    #region Unity Methods

    /// <summary>
    /// Making sure that the width and height are never less than 1. 
    /// Also making sure that lacunarity is a value that is 1 or greater and
    /// octaves is a value that is greater than 0. 
    /// </summary>
    private void OnValidate()
    {
        if (lacunarity < 1)
        {
            lacunarity = 1;
        }

        if (editorMapSize % 8 != 0)
        {
            editorMapSize -= editorMapSize % 8;
            editorMapSize = editorMapSize < 8 ? 8 : editorMapSize;
        }

        inEditor = true;
    }
    /// <summary>
    /// Generate the falloffMap when the game starts. 
    /// </summary>
    private void Awake()
    {
        inEditor = false;
        ClearEditorPrev();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Returns the size of the map
    /// </summary>
    /// <returns>int mapSize</returns>
    public static int GetMapVerts()
    {
        return mapVerts;
    }

    /// <summary>
    /// Sets the mapVerts
    /// </summary>
    /// <param name="mapSize">int greater than or equal to 0</param>
    public static void SetMapVerts(int mapSize)
    {
        if (mapSize >= 0)
        {
            mapVerts = mapSize;
        }
    }

    /// <summary>
    /// Returns a reference to the animation curve used in meshGeneration.
    /// </summary>
    /// <returns>Reference to an animation curve</returns>
    public AnimationCurve GetMeshHeightCurve()
    {
        return meshHeightCurve;
    }



    /// <summary>
    /// Draws the map in the editor. 
    /// 
    /// If the drawMode enum is noiseMap then it activates the plane and draws the noise map.
    /// If the drawMode is mesh then it activates the mesh and makes the mesh.
    /// If the drawMode is the fallOffMap then it activates the plane and draws the falloff map.
    /// </summary>
    public void DrawMapInEditor()
    {
        MapDisplay display = FindObjectOfType<MapDisplay>();
        MapData mapData = GenerateMapData(Vector2.zero, editorMapSize);

        // if the drawmode enum is noisemap then it will show the noise map. Or the color map or the mesh. 
        switch (drawMode)
        {
            case DrawMode.NoiseMap:
                ActivatePlane(true);
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
                break;
            case DrawMode.Mesh:
                ActivatePlane(false);
                testChunk.GenerateChunkInEditor(mapData, meshHeightCurve, editorLOD, editorMapSize);
                break;
            case DrawMode.TreeMap:
                ActivatePlane(true);
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.treeMap));
                break;
            case DrawMode.Nothing:
                ClearEditorPrev();
                break;
        }
    }

    /// <summary>
    /// Turns the editor plane and editor chunk inactive.
    /// This is used when the game starts or when the drawMode is set 
    /// to nothing.
    /// </summary>
    private void ClearEditorPrev()
    {
        editorPlane.SetActive(false);
        editorChunk.SetActive(false);
    }

    /// <summary>
    /// Returns true if the mapGenerator is in auto update mode. 
    /// </summary>
    /// <returns>bool true if autoUpdate. </returns>
    public bool IsAutoUpdate()
    {
        return autoUpdate;
    }
    #endregion

    #region Private Methods


    /// <summary>
    /// This method should only be used in the editor. 
    /// If active is true then it will first check if the editor plane is inactive and
    /// the editor chunk is active. If either of these are true then it will activate the plane
    /// and deactivate the chunk.
    /// 
    /// If active is set to false then it will activate the chunk and deactivate the plane.
    /// It will only change the activity if the chunk is not already active or the plane
    /// is active. 
    /// </summary>
    /// <param name="active"></param>
    private void ActivatePlane(bool active)
    {
        if (inEditor && !(active ^ !editorPlane.activeSelf) || !(active ^ editorChunk.activeSelf))
        {
            editorPlane.SetActive(active);
            editorChunk.SetActive(!active);
        }
    }



    /// <summary>
    /// Generates the information needed to make a map. 
    /// </summary>
    /// <returns>MapData Structure that holds the noiseMap and the heightMap</returns>
    public MapData GenerateMapData(Vector2 center)
    {

        float[,] noiseMap = Noise.GenerateNoiseMap(
            mapVerts, mapVerts, seed, noiseScale,
            octaves, persistance, lacunarity,
            center + offset, normalizeMode
        );

        float[,] treeMap = TreeMapGenerator.GenerateTreeMap(
            mapVerts, mapVerts, treeX, 
            treeX, (int) (center.x * center.y), treeOffset
            
        );

        return new MapData(noiseMap, treeMap, (int) (center.x * center.y));
    }

    /// <summary>
    /// Generataes the mapData in the editor. This will change the size
    /// of the map and chunk but the game chunk will always be the size of 
    /// the mapVerts field.
    /// </summary>
    /// <param name="center"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    private MapData GenerateMapData(Vector2 center, int size)
    {

        float[,] noiseMap = Noise.GenerateNoiseMap(
            size, size, seed, noiseScale,
            octaves, persistance, lacunarity,
            center + offset, normalizeMode
        );

        float[,] treeMap = TreeMapGenerator.GenerateTreeMap(
            size, size, treeX,
            treeX, treeSeed, treeOffset

        );

        return new MapData(noiseMap, treeMap, treeLeafSeed);

    }

        #endregion

    }



