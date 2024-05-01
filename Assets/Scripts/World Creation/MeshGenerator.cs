using UnityEngine;
using System.Collections;

// A static class responsible for generating terrain meshes
public static class MeshGenerator {

	// Method to generate a terrain mesh based on a height map
	public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve _heightCurve, int levelOfDetail, bool useFlatShading) {
		// Create a new animation curve from the provided keys
		AnimationCurve heightCurve = new AnimationCurve (_heightCurve.keys);

		// Determine the mesh simplification increment based on the level of detail
		int meshSimplificationIncrement = (levelOfDetail == 0)?1:levelOfDetail * 2;

		// Calculate the bordered size of the height map
		int borderedSize = heightMap.GetLength (0);
		int meshSize = borderedSize - 2*meshSimplificationIncrement;
		int meshSizeUnsimplified = borderedSize - 2;

		// Calculate the coordinates of the top-left corner of the mesh
		float topLeftX = (meshSizeUnsimplified - 1) / -2f;
		float topLeftZ = (meshSizeUnsimplified - 1) / 2f;

		// Calculate the number of vertices per line in the mesh
		int verticesPerLine = (meshSize - 1) / meshSimplificationIncrement + 1;

		// Create a new MeshData object to store mesh information
		MeshData meshData = new MeshData (verticesPerLine,useFlatShading);

		// Create a mapping of vertex indices for border vertices
		int[,] vertexIndicesMap = new int[borderedSize,borderedSize];
		int meshVertexIndex = 0;
		int borderVertexIndex = -1;

		// Populate the vertex indices map
		for (int y = 0; y < borderedSize; y += meshSimplificationIncrement) {
			for (int x = 0; x < borderedSize; x += meshSimplificationIncrement) {
				bool isBorderVertex = y == 0 || y == borderedSize - 1 || x == 0 || x == borderedSize - 1;

				if (isBorderVertex) {
					vertexIndicesMap [x, y] = borderVertexIndex;
					borderVertexIndex--;
				} else {
					vertexIndicesMap [x, y] = meshVertexIndex;
					meshVertexIndex++;
				}
			}
		}

		// Generate vertices, UVs, and triangles for the mesh
		for (int y = 0; y < borderedSize; y += meshSimplificationIncrement) {
			for (int x = 0; x < borderedSize; x += meshSimplificationIncrement) {
				int vertexIndex = vertexIndicesMap [x, y];
				Vector2 percent = new Vector2 ((x-meshSimplificationIncrement) / (float)meshSize, (y-meshSimplificationIncrement) / (float)meshSize);
				float height = heightCurve.Evaluate (heightMap [x, y]) * heightMultiplier;
				Vector3 vertexPosition = new Vector3 (topLeftX + percent.x * meshSizeUnsimplified, height, topLeftZ - percent.y * meshSizeUnsimplified);

				meshData.AddVertex (vertexPosition, percent, vertexIndex);

				if (x < borderedSize - 1 && y < borderedSize - 1) {
					int a = vertexIndicesMap [x, y];
					int b = vertexIndicesMap [x + meshSimplificationIncrement, y];
					int c = vertexIndicesMap [x, y + meshSimplificationIncrement];
					int d = vertexIndicesMap [x + meshSimplificationIncrement, y + meshSimplificationIncrement];
					meshData.AddTriangle (a,d,c);
					meshData.AddTriangle (d,a,b);
				}

				vertexIndex++;
			}
		}

		// Process the mesh data (flat shading or normal baking)
		meshData.ProcessMesh ();

		// Return the generated mesh data
		return meshData;

	}
}

// A class representing mesh data (vertices, triangles, and UVs)
public class MeshData {
	Vector3[] vertices;
	int[] triangles;
	Vector2[] uvs;
	Vector3[] bakedNormals;

	Vector3[] borderVertices;
	int[] borderTriangles;

	int triangleIndex;
	int borderTriangleIndex;

	bool useFlatShading;

	// Constructor to initialize mesh data arrays
	public MeshData(int verticesPerLine, bool useFlatShading) {
		this.useFlatShading = useFlatShading;

		vertices = new Vector3[verticesPerLine * verticesPerLine];
		uvs = new Vector2[verticesPerLine * verticesPerLine];
		triangles = new int[(verticesPerLine-1)*(verticesPerLine-1)*6];

		borderVertices = new Vector3[verticesPerLine * 4 + 4];
		borderTriangles = new int[24 * verticesPerLine];
	}

