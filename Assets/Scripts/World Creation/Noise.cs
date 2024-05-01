using UnityEngine;
using System.Collections;

// A static class to generate noise maps using Perlin noise
public static class Noise {

	// Enumeration to specify the normalization mode
	public enum NormalizeMode { Local, Global };

	// Method to generate a 2D noise map
	public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset, NormalizeMode normalizeMode) {
		// Create a 2D array to store the noise map
		float[,] noiseMap = new float[mapWidth,mapHeight];

		// Initialize a pseudo-random number generator with the given seed
		System.Random prng = new System.Random (seed);
		// Array to store offsets for each octave
		Vector2[] octaveOffsets = new Vector2[octaves];

		// Variables to calculate maximum possible height for normalization
		float maxPossibleHeight = 0;
		float amplitude = 1;
		float frequency = 1;

		// Calculate octave offsets and maximum possible height
		for (int i = 0; i < octaves; i++) {
			float offsetX = prng.Next (-100000, 100000) + offset.x;
			float offsetY = prng.Next (-100000, 100000) - offset.y;
			octaveOffsets [i] = new Vector2 (offsetX, offsetY);

			maxPossibleHeight += amplitude;
			amplitude *= persistance;
		}

		// Ensure scale is not zero to prevent division by zero
		if (scale <= 0) {
			scale = 0.0001f;
		}

		// Variables to track local min and max noise heights
		float maxLocalNoiseHeight = float.MinValue;
		float minLocalNoiseHeight = float.MaxValue;

		// Calculate half-width and half-height for centering the map
		float halfWidth = mapWidth / 2f;
		float halfHeight = mapHeight / 2f;

		// Generate noise values for each point in the map
		for (int y = 0; y < mapHeight; y++) {
			for (int x = 0; x < mapWidth; x++) {

				amplitude = 1;
				frequency = 1;
				float noiseHeight = 0;

				// Combine octaves to generate Perlin noise
				for (int i = 0; i < octaves; i++) {
					float sampleX = (x - halfWidth + octaveOffsets[i].x) / scale * frequency;
					float sampleY = (y - halfHeight + octaveOffsets[i].y) / scale * frequency;

					float perlinValue = Mathf.PerlinNoise (sampleX, sampleY) * 2 - 1;
					noiseHeight += perlinValue * amplitude;

					amplitude *= persistance;
					frequency *= lacunarity;
				}

				// Update local min and max noise heights
				if (noiseHeight > maxLocalNoiseHeight) {
					maxLocalNoiseHeight = noiseHeight;
				} else if (noiseHeight < minLocalNoiseHeight) {
					minLocalNoiseHeight = noiseHeight;
				}
				// Store the noise value in the map
				noiseMap [x, y] = noiseHeight;
			}
		}

		// Normalize the noise map based on the specified mode
		for (int y = 0; y < mapHeight; y++) {
			for (int x = 0; x < mapWidth; x++) {
				if (normalizeMode == NormalizeMode.Local) {
					// Normalize locally between min and max noise heights
					noiseMap [x, y] = Mathf.InverseLerp (minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap [x, y]);
				} else {
					// Normalize globally using max possible height
					float normalizedHeight = (noiseMap [x, y] + 1) / (maxPossibleHeight / 0.9f);
					noiseMap [x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
				}
			}
		}

		// Return the generated noise map
		return noiseMap;
	}

}
