using UnityEngine;
using System.Collections;

public class MapDisplay : MonoBehaviour {

	// Reference to the texture renderer
	public Renderer textureRender;

	// Reference to the mesh filter
	public MeshFilter meshFilter;

    // Reference to the mesh renderer	
	public MeshRenderer meshRenderer;

	// Method to draw a texture on the texture renderer
	public void DrawTexture(Texture2D texture) {
		// Assign the texture to the material of the texture renderer
		textureRender.sharedMaterial.mainTexture = texture;
		// Set the scale of the texture renderer to match the texture dimensions
		textureRender.transform.localScale = new Vector3 (texture.width, 1, texture.height);
	}

	// Method to draw a mesh using provided mesh data and texture
	public void DrawMesh(MeshData meshData, Texture2D texture) {
		// Assign the mesh generated from the mesh data to the mesh filter
		meshFilter.sharedMesh = meshData.CreateMesh ();
		// Assign the mesh generated from the mesh data to the mesh filter
		meshRenderer.sharedMaterial.mainTexture = texture;
	}

}