using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{

    #region Fields


    private const float viewerMoveThreshHoldForUpdate = 16f;                            // distance that a viewer needs to move to update the chunk visibility
    private const float sqrViewerMoveThreshHoldForUpdate 
        = viewerMoveThreshHoldForUpdate * viewerMoveThreshHoldForUpdate;                // sqr distance of above

    public static float maxViewDist;                                                    // max distance viewer can see
    public static Vector2 playerPos;                                                    // vector2 which is the x and z coord of the player. This is a static
                                                                                        //      reference to make it easier to access
    public static ThreadHandler threadHandler;                                          
    public static List<TerrainChunk> visibileLastUpdate = new List<TerrainChunk>();
    public static bool stopStart = false;

    [SerializeField] private bool debug = false;
    [SerializeField] [Tooltip("X is the xz size of the chunk, Y is the chunk height")]
    private Vector2Int debugChunkSize;                                                  // Vector2 that determines the size of the chunks when debugging.
    [SerializeField] private SavingHandler saver;
    [SerializeField] private GameObject player;
    private Transform playerTransform;                                                  // Transform of the player
    private Vector2 playerPosOld;                                                       // The old player position used to determine if the move threashold
                                                                                        //      is reached.

    [SerializeField] private Material mapMaterial;
    [SerializeField] private LODInfo[] detailLevels;                                    // List of possible detail levels to give each of the meshes.

    private int chunkSize, chunksVisible;
    private Dictionary<Vector2, TerrainChunk> allChunks = new Dictionary<Vector2, TerrainChunk>();

    #endregion

    #region Public methods

    /// <summary>
    /// Given a point that corresponds to the center of a chunk, this method will return a reference
    /// to the terrainChunk script associated with that chunk.
    /// </summary>
    /// <param name="chunkPos"></param>
    /// <returns></returns>
    public TerrainChunk GetTerrainChunk(Vector3 chunkPos)
    {
        Vector2Int chunkCoord = new Vector2Int((int) chunkPos.x / chunkSize, (int) chunkPos.z / chunkSize);

        return allChunks[chunkCoord];
    }
    
    /// <summary>
    /// Method that is called when saving the game to a file. The method
    /// returns an array of SavedTerrainChunk objects representing all the 
    /// chunks that have been edited.
    /// </summary>
    /// <returns>Array of SavedTerrainChunk objects</returns>
    public SavedTerrainChunk[] GetChangedChunks()
    {
        List<SavedTerrainChunk> tempList = new List<SavedTerrainChunk>();
        foreach (KeyValuePair<Vector2, TerrainChunk> entry in allChunks)
        {
            if (entry.Value.HasChanged())
            {
                tempList.Add(new SavedTerrainChunk(entry.Key, entry.Value));
            }
        }
        return tempList.ToArray();
    }
    #endregion

    #region Unity Methods

    /// <summary>
    /// Finds the script for the map generator. It also sets up the max
    /// view distance by looking at the detail levels array and taking the 
    /// visible threashold of the last one. 
    /// 
    /// It gets the chunkSize by taking the size of the noiseMap in the 
    /// MapGenerator class and sets the chunks visible to the maximum
    /// number of chunks that can fit in the max view distance. 
    /// 
    /// The last thing it does is update the visible chunks.
    /// </summary>
    private void Start()
    {



        int mapSize = MapGenerator.GetMapVerts();



        threadHandler = FindObjectOfType<ThreadHandler>();
        playerTransform = player.GetComponent<Transform>();

        maxViewDist = detailLevels[detailLevels.Length - 1].visibleDistThreashold;
        chunkSize = mapSize;

        mapSize = SetDebuggingSettings(mapSize);

        if ((maxViewDist * mapSize) / (mapSize * mapSize) > 500)
        {
            Debug.LogError("Too many chunks");
            stopStart = true;
            return;
        }

        chunksVisible = Mathf.RoundToInt(maxViewDist / chunkSize);

        LoadSavedData();

        UpdateChunksVisible();
    }




    /// <summary>
    /// Every time update is called it generates a new Vector2 for the viewer position. 
    /// It then chekcs to see if the current viewer position is far enough away from the old 
    /// viewer position (by using teh sqr movement threashold). If the distance is far enough
    /// it will update the viewerOld position to the current position and it will update all
    /// the visible chunks. 
    /// </summary>
    private void Update()
    {
        if (stopStart)
        {
            return;
        }
        playerPos = new Vector2(playerTransform.position.x, playerTransform.position.z);
        if ((playerPosOld - playerPos).sqrMagnitude > sqrViewerMoveThreshHoldForUpdate)
        {
            UpdateChunksVisible();
            playerPosOld = playerPos;
        }

    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Sets up the variables both in the case of debugging and not.
    /// </summary>
    /// <param name="mapSize">mapSize</param>
    /// <returns></returns>
    private int SetDebuggingSettings(int mapSize)
    {
        if (debug)
        {
            MapGenerator.SetMapVerts(debugChunkSize.x);
            MeshGenerator.SetChunkHeight(debugChunkSize.y);

            mapSize = MapGenerator.GetMapVerts();
            chunkSize = mapSize;
            threadHandler.SetThreaded(false);
            maxViewDist = debugChunkSize.x / 2f;
            Debug.LogWarning("Debugging Mode");
        }
        else
        {
            MeshGenerator.SetChunkHeight(32);
        }

        return mapSize;
    }

    /// <summary>
    /// Loads in all the savedData from a file.
    /// </summary>
    private void LoadSavedData()
    {
        SavedEndlessTerrain savedData = saver.LoadTerrain();
        foreach (SavedTerrainChunk savedChunk in savedData.GetSavedChunks())
        {
            Vector2 chunkCoord = savedChunk.GetChunkCoord();
            TerrainChunk chunk = new TerrainChunk(chunkCoord, chunkSize, detailLevels, transform, mapMaterial, UpdateChunksVisible, savedChunk.GetMapData());
            allChunks.Add(chunkCoord, chunk);
        }
    }

    /// <summary>
    /// Keeps track of the current player position in the meshSpace (Where one unit is the size 
    /// of the mesh). It then loops through all the visible chunks in the space (from negative to positive).
    /// It will then check if that particular coordinate in chunk space is already assigned a terrain. 
    /// 
    /// If it isn't then it will create a new terrain object which includes the mesh. If it is then it will update.
    /// It also keeps track of the chunks that were visible last update and sets them all to invisible. It does
    /// not destory chunks.
    /// the chunk;
    /// </summary>
    private void UpdateChunksVisible()
    {
        int currentX = Mathf.RoundToInt(playerPos.x / chunkSize);
        int currentY = Mathf.RoundToInt(playerPos.y / chunkSize);

        foreach (TerrainChunk chunk in visibileLastUpdate)
        {
            chunk.SetVisible(false);
        }
        visibileLastUpdate.Clear();

        for (int yOffset = -chunksVisible; yOffset <= chunksVisible; yOffset++)
        {
            for (int xOffset = -chunksVisible; xOffset <= chunksVisible; xOffset++)
            {
                Vector2 chunkCoord = new Vector2(currentX + xOffset, currentY + yOffset);
                TerrainChunk currentChunk;

                if (allChunks.ContainsKey(chunkCoord))
                {
                    currentChunk = allChunks[chunkCoord];
                    currentChunk.UpdateChunk();
                } else
                {
                    currentChunk = new TerrainChunk(chunkCoord, chunkSize, detailLevels, transform, mapMaterial, UpdateChunksVisible);
                    allChunks.Add(chunkCoord, currentChunk);
                }


            }
        }

        
    }
    #endregion


        
}