	// Method to add a vertex to the mesh data
	public void AddVertex(Vector3 vertexPosition, Vector2 uv, int vertexIndex) {
		if (vertexIndex < 0) {
			borderVertices [-vertexIndex - 1] = vertexPosition;
		} else {
			vertices [vertexIndex] = vertexPosition;
			uvs [vertexIndex] = uv;
		}
	}

	// Method to add a triangle to the mesh data
	public void AddTriangle(int a, int b, int c) {
		if (a < 0 || b < 0 || c < 0) {
			borderTriangles [borderTriangleIndex] = a;
			borderTriangles [borderTriangleIndex + 1] = b;
			borderTriangles [borderTriangleIndex + 2] = c;
			borderTriangleIndex += 3;
		} else {
			triangles [triangleIndex] = a;
			triangles [triangleIndex + 1] = b;
			triangles [triangleIndex + 2] = c;
			triangleIndex += 3;
		}
	}

	// Method to calculate vertex normals
	Vector3[] CalculateNormals() {

		Vector3[] vertexNormals = new Vector3[vertices.Length];
		int triangleCount = triangles.Length / 3;
		for (int i = 0; i < triangleCount; i++) {
			int normalTriangleIndex = i * 3;
			int vertexIndexA = triangles [normalTriangleIndex];
			int vertexIndexB = triangles [normalTriangleIndex + 1];
			int vertexIndexC = triangles [normalTriangleIndex + 2];

			Vector3 triangleNormal = SurfaceNormalFromIndices (vertexIndexA, vertexIndexB, vertexIndexC);
			vertexNormals [vertexIndexA] += triangleNormal;
			vertexNormals [vertexIndexB] += triangleNormal;
			vertexNormals [vertexIndexC] += triangleNormal;
		}

		int borderTriangleCount = borderTriangles.Length / 3;
		for (int i = 0; i < borderTriangleCount; i++) {
			int normalTriangleIndex = i * 3;
			int vertexIndexA = borderTriangles [normalTriangleIndex];
			int vertexIndexB = borderTriangles [normalTriangleIndex + 1];
			int vertexIndexC = borderTriangles [normalTriangleIndex + 2];

			Vector3 triangleNormal = SurfaceNormalFromIndices (vertexIndexA, vertexIndexB, vertexIndexC);
			if (vertexIndexA >= 0) {
				vertexNormals [vertexIndexA] += triangleNormal;
			}
			if (vertexIndexB >= 0) {
				vertexNormals [vertexIndexB] += triangleNormal;
			}
			if (vertexIndexC >= 0) {
				vertexNormals [vertexIndexC] += triangleNormal;
			}
		}


		for (int i = 0; i < vertexNormals.Length; i++) {
			vertexNormals [i].Normalize ();
		}

		return vertexNormals;

	}

	Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC) {
		Vector3 pointA = (indexA < 0)?borderVertices[-indexA-1] : vertices [indexA];
		Vector3 pointB = (indexB < 0)?borderVertices[-indexB-1] : vertices [indexB];
		Vector3 pointC = (indexC < 0)?borderVertices[-indexC-1] : vertices [indexC];

		Vector3 sideAB = pointB - pointA;
		Vector3 sideAC = pointC - pointA;
		return Vector3.Cross (sideAB, sideAC).normalized;
	}

	// Method to process the mesh data (flat shading or normal baking)
	public void ProcessMesh() {
		if (useFlatShading) {
			FlatShading ();
		} else {
			BakeNormals ();
		}
	}

	// Method to bake normals
	void BakeNormals() {
		bakedNormals = CalculateNormals ();
	}

	// Method to perform flat shading
	void FlatShading() {
		Vector3[] flatShadedVertices = new Vector3[triangles.Length];
		Vector2[] flatShadedUvs = new Vector2[triangles.Length];

		for (int i = 0; i < triangles.Length; i++) {
			flatShadedVertices [i] = vertices [triangles [i]];
			flatShadedUvs [i] = uvs [triangles [i]];
			triangles [i] = i;
		}

		vertices = flatShadedVertices;
		uvs = flatShadedUvs;
	}

	// Method to create a mesh from the mesh data
	public Mesh CreateMesh() {
		Mesh mesh = new Mesh ();
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.uv = uvs;
		if (useFlatShading) {
			mesh.RecalculateNormals ();
		} else {
			mesh.normals = bakedNormals;
		}
		return mesh;
	}

}