using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GenerateTerrain : MonoBehaviour {

	// Scale factor for positioning
	const float scale = 2.5f;

	// Thresholds for updating chunks based on viewer movement
	const float viewerMoveThresholdForChunkUpdate = 25f;
	const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

    // Thresholds for updating chunks based on viewer movement // Array of detail levels for LOD (Level of Detail)
	public LODInfo[] detailLevels;

	// Maximum view distance
	public static float maxViewDst;

	// Reference to the viewer's transform
	public Transform viewer;

	// Material to apply to the terrain
	public Material mapMaterial;

	// Current position of the viewer
	public static Vector2 viewerPosition;

	// Previous position of the viewer
	Vector2 viewerPositionOld;

	// Reference to the MapGenerator script	
	static MapGenerator mapGenerator;

	// Size of each terrain chunk
	int chunkSize;

	// Number of visible chunks in view distance
	int chunksVisibleInViewDst;

	// Dictionary to store terrain chunks
	Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
	
	// List of terrain chunks visible in the last update
	static List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

	void Start() {
		// List of terrain chunks visible in the last update
		mapGenerator = FindObjectOfType<MapGenerator> ();

		// Set the maximum view distance to the threshold of the last detail level
		maxViewDst = detailLevels [detailLevels.Length - 1].visibleDstThreshold;
		// Calculate chunk size based on map chunk size
		chunkSize = MapGenerator.mapChunkSize - 1;
		// Calculate the number of chunks visible in view distance
		chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDst / chunkSize);

		// Update visible chunks when starting
		UpdateVisibleChunks ();
	}

	void Update() {
		// Update viewer position
		viewerPosition = new Vector2 (viewer.position.x, viewer.position.z) / scale;

		// Check if viewer has moved enough to update visible chunks
		if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate) {
			viewerPositionOld = viewerPosition;
			UpdateVisibleChunks ();
		}
	}
		
	void UpdateVisibleChunks() {

		// Hide all terrain chunks from the last update
		for (int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++) {
			terrainChunksVisibleLastUpdate [i].SetVisible (false);
		}
		terrainChunksVisibleLastUpdate.Clear ();

		// Calculate current chunk coordinates based on viewer position	
		int currentChunkCoordX = Mathf.RoundToInt (viewerPosition.x / chunkSize);
		int currentChunkCoordY = Mathf.RoundToInt (viewerPosition.y / chunkSize);

		// Loop through visible chunks in view distance
		for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++) {
			for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++) {
				Vector2 viewedChunkCoord = new Vector2 (currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

				// Check if the chunk is already generated (If the chunk is in the dictionary, update it)
				if (terrainChunkDictionary.ContainsKey (viewedChunkCoord)) {
					terrainChunkDictionary [viewedChunkCoord].UpdateTerrainChunk ();
				} else {
					// Generate new terrain chunk if not found
					terrainChunkDictionary.Add (viewedChunkCoord, new TerrainChunk (viewedChunkCoord, chunkSize, detailLevels, transform, mapMaterial));
				}

			}
		}
	}

	// Class to represent a terrain chunk
	public class TerrainChunk {

		GameObject meshObject;
		Vector2 position;
		Bounds bounds;

		MeshRenderer meshRenderer;
		MeshFilter meshFilter;
		MeshCollider meshCollider;

		LODInfo[] detailLevels;
		LODMesh[] lodMeshes;

		MapData mapData;
		bool mapDataReceived;
		int previousLODIndex = -1;

		// Constructor for terrain chunk
		public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, Transform parent, Material material) {
			this.detailLevels = detailLevels;

			position = coord * size;
			bounds = new Bounds(position,Vector2.one * size);
			Vector3 positionV3 = new Vector3(position.x,0,position.y);

			// Create mesh object and components
			meshObject = new GameObject("Terrain Chunk");
			meshRenderer = meshObject.AddComponent<MeshRenderer>();
			meshFilter = meshObject.AddComponent<MeshFilter>();
			meshCollider = meshObject.AddComponent<MeshCollider>();
			meshRenderer.material = material;

			// Set mesh object's position and parent
			meshObject.transform.position = positionV3 * scale;
			meshObject.transform.parent = parent;
			meshObject.transform.localScale = Vector3.one * scale;
			SetVisible(false);

			// Initialize LOD meshes
			lodMeshes = new LODMesh[detailLevels.Length];
			for (int i = 0; i < detailLevels.Length; i++) {
				lodMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrainChunk);
			}

			// Request map data
			mapGenerator.RequestMapData(position,OnMapDataReceived);
		}

		// Callback for when map data is received
		void OnMapDataReceived(MapData mapData) {
			this.mapData = mapData;
			mapDataReceived = true;

			Texture2D texture = TextureGenerator.TextureFromColourMap (mapData.colourMap, MapGenerator.mapChunkSize, MapGenerator.mapChunkSize);
			meshRenderer.material.mainTexture = texture;

			UpdateTerrainChunk ();
		}

	
		// Method to update the terrain chunk based on LOD and visibility
		public void UpdateTerrainChunk() {
			if (mapDataReceived) {
				float viewerDstFromNearestEdge = Mathf.Sqrt (bounds.SqrDistance (viewerPosition));
				bool visible = viewerDstFromNearestEdge <= maxViewDst;

				if (visible) {
					int lodIndex = 0;

					for (int i = 0; i < detailLevels.Length - 1; i++) {
						if (viewerDstFromNearestEdge > detailLevels [i].visibleDstThreshold) {
							lodIndex = i + 1;
						} else {
							break;
						}
					}

					if (lodIndex != previousLODIndex) {
						LODMesh lodMesh = lodMeshes [lodIndex];
						if (lodMesh.hasMesh) {
							previousLODIndex = lodIndex;
							meshFilter.mesh = lodMesh.mesh;
							meshCollider.sharedMesh = lodMesh.mesh;
						} else if (!lodMesh.hasRequestedMesh) {
							lodMesh.RequestMesh (mapData);
						}
					}

					terrainChunksVisibleLastUpdate.Add (this);
				}

				SetVisible (visible);
			}
		}

		// Method to set the visibility of the terrain chunk
		public void SetVisible(bool visible) {
			meshObject.SetActive (visible);
		}

		// Method to check if the terrain chunk is visible
		public bool IsVisible() {
			return meshObject.activeSelf;
		}

	}

	// Class for LOD mesh
	class LODMesh {

		public Mesh mesh;
		public bool hasRequestedMesh;
		public bool hasMesh;
		int lod;
		System.Action updateCallback;

		// Constructor for LODMesh
		public LODMesh(int lod, System.Action updateCallback) {
			this.lod = lod;
			this.updateCallback = updateCallback;
		}

		// Callback for when mesh data is received
		void OnMeshDataReceived(MeshData meshData) {
			mesh = meshData.CreateMesh ();
			hasMesh = true;

			updateCallback ();
		}

		// Method to request mesh data
		public void RequestMesh(MapData mapData) {
			hasRequestedMesh = true;
			mapGenerator.RequestMeshData (mapData, lod, OnMeshDataReceived);
		}

	}

	// Struct for LOD information
	[System.Serializable]
	public struct LODInfo {
		public int lod;
		public float visibleDstThreshold;
	}

}

