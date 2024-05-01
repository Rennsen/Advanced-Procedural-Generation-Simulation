using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class NumericInputField : MonoBehaviour
{
    public TMP_InputField inputField;
    public string Game; // Name of the scene where MapGenerator is located

    void Start()
    {
        // Load the MapGenerator scene
        SceneManager.LoadScene(Game, LoadSceneMode.Single);

        // Wait for the MapGenerator scene to be loaded
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Check if the loaded scene is the MapGenerator scene
        if (scene.name == Game)
        {
            // Remove the sceneLoaded event to prevent it from being called multiple times
            SceneManager.sceneLoaded -= OnSceneLoaded;

            // Now that the MapGenerator scene is loaded, you can access MapGenerator.Instance
            inputField.onValueChanged.AddListener(delegate
            {
                long number;
                if (long.TryParse(inputField.text, out number))
                {
                    if (number >= 0 && number <= 10000000000)
                    {
                        MapGenerator.Instance.SetSeed(number.ToString(), Vector2.zero); // Pass a default value for the 'centre' parameter
                    }
                }
            });
        }
    }
}

// // using UnityEngine;
// // using TMPro;

// // public class NumericInputField : MonoBehaviour
// // {
// //     public TMP_InputField inputField;

// //     void Start()
// //     {
// //         inputField.onValidateInput += delegate(string input, int charIndex, char addedChar)
// //         {
// //             string acceptableChars = "0123456789";
// //             if (!acceptableChars.Contains(addedChar.ToString())) return '\0';

// //             long number;
// //             if (long.TryParse(input + addedChar, out number))
// //             {
// //                 if (number >= 0 && number <= 10000000000) return addedChar;
// //             }

// //             return '\0';
// //         };
// //     }
// // }


// using UnityEngine;
// using TMPro;

// // public class NumericInputField : MonoBehaviour
// // {
// //     public TMP_InputField inputField;
// //     public MapGenerator mapGenerator;

// //     void Start()
// //     {
// //         inputField.onValueChanged.AddListener(mapGenerator.SetSeed);

// //         inputField.onValidateInput += delegate(string input, int charIndex, char addedChar)
// //         {
// //             string acceptableChars = "0123456789";
// //             if (!acceptableChars.Contains(addedChar.ToString())) return '\0';

// //             long number;
// //             if (long.TryParse(input + addedChar, out number))
// //             {
// //                 if (number >= 0 && number <= 10000000000) return addedChar;
// //             }

// //             return '\0';
// //         };
// //     }
// // }
// public class NumericInputField : MonoBehaviour
// {
//     public TMP_InputField inputField;

//     void Start()
//     {
//         inputField.onEndEdit.AddListener(delegate
//         {
//             long number;
//             if (long.TryParse(inputField.text, out number))
//             {
//                 if (number >= 0 && number <= 10000000000)
//                 {
//                     MapGenerator.Instance.SetSeed(number.ToString());
//                 }
//             }
//         });
//     }
// }













// // using UnityEngine;

// // public class MapGenerator : MonoBehaviour
// // {
// //     public NumericInputField numericInputField;
// //     private long seed;

// //     void Start()
// //     {
// //         seed = long.Parse(numericInputField.inputField.text);
// //         // Use the seed to adjust your map generation parameters here
// //     }
// // }