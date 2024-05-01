﻿using UnityEngine;
using System.Collections;
using System;
using System.Threading;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour {

	// Enumeration for different draw modes
	public enum DrawMode {NoiseMap, ColourMap, Mesh};
	public DrawMode drawMode;

	// Normalization mode for noise generation
	public Noise.NormalizeMode normalizeMode;

	// public const int mapChunkSize = 95;

	// Boolean to enable/disable flat shading
	public bool useFlatShading;

	// Editor preview level of detail (LOD)
	[Range(0,6)]
	public int editorPreviewLOD;

	// Scale of the noise
	public float noiseScale;

	// Parameters for noise generation
	public int octaves;
	[Range(0,1)]
	public float persistance;
	public float lacunarity;

	// Seed for random number generation
	public int seed;
	
	// Offset for noise generation
	public Vector2 offset;

	// Multiplier for mesh height
	public float meshHeightMultiplier;

	// Curve for mapping noise values to mesh heights
	public AnimationCurve meshHeightCurve;

	// Boolean to auto-update the map
	public bool autoUpdate;

	// Array of terrain types
	public TerrainType[] regions;

// Queues for storing map and mesh thread information
	Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
	Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

	// Property to get map chunk size based on flat shading
	public static int mapChunkSize {
		get {
				// Property to get map chunk size based on flat shading
			if (Instance == null) {
				Instance = FindObjectOfType<MapGenerator> ();
			}
			// Return chunk size based on flat shading
			if (Instance.useFlatShading) {
				return 95;
			}
			return 239;
		}
	
	}

	// Method to draw the map in the editor
	public void DrawMapInEditor() {
		// Generate map data
		MapData mapData = GenerateMapData (Vector2.zero);

		// Get the MapDisplay instance
		MapDisplay display = FindObjectOfType<MapDisplay> ();
		// Draw based on draw mode
		if (drawMode == DrawMode.NoiseMap) {
			display.DrawTexture (TextureGenerator.TextureFromHeightMap (mapData.heightMap));
		} else if (drawMode == DrawMode.ColourMap) {
			display.DrawTexture (TextureGenerator.TextureFromColourMap (mapData.colourMap, mapChunkSize, mapChunkSize));
		} else if (drawMode == DrawMode.Mesh) {
			display.DrawMesh (MeshGenerator.GenerateTerrainMesh (mapData.heightMap, meshHeightMultiplier, meshHeightCurve, editorPreviewLOD, useFlatShading), TextureGenerator.TextureFromColourMap (mapData.colourMap, mapChunkSize, mapChunkSize));
		}
	}

	// Method to request map data
	public void RequestMapData(Vector2 centre, Action<MapData> callback) {
		// Start a new thread for map data generation
		ThreadStart threadStart = delegate {
			MapDataThread (centre, callback);
		};

		new Thread (threadStart).Start ();
	}

	// Method for map data thread
	void MapDataThread(Vector2 centre, Action<MapData> callback) {
		// Generate map data
		MapData mapData = GenerateMapData (centre);
		// Enqueue map data thread info
		lock (mapDataThreadInfoQueue) {
			mapDataThreadInfoQueue.Enqueue (new MapThreadInfo<MapData> (callback, mapData));
		}
	}

	// Method to request mesh data
	public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callback) {
		// Start a new thread for mesh data generation
		ThreadStart threadStart = delegate {
			MeshDataThread (mapData, lod, callback);
		};

		new Thread (threadStart).Start ();
	}

    // Method for mesh data thread
	void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback) {
		// Generate mesh data
		MeshData meshData = MeshGenerator.GenerateTerrainMesh (mapData.heightMap, meshHeightMultiplier, meshHeightCurve, lod, useFlatShading);
		// Enqueue mesh data thread info
		lock (meshDataThreadInfoQueue) {
			meshDataThreadInfoQueue.Enqueue (new MapThreadInfo<MeshData> (callback, meshData));
		}
	}

	// Update method to process thread info queues
	void Update() {
		if (mapDataThreadInfoQueue.Count > 0) {
			for (int i = 0; i < mapDataThreadInfoQueue.Count; i++) {
				MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue ();
				threadInfo.callback (threadInfo.parameter);
			}
		}

		if (meshDataThreadInfoQueue.Count > 0) {
			for (int i = 0; i < meshDataThreadInfoQueue.Count; i++) {
				MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue ();
				threadInfo.callback (threadInfo.parameter);
			}
		}
	}

	// Method to generate map data
	MapData GenerateMapData(Vector2 centre) {
		// Generate noise map
		float[,] noiseMap = Noise.GenerateNoiseMap (mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, centre + offset, normalizeMode);

		// Generate color map
		Color[] colourMap = new Color[mapChunkSize * mapChunkSize];
		for (int y = 0; y < mapChunkSize; y++) {
			for (int x = 0; x < mapChunkSize; x++) {
				float currentHeight = noiseMap [x, y];
				for (int i = 0; i < regions.Length; i++) {
					if (currentHeight >= regions [i].height) {
						colourMap [y * mapChunkSize + x] = regions [i].colour;
					} else {
						break;
					}
				}
			}
		}

		// Return the generated map data
		return new MapData (noiseMap, colourMap);
	}

	// Method to ensure valid values for parameters
	void OnValidate() {
		if (lacunarity < 1) {
			lacunarity = 1;
		}
		if (octaves < 0) {
			octaves = 0;
		}
	}

	// Method to set the seed for random number generation
    public void SetSeed(string seedValue, Vector2 centre)
    {
        // if (long.TryParse(seedValue, out long parsedSeed))
        // {
        //     seed = (int)parsedSeed;
        // 	GenerateMapData();
		// 	Update();
        // }
		seed = int.Parse(seedValue);
		GenerateMapData(centre);
		Update();
    }

	public static MapGenerator Instance { get; private set; } // Singletone property instance

	// Awake method to handle object instantiation
    void Awake()
    {
		// Check if an instance already exists
        if (Instance == null)
        {
			// Check if an instance already exists
            Instance = this;
			// Don't destroy this object when loading a new scene
            DontDestroyOnLoad(gameObject); // This ensures the object isn't destroyed when loading a new scene
        }
        else
        {
			// Destroy the duplicate instance
            Destroy(gameObject);
        }
    }
	//ma_azoug@esi.dz
	
	// Structure to store thread information
	struct MapThreadInfo<T> {
		public readonly Action<T> callback;
		public readonly T parameter;

		// Constructor
		public MapThreadInfo (Action<T> callback, T parameter)
		{
			this.callback = callback;
			this.parameter = parameter;
		}
		
	}

}

// Serializable struct for defining terrain types
[System.Serializable]
public struct TerrainType {
	public string name;
	public float height;
	public Color colour;
}

// Structure to store map data
public struct MapData {
	public readonly float[,] heightMap;
	public readonly Color[] colourMap;

	// Constructor
	public MapData (float[,] heightMap, Color[] colourMap)
	{
		this.heightMap = heightMap;
		this.colourMap = colourMap;
	}
}