using UnityEngine;
using System.Collections;

public static class TextureGenerator {

	// Method to generate a texture from a given color map, width, and height
	public static Texture2D TextureFromColourMap(Color[] colourMap, int width, int height) {
		// Create a new Texture2D object with the specified width and height
		Texture2D texture = new Texture2D (width, height);
		// Set the filter mode to point to maintain pixel clarity
		texture.filterMode = FilterMode.Point;
		// Set the wrap mode to clamp to prevent texture repeating
		texture.wrapMode = TextureWrapMode.Clamp;
		// Set the pixels of the texture using the provided color map
		texture.SetPixels (colourMap);
		// Apply the changes made to the texture
		texture.Apply ();
		// Return the generated texture
		return texture;
	}


	// Method to generate a texture from a given height map
	public static Texture2D TextureFromHeightMap(float[,] heightMap) {
		// Get the width and height of the height map
		int width = heightMap.GetLength (0);
		int height = heightMap.GetLength (1);

		// Create a color map to store color values based on height map values
		Color[] colourMap = new Color[width * height];
		// Loop through each pixel in the height map
		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {
				// Calculate the color of the pixel by interpolating between black and white based on the height map value
				colourMap [y * width + x] = Color.Lerp (Color.black, Color.white, heightMap [x, y]);
			}
		}

		// Calculate the color of the pixel by interpolating between black and white based on the height map value
		return TextureFromColourMap (colourMap, width, height);
	}

}