using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;

public class ThreadHandler : MonoBehaviour
{
    #region Private Classes

    /// <summary>
    /// Class to hold information about  athread.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    private struct MapThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadInfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }
    }
    #endregion



    #region Fields

    [SerializeField] private bool threaded;
    [SerializeField] private MapGenerator mapGenerator;
    [SerializeField] private BlockUVHolder uvHolder;


    private Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    private Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();
    #endregion

    #region Public Methods
   
    /// <summary>
    /// Sets the game to be threaded
    /// </summary>
    /// <param name="threaded">true if threaded</param>
    public void SetThreaded(bool threaded)
    {
        this.threaded = threaded;
    }


    /// <summary>
    /// Sets up a thread that then generates the mapData. 
    /// </summary>
    /// <param name="callBack">lambda Expression of what to do once MapData is made. </param>
    public void RequtestMapData(Vector2 center, Action<MapData> callBack)
    {
        if (threaded)
        {
            ThreadStart threadStart = delegate
            {
                MapDataThread(center, callBack);
            };

            new Thread(threadStart).Start();
            return;
        }

        MapDataThread(center, callBack);
    }

    /// <summary>
    /// Sets up the thread that generates the mesh from the map data. The callbacck function will
    /// be executed when the mesh is created in the update method (main thread). 
    /// </summary>
    /// <param name="mapData">map data</param>
    /// <param name="callBack">callback Lambda expression that is executed when everything is done.</param>
    public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callBack)
    {
        if (threaded)
        {
            ThreadStart threadStart = delegate
            {
                MeshDataThread(mapData, lod, callBack);
            };
            new Thread(threadStart).Start();
            return;
        }

        MeshDataThread(mapData, lod, callBack);
    }
    #endregion

    #region Unity Methods

    /// <summary>
    /// Will throw an error if the game is not in threaded mode (so I don't forget to 
    /// turn on threeading).
    /// </summary>
    private void Awake()
    {
        if (!threaded)
        {
            Debug.LogWarning("not threaded");
        }

    }

    /// <summary>
    /// Every frame it will check if the threads are finished (if they are finished then
    /// there should be objects in the queue). If they are finished then it will 
    /// remove them from the queue and call the lambda expression.
    /// 
    /// The lambda expression is usually some method that can only be executed in the main
    /// unity thread (such as making a mesh object).
    /// </summary>
    void Update()
    {
        if (mapDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < mapDataThreadInfoQueue.Count; i++)
            {
                lock (mapDataThreadInfoQueue)
                {
                    MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
                    threadInfo.callback(threadInfo.parameter);
                }

            }
        }

        if (meshDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
            {
                lock (meshDataThreadInfoQueue)
                {
                    MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                    threadInfo.callback(threadInfo.parameter);
                }
            }
        }
    }

    #endregion

    #region Private Methods
    /// <summary>
    /// This is the method that is called by each of the threads
    /// that are making the MapData for the chunk. it will make the 
    /// data and then add the data as well as the callBack lambda expression to a queue.
    /// The queue is processed in the update method because some of the processing (like making
    /// meshes) are restricted to Unities main thread only. 
    /// </summary>
    /// <param name="callback">Lambda expression that is executed in the main thread</param>
    private void MapDataThread(Vector2 center, Action<MapData> callback)
    {
        MapData mapData = mapGenerator.GenerateMapData(center);
        lock (mapDataThreadInfoQueue)
        {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }

    }

    /// <summary>
    /// This is the method that is called by each of the threads
    /// that are making the MapData for the chunk. it will make the 
    /// data and then add the data as well as the callBack lambda expression to a queue.
    /// The queue is processed in the update method because some of the processing (like making
    /// meshes) are restricted to Unities main thread only. 
    /// </summary>
    /// <param name="mapData">Map data needed to make the MeshInfo</param>
    /// <param name="callBack">Lambda expression that is executed in the main thread</param>
    private void MeshDataThread(MapData mapData, int lod, Action<MeshData> callBack)
    {
        MeshData meshData;
        if (!mapData.HasNoiseChunk() || lod != 0)
        {
            meshData = MeshGenerator.GenerateTerrainMesh(mapData, mapGenerator.GetMeshHeightCurve(), lod, MapGenerator.GetMapVerts(), uvHolder);
        } else
        {
            meshData = MeshGenerator.GenerateMeshFromChunk(mapData, uvHolder);
        }
        lock (meshDataThreadInfoQueue)
        {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callBack, meshData));
        }
    }
    #endregion
}
