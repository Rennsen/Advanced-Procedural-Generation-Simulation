using UnityEngine;
using System.Collections;
using UnityEditor;

// Custom editor class for MapGenerator
[CustomEditor (typeof (MapGenerator))]
public class MapGeneratorEditor : Editor {

	// Override method for drawing the inspector GUI
	public override void OnInspectorGUI() {
		// Cast the target to MapGenerator
		MapGenerator mapGen = (MapGenerator)target;

		// Draw the default inspector and check if any values were changed
		if (DrawDefaultInspector ()) {
			// If autoUpdate is enabled, redraw the map in the editor
			if (mapGen.autoUpdate) {
				mapGen.DrawMapInEditor ();
			}
		}

		// Add a button to manually generate the map
		if (GUILayout.Button ("Generate")) {
			mapGen.DrawMapInEditor ();
		}
	}
}
