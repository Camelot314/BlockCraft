using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator
{

    #region Private classes

    /// <summary>
    /// This is a neighbor class that keeps track
    /// of a vector that is associated with the 6 cardinal directions of a 
    /// block neighbor as well as the direction in which the neighbor is located. 
    /// </summary>
    private class Neighbor
    {
        public Vector3Int direction;
        public BlockSide side;

        public Neighbor(Vector3Int direction, BlockSide side)
        {
            this.direction = direction;
            this.side = side;
        }

        public string toString()
        {
            return side + " " + direction;
        }
    }

    #endregion

    #region Static variables
    /// <summary>
    /// List of all the posible neighbor locations for a specific index in a 3d array.
    /// It is in the form of a Neighbor class that contains the Vector3 for the neighbor
    /// coordinate as well as a enumerator that indicated which side of the block that this
    /// neighbor is touching. 
    /// </summary>
    private static Neighbor[] neighbors =
    {
        new Neighbor(new Vector3Int(0, 1, 0), BlockSide.TOP),
        new Neighbor(new Vector3Int(-1, 0, 0), BlockSide.LEFT),
        new Neighbor(new Vector3Int(1, 0, 0), BlockSide.RIGHT),
        new Neighbor(new Vector3Int(0, 0, -1), BlockSide.BACK),
        new Neighbor(new Vector3Int(0, 0, 1), BlockSide.FRONT),
        new Neighbor(new Vector3Int(0, -1, 0), BlockSide.BOTTOM)
    };

    private static Vector3Int[] neighborCorners =
    {
        new Vector3Int(-1, -1, 0),
        new Vector3Int(-1, 1, 0),
        new Vector3Int(1, -1, 0),
        new Vector3Int(1, 1, 0),

        new Vector3Int(0, 1, 1),
        new Vector3Int(0, 1, -1),
        new Vector3Int(0, -1, 1),
        new Vector3Int(0, -1, -1),

        new Vector3Int(-1, 0, 1),
        new Vector3Int(-1, 0, -1),
        new Vector3Int(1, 0, 1),
        new Vector3Int(1, 0, -1),

        new Vector3Int(-1, -1, 1),
        new Vector3Int(-1, -1, -1),
        new Vector3Int(1, -1, 1),
        new Vector3Int(1, -1, -1),

        new Vector3Int(-1, 1, 1),
        new Vector3Int(-1, 1, -1),
        new Vector3Int(1, 1, 1),
        new Vector3Int(1, 1, -1)
    };

    private static int chunkHeight = 1, maxTreeHeight = 6;          // Max mesh height

    private static BlockUVHolder uvHolder;

    #endregion

    #region Public methods

    /// <summary>
    /// Returns the height of the chunk when it creates a mesh
    /// </summary>
    /// <returns>int</returns>
    public static int GetChunkHeight()
    {
        return chunkHeight;
    }

    /// <summary>
    /// Sets the chunk height to the one provided
    /// </summary>
    /// <param name="height">height > 0</param>
    public static void SetChunkHeight(int height)
    {
        if (height >= 0)
        {
            chunkHeight = height;
        }
    }



    /// <summary>
    /// Generates the mesh from a 2d float noiseMap and an Animation curve.
    /// 
    /// The first thing it does is create a 3d array of shorts that represent the grid
    /// in which all the blocks on the chunk exist. If the value is 1 then there is a block
    /// if it is 0 then it is empty. 
    /// 
    /// The 3d array is made by taking the x,z coordinate of a noiseMap, then evaluating
    /// its height using the height curve. This is then shifted to a number between 1 and
    /// 16 (the height of the chunk). 
    /// 
    /// The height curve is copied to make it thread safe. 
    /// 
    /// Then through a loop it will set all the array indices above that x,z point 
    /// to 1 until it reaches the proper height. 
    /// 
    /// It then loops through any adjustments dictated by the mapData adjustment dictionary.
    /// This part is used when the user places and removes blocks. 
    /// 
    /// This generates the grid of what boxes are filled.
    /// 
    /// The next loop goes through each index in the grid and
    /// creates the indices and vertices and uvs for the mesh.
    /// It will not do anything for any non-surface faces. 
    /// </summary>
    /// <param name="mapData">mapData object with height map and adjustments</param>
    /// <param name="heightCurve">reference to the height curve</param>
    /// <param name="lod">The level of detail to draw the mesh</param>
    /// <param name="squareSize">the xz size of the mesh</param>
    /// <param name="uvHolder">Object that hold information about the block types</param>
    /// <returns>Mesh data object with all the info to make a mesh</returns>
    public static MeshData GenerateTerrainMesh(MapData mapData, AnimationCurve heightCurve, int lod, int squareSize, BlockUVHolder uvHolder)
    {
        if (lod < 0 || lod > 4)
        {
            Debug.LogError("lod is not a number from 0 to 4");
        }

        if (uvHolder == null)
        {
            Debug.LogError("no uvs");
        }

        if (MeshGenerator.uvHolder == null)
        {
            MeshGenerator.uvHolder = uvHolder;
        }
        float[,] noiseMap = mapData.heightMap;
        int increment = Mathf.RoundToInt(Mathf.Pow(2, lod));
        int meshVerts = squareSize;
        short[,,] blockIds;
        System.Random leafRand = new System.Random(mapData.treeLeafSeed);

        AnimationCurve threadSafeCurve = new AnimationCurve(heightCurve.keys);

        if (increment == 1)
        {
            blockIds = new short[meshVerts, chunkHeight, meshVerts];
            // first loop making all the larger pixel corners and edges
            for (int row = 0; row < meshVerts; row++)
            {

                
                for (int col = 0; col < meshVerts; col++)
                {
                    int height = CalcBlockHeight(noiseMap, threadSafeCurve, row, col);

                    if (height > meshVerts || height < 0)
                    {
                        height = Mathf.RoundToInt(Mathf.Clamp(height, 0, chunkHeight - 1));
                    }
                    // flipping cols so that the offset math in endless terrain works
                    FillBlockIds(blockIds, row, meshVerts - 1 - col, height, 1, mapData, leafRand);
                }
            }

            AddAdjustments(mapData, blockIds, increment);

            return GenerateMeshFromID(blockIds, increment);
        }

        int avgHeight;
        blockIds = new short[meshVerts / increment, chunkHeight / increment, meshVerts / increment];

        // looping through every xth pixel in the noise map
        for (int row = 0; row < meshVerts - 1; row += increment)
        {
            for (int col = 0; col < meshVerts - 1; col += increment)
            {
                avgHeight = CalcBlockHeight(noiseMap, threadSafeCurve, col, row);

                // setting the appropriate height
                int yLocation = meshVerts / increment - row / increment - 1;
                FillBlockIds(blockIds, col / increment, yLocation, avgHeight / increment, increment, mapData, leafRand);
            }
        }

        return GenerateMeshFromID(blockIds, increment);
    }

    /// <summary>
    /// This method is called when there has already been a highRes blockID array that has been
    /// created and the level of detail call is 0. This is to prevent any unnecessary executions
    /// such as regenerating a block id list.
    /// </summary>
    /// <param name="mapData">MapData object with the information</param>
    /// <returns>MeshData object that has all info to make a mesh</returns>
    public static MeshData GenerateMeshFromChunk(MapData mapData)
    {
        short[,,] blockIds = mapData.GetHighResChunkArray();

        AddAdjustments(mapData, blockIds, 1);

        return GenerateMeshFromID(blockIds, 1);



    }

    /// <summary>
    /// Adds the adjustments to the blockIDs array based on the adjustments
    /// in the mapData class. 
    /// 
    /// This should only be called when lod is 0 as it sets the chunk array
    /// of the mapData class to the ajdusted blockID array and clears the adjustments;
    /// </summary>
    /// <param name="mapData">object containing the adjustment dictionary</param>
    /// <param name="blockIds">array to adjust</param>
    /// <param name="increment">increment of the mesh</param>
    private static void AddAdjustments(MapData mapData, short[,,] blockIds, int increment)
    {
        if (increment != 1)
        {
            Debug.LogError("should not be adjusting when increment is not 1");
        }
        foreach (KeyValuePair<Vector3Int, short> entry in mapData.GetAdjustments())
        {
            Vector3Int local = entry.Key;
            blockIds[local.x, local.y, local.z] = entry.Value;
        }

        mapData.SetHighResChunkArray(blockIds);
        mapData.GetAdjustments().Clear();
    }
    #endregion


    #region Private methods
    /// <summary>
    /// Generates the mesh including vertices, uvs, and normals
    /// given a 3d array of blocks to fill. The multiplier is used
    /// when the level of detail is greaeter than 0.
    /// </summary>
    /// <param name="blockIds">the array of shorts with block ids</param>
    /// <param name="multiplier">how much to skip indices by</param>
    /// <returns>Mesh Data object with all the info</returns>
    private static MeshData GenerateMeshFromID(short[,,] blockIds, int multiplier)
    {
        MeshData meshData = new MeshData();


        int currentIndex = 0, xSize, ySize, zSize;

        xSize = blockIds.GetLength(0);
        ySize = blockIds.GetLength(1);
        zSize = blockIds.GetLength(2);

        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                for (int z = 0; z < zSize; z++)
                {

                    if (blockIds[x, y, z] == 0 && y != 0)
                    {
                        continue;
                    }
                    Vector3 offset = (new Vector3(x, y, z) - new Vector3(xSize / 2f, ySize / 2f, zSize / 2f) + new Vector3(0.5f, 0.5f, 0.5f)) * multiplier;

                    if (blockIds[x,y,z] == 0)
                    {
                        GenMeshFloor(ref currentIndex, offset, meshData, multiplier);
                    } else
                    {
                        GenerateVisBockFace(new Vector3Int(x, y, z), blockIds, ref currentIndex, offset, meshData, multiplier);
                    }

                    

                }
            }
        }
        return meshData;
    }



    /// <summary>
    /// Given an animation curve as well as a row and col coordinate and a noise map it will
    /// calculate the height of the mesh.
    /// </summary>
    /// <param name="noiseMap">2d float array for noise map</param>
    /// <param name="heightCurve">animation curve</param>
    /// <param name="row">current row</param>
    /// <param name="col">current col </param>
    /// <returns>the height of the blocks in the location.</returns>
    private static int CalcBlockHeight(float[,] noiseMap, AnimationCurve heightCurve, int row, int col)
    {
        float noiseHeightEval = heightCurve.Evaluate(noiseMap[row, col]);
        float heightUnrounded = noiseHeightEval * (chunkHeight - 1);

        int height = Mathf.RoundToInt(heightUnrounded) + 1;
        return height;
    }

    /// <summary>
    ///     /// <summary>
    /// This method will fill in the spaces in the 3d array given a row and col coordinate as well as a 
    /// height value.
    /// 
    /// It will set the indices to shorts the represent the type of block that will be filled at each location.
    /// The surface block id is to determine if there is grass or snow on top becuase those only appear at the 
    /// top most layer.
    /// </summary>
    /// <param name="blockIds">3d array that is filled</param>
    /// <param name="row">row in the noise data (y coordiante)</param>
    /// <param name="col">col in the noise data (x coordinate)</param>
    /// <param name="height">height at which to fill the blocks to</param>
    /// <param name="multiplier">The amount by which to skip by (if lod is greater than 0)</param>
    /// <param name="mapData">mapData object that houses the tree map, and the adjustments. </param>
    /// <param name="rand">Random number generator made with the mapData seed for generating the tree leaves.</param>
    private static void FillBlockIds(short[,,] blockIds, int row, int col, int height, int multiplier, MapData mapData, System.Random rand)
    {

        float chunkMax = (blockIds.GetLength(1) - 1 - (blockIds.GetLength(1) / 2f) + 0.5f) * multiplier + (chunkHeight / 2f);

        float noiseHeightOffset = (height - 1 - (blockIds.GetLength(1) / 2f) + 0.5f) * multiplier + (chunkHeight / 2f);

        short surfaceBlockID = uvHolder.GetBlockID(noiseHeightOffset, chunkMax);

        for (int i = 0; i < height && i < blockIds.GetLength(1); i++)
        {
            if (i < height - 1)
            {
                blockIds[row, i, col] = uvHolder.GetSubSurfaceBlock(surfaceBlockID, height - i);
            }
            else
            {
                blockIds[row, i, col] = surfaceBlockID;
            }


        }

        GenerateTree(blockIds, row, col, height, multiplier, mapData, surfaceBlockID, rand);
    }

    /// <summary>
    /// Generates a tree at the specified location if the tree exists in the tree map and the location is below 
    /// the max tree growth height as well as greater than the min tree growth height. 
    /// </summary>
    /// <param name="blockIds">array to fill</param>
    /// <param name="row">y coordinate for the row</param>
    /// <param name="col">x coordinate for the column </param>
    /// <param name="height">height that the surface block is at</param>
    /// <param name="multiplier">multiplier for lod</param>
    /// <param name="mapData">mapData object with the treemap</param>
    /// <param name="surfaceBlockID">The surface block type so it doesn't grow on stone. </param>
    /// <param name="rand">random number generator to make the leaves. </param>
    private static void GenerateTree(short[,,] blockIds, int row, int col, int height, int multiplier, MapData mapData, short surfaceBlockID, System.Random rand)
    {
        int max = Mathf.RoundToInt(height + (mapData.treeMap[row, col] * maxTreeHeight));
        bool makeTree =
            multiplier < 3 && mapData.treeMap[row, col] > 0
            && chunkHeight / multiplier > max
            && surfaceBlockID > 2;

        if (makeTree)
        {
            
            for (int i = height; i < max; i++)
            {
                blockIds[row, i, col] = uvHolder.GetTree(true);
                if (i == max - 1) {
                    GenerateLeaves(blockIds, new Vector3Int(row, i, col), rand);
                }

            }
        }
    }

    /// <summary>
    /// Generates the leaves on the tree. 
    /// It does this by first making a 25 block cube (all blocks except center and one below)
    /// and randomly deletes 20 percent of the blocks
    /// </summary>
    /// <param name="blockIds">array to fill</param>
    /// <param name="startCoord">Vector3 for the top of the tree trunk</param>
    /// <param name="rand">random number generator</param>
    private static void GenerateLeaves(short[,,] blockIds, Vector3Int startCoord, System.Random rand)
    {
        foreach(Neighbor neighbor in neighbors)
        {
            bool validCoord = ValidLocal(startCoord + neighbor.direction, blockIds);
            if (neighbor.side != BlockSide.BOTTOM && validCoord && rand.NextDouble() > 0.2)
            {
                int x = startCoord.x + neighbor.direction.x;
                int y = startCoord.y + neighbor.direction.y;
                int z = startCoord.z + neighbor.direction.z;

                blockIds[x, y, z] = uvHolder.GetTree(false);
            }
        }

        foreach (Vector3Int direction in neighborCorners)
        {
            bool validCoord = ValidLocal(startCoord + direction, blockIds);
            if (validCoord && rand.NextDouble() > 0.2)
            {
                int x = startCoord.x + direction.x;
                int y = startCoord.y + direction.y;
                int z = startCoord.z + direction.z;

                blockIds[x, y, z] = uvHolder.GetTree(false);
            }
        }
    }

    /// <summary>
    /// This method will takes a block position in the chunk.
    /// It will then determine which sides of this block are visible by checking
    /// to see if the block itself is a border block or if it has a neighbor.
    /// 
    /// For any sides that are not neighboring other blocks or any sides that are
    /// borders, it will add the vertices, uvs, and normals to the meshData class.
    /// This way it will only add normals of visible points.
    /// </summary>
    /// <param name="blockPos">Vector3Int corresponding to the block index</param>
    /// <param name="blockIds">3d Flaot array corresponding to the blocks to fill</param>
    /// <param name="currentIndex">reference to the current index for verts</param>
    /// <param name="offset">Vector3 offset which is the center of the cube to fill</param>
    /// <param name="meshData">mesh data object</param>
    /// <param name="multiplier">the multiple of which to skip by</param>
    private static void GenerateVisBockFace (
        Vector3Int blockPos, short[,,] blockIds,
        ref int currentIndex, Vector3 offset, 
        MeshData meshData, int multiplier
        )
    {

        short blockID = blockIds[blockPos.x, blockPos.y, blockPos.z];

        foreach (Neighbor neighbor in neighbors)
        {
            Vector3Int neighborPos = blockPos + neighbor.direction;

            if (ValidLocal(neighborPos, blockIds)) {
                short neighborVal = blockIds[neighborPos.x, neighborPos.y, neighborPos.z];
                if (neighborVal != 0 && !uvHolder.IsTransparent(neighborVal))
                {
                    // skipping mesh generation on blocks where there are neighbors
                    // but not skipping if the block is transparent (this is if the value is 7 or 8
                    // as those are the semitransparent glass and leaf blocks.
                    continue;
                }
            }

            // at this point block position is either a border block or has an empty neighbor
            // this means that the face should be made. 
            

            // This if statement is to prevent transparent blocks at y level 0 from seeing past bedrock.
            if (blockPos.y == 0 && uvHolder.IsTransparent(blockID))
            {
                GenMeshFloor(ref currentIndex, offset, meshData, multiplier);
            }
           

            switch(neighbor.side)
            {
                case BlockSide.TOP:
                    GenerateBlockTop(ref currentIndex, offset, meshData, multiplier, blockID);
                    break;
                case BlockSide.BOTTOM:
                    GenerateBlockBottom(ref currentIndex, offset, meshData, multiplier, blockID);
                    break;
                case BlockSide.FRONT:
                    GenerateBlockFront(ref currentIndex, offset, meshData, multiplier, blockID);
                    break;
                case BlockSide.BACK:
                    GenerateBlockBack(ref currentIndex, offset, meshData, multiplier, blockID);
                    break;
                case BlockSide.LEFT:
                    GenerateBlockLeft(ref currentIndex, offset, meshData, multiplier, blockID);
                    break;
                case BlockSide.RIGHT:
                    GenerateBlockRight(ref currentIndex, offset, meshData, multiplier, blockID);
                    break;
            }


        }
    }

    /// <summary>
    /// Checks to see if the array indices are valid.
    /// </summary>
    /// <param name="location">Vector3Int index location</param>
    /// <param name="array">array to which to check the length against</param>
    /// <returns>true if the index is within the size parameter</returns>
    private static bool ValidLocal (Vector3Int location, short[,,] array)
    {
        if (location.x < 0 || location.x >= array.GetLength(0))
        {
            return false;
        }

        if (location.y < 0 || location.y >= array.GetLength(1))
        {
            return false;
        }
        
        if (location.z < 0 || location.z >= array.GetLength(2))
        {
            return false;
        }

        return true;
    }

    #region Mesh Faces

    /// <summary>
    /// This method is called if there is a value of zero in the mesh due to simplification. This
    /// makes a floor mesh layer that has a normal facing up and down but is a the floor depth (bedrock).
    /// </summary>
    /// <param name="currentIndex">reference to current vertex index</param>
    /// <param name="offset">Vector3 offset which is the center of the block to fill</param>
    /// <param name="meshData">MeshData object</param>
    /// <param name="multiplier">multiplier which is size of the block due to simplification</param>
    private static void GenMeshFloor(ref int currentIndex, Vector3 offset, MeshData meshData, int multiplier)
    {

        Rect blockUVs = UVCalculator.GetUVs(uvHolder.GetBlockFromID(0),BlockSide.TOP);

        meshData.vertices.Add(multiplier * new Vector3(-0.5f, -0.5f, 0.5f) + offset);
        meshData.vertices.Add(multiplier * new Vector3(0.5f, -0.5f, 0.5f) + offset);
        meshData.vertices.Add(multiplier * new Vector3(0.5f, -0.5f, -0.5f) + offset);
        meshData.vertices.Add(multiplier * new Vector3(-0.5f, -0.5f, -0.5f) + offset);

        AddUVsAndIndex(currentIndex, meshData, blockUVs, BlockSide.TOP);

        currentIndex += 4;

        // NOTE: the two chunks of code (above and below) are not identical code the vectors are different.

        meshData.vertices.Add(multiplier * new Vector3(0.5f, -0.5f, 0.5f) + offset);
        meshData.vertices.Add(multiplier * new Vector3(-0.5f, -0.5f, 0.5f) + offset);
        meshData.vertices.Add(multiplier * new Vector3(-0.5f, -0.5f, -0.5f) + offset);
        meshData.vertices.Add(multiplier * new Vector3(0.5f, -0.5f, -0.5f) + offset);
        
        AddUVsAndIndex(currentIndex, meshData, blockUVs, BlockSide.BOTTOM);

        currentIndex += 4;
    }

    /// <summary>
    /// Adding the mesh vertex data for the top face of the block. 
    /// Every vertext, normal and uv for the 4 verticies is added from the top left
    /// going clockwise. Order is top left is (xMin, yMax, zMax) then topRight (xMax, yMax, zMax)
    /// with the y values staying constant because y is vertical axis. 
    /// 
    /// The indicies corresspond to the triangles that are made by the mesh. There are 2 triangles
    /// for each square and the order of the verticies are clockwise always starting from the top left. 
    /// </summary>
    /// <param name="currentIndex">reference to current vertex index</param>
    /// <param name="offset">Vector3 offset which is the center of the block to fill</param>
    /// <param name="meshData">MeshData object</param>
    /// <param name="multiplier">multiplier which is size of the block due to simplification</param>
    /// <param name="blockID">the short to use to get the uvs</param>
    private static void GenerateBlockTop(ref int currentIndex, Vector3 offset, MeshData meshData, int multiplier, short blockID)
    {

        
        meshData.vertices.Add(multiplier * new Vector3(-0.5f, 0.5f, 0.5f) + offset);
        meshData.vertices.Add(multiplier * new Vector3(0.5f, 0.5f, 0.5f) + offset);
        meshData.vertices.Add(multiplier * new Vector3(0.5f, 0.5f, -0.5f) + offset);
        meshData.vertices.Add(multiplier * new Vector3(-0.5f, 0.5f, -0.5f) + offset);

        Rect blockUVs = UVCalculator.GetUVs(uvHolder.GetBlockFromID(blockID), BlockSide.TOP);
        

        AddUVsAndIndex(currentIndex, meshData, blockUVs, BlockSide.TOP);

        currentIndex += 4;
    }


    /// <summary>
    /// Generates the bottom vertices these are done with the negative y corrdinates.
    /// </summary>
    /// <param name="currentIndex">reference to current vertex index</param>
    /// <param name="offset">Vector3 offset which is the center of the block to fill</param>
    /// <param name="meshData">MeshData object</param>
    /// <param name="multiplier">multiplier which is size of the block due to simplification</param>
    /// <param name="blockID">the short to use to get the uvs</param>
    private static void GenerateBlockBottom(ref int currentIndex, Vector3 offset, MeshData meshData, int multiplier, short blockID)
    {
        meshData.vertices.Add(multiplier * new Vector3(0.5f, -0.5f, 0.5f) + offset);
        meshData.vertices.Add(multiplier * new Vector3(-0.5f, -0.5f, 0.5f) + offset);
        meshData.vertices.Add(multiplier * new Vector3(-0.5f, -0.5f, -0.5f) + offset);
        meshData.vertices.Add(multiplier * new Vector3(0.5f, -0.5f, -0.5f) + offset);

        Rect blockUVs = UVCalculator.GetUVs(uvHolder.GetBlockFromID(blockID), BlockSide.BOTTOM);

        AddUVsAndIndex(currentIndex, meshData, blockUVs, BlockSide.BOTTOM);

        currentIndex += 4;
    }

    /// <summary>
    /// Generates the front vertices this is done with the positive z coordinates. 
    /// </summary>
    /// <param name="currentIndex">reference to current vertex index</param>
    /// <param name="offset">Vector3 offset which is the center of the block to fill</param>
    /// <param name="meshData">MeshData object</param>
    /// <param name="multiplier">multiplier which is size of the block due to simplification</param>
    /// <param name="blockID">the short to use to get the uvs</param>
    private static void GenerateBlockFront(ref int currentIndex, Vector3 offset, MeshData meshData, int multiplier, short blockID)
    {
        meshData.vertices.Add(multiplier * new Vector3(0.5f, 0.5f, 0.5f) + offset);
        meshData.vertices.Add(multiplier * new Vector3(-0.5f, 0.5f, 0.5f) + offset);
        meshData.vertices.Add(multiplier * new Vector3(-0.5f, -0.5f, 0.5f) + offset);
        meshData.vertices.Add(multiplier * new Vector3(0.5f, -0.5f, 0.5f) + offset);

        Rect blockUVs= UVCalculator.GetUVs(uvHolder.GetBlockFromID(blockID), BlockSide.FRONT);
        

        AddUVsAndIndex(currentIndex, meshData, blockUVs, BlockSide.FRONT);

        currentIndex += 4;
    }

    /// <summary>
    /// Generates the back vertices, these are with the negative z corrdiantes. 
    /// </summary>
    /// <param name="currentIndex">reference to current vertex index</param>
    /// <param name="offset">Vector3 offset which is the center of the block to fill</param>
    /// <param name="meshData">MeshData object</param>
    /// <param name="multiplier">multiplier which is size of the block due to simplification</param>
    /// <param name="blockID">the short to use to get the uvs</param>
    private static void GenerateBlockBack(ref int currentIndex, Vector3 offset, MeshData meshData, int multiplier, short blockID)
    {
        meshData.vertices.Add(multiplier * new Vector3(-0.5f, 0.5f, -0.5f) + offset);
        meshData.vertices.Add(multiplier * new Vector3(0.5f, 0.5f, -0.5f) + offset);
        meshData.vertices.Add(multiplier * new Vector3(0.5f, -0.5f, -0.5f) + offset);
        meshData.vertices.Add(multiplier * new Vector3(-0.5f, -0.5f, -0.5f) + offset);

        Rect blockUVs = UVCalculator.GetUVs(uvHolder.GetBlockFromID(blockID), BlockSide.BACK);
        


        AddUVsAndIndex(currentIndex, meshData, blockUVs, BlockSide.BACK);

        currentIndex += 4;
    }

    /// <summary>
    /// Generates the left vertices, this is done with the negative x corrdinates. 
    /// </summary>
    /// <param name="currentIndex">reference to current vertex index</param>
    /// <param name="offset">Vector3 offset which is the center of the block to fill</param>
    /// <param name="meshData">MeshData object</param>
    /// <param name="multiplier">multiplier which is size of the block due to simplification</param>
    /// <param name="blockID">the short to use to get the uvs</param>
    private static void GenerateBlockLeft(ref int currentIndex, Vector3 offset, MeshData meshData, int multiplier, short blockID)
    {
        meshData.vertices.Add(multiplier * new Vector3(-0.5f, 0.5f, 0.5f) + offset);
        meshData.vertices.Add(multiplier * new Vector3(-0.5f, 0.5f, -0.5f) + offset);
        meshData.vertices.Add(multiplier * new Vector3(-0.5f, -0.5f, -0.5f) + offset);
        meshData.vertices.Add(multiplier * new Vector3(-0.5f, -0.5f, 0.5f) + offset);


        Rect blockUVs = UVCalculator.GetUVs(uvHolder.GetBlockFromID(blockID), BlockSide.LEFT);
        

        AddUVsAndIndex(currentIndex, meshData, blockUVs, BlockSide.LEFT);

        currentIndex += 4;
    }

    /// <summary>
    /// Generates the right vertices, this is done with the positive x corrdinates. 
    /// </summary>
    /// <param name="currentIndex">reference to current vertex index</param>
    /// <param name="offset">Vector3 offset which is the center of the block to fill</param>
    /// <param name="meshData">MeshData object</param>
    /// <param name="multiplier">multiplier which is size of the block due to simplification</param>
    /// <param name="blockID">the short to use to get the uvs</param>
    private static void GenerateBlockRight(ref int currentIndex, Vector3 offset, MeshData meshData, int multiplier, short blockID)
    {
        meshData.vertices.Add(multiplier * new Vector3(0.5f, 0.5f, -0.5f) + offset);
        meshData.vertices.Add(multiplier * new Vector3(0.5f, 0.5f, 0.5f) + offset);
        meshData.vertices.Add(multiplier * new Vector3(0.5f, -0.5f, 0.5f) + offset);
        meshData.vertices.Add(multiplier * new Vector3(0.5f, -0.5f, -0.5f) + offset);

        Rect blockUVs = UVCalculator.GetUVs(uvHolder.GetBlockFromID(blockID), BlockSide.RIGHT);
        

        AddUVsAndIndex(currentIndex, meshData, blockUVs, BlockSide.RIGHT);

        currentIndex += 4;
    }



    #endregion

    /// <summary>
    /// Adds the UVs and the indexing for all the coordinates so the mesh has the necessary data. 
    /// </summary>
    /// <param name="currentIndex">current vertex index</param>
    /// <param name="meshData">Mesh data object</param>
    /// <param name="blockUVs">block uvs rect to use for uvs</param>
    /// <param name="side">enumerator to indicate which side of the block this is</param>
    private static void AddUVsAndIndex(int currentIndex, MeshData meshData, Rect blockUVs, BlockSide side)
    {
        var direction = side switch
        {
            BlockSide.TOP => Vector3.up,
            BlockSide.BOTTOM => Vector3.down,
            BlockSide.LEFT => Vector3.left,
            BlockSide.RIGHT => Vector3.right,
            BlockSide.FRONT => Vector3.forward,
            BlockSide.BACK => Vector3.back,
            _ => Vector3.up,
        };

        for (int i = 0; i < 4; i ++)
        {
            meshData.normals.Add(direction);
        }

        meshData.uvs.Add(new Vector2(blockUVs.xMin, blockUVs.yMax));
        meshData.uvs.Add(new Vector2(blockUVs.xMax, blockUVs.yMax));
        meshData.uvs.Add(new Vector2(blockUVs.xMax, blockUVs.yMin));
        meshData.uvs.Add(new Vector2(blockUVs.xMin, blockUVs.yMin));

        meshData.indices.Add(currentIndex + 0);
        meshData.indices.Add(currentIndex + 1);
        meshData.indices.Add(currentIndex + 2);
        meshData.indices.Add(currentIndex + 0);
        meshData.indices.Add(currentIndex + 2);
        meshData.indices.Add(currentIndex + 3);
    }

    #endregion
}


