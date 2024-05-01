using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// This is our MainMenu class. It's a blueprint for creating MainMenu objects.
public class MainMenu : MonoBehaviour
{
    // Awake is a function that runs once when the object is created, before any other function runs.
    void Awake()
    {
        // This line makes sure our MainMenu object doesn't get destroyed when we load a new scene.
        // This is useful if we want to keep some information or settings between different scenes.
        DontDestroyOnLoad(gameObject);
    }

    // This is a function we can call to start the game.
    public void PlayGame()
    {
        // This line loads the next scene in the list of scenes in our game.
        // For example, if we're in the main menu scene and the next scene is the game scene, this will start the game.
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    // This is a function we can call to quit the game.
    public void QuitGame()
    {
        // This line is for us, the developers. It will write "QUITTING GAME" in the console so we know this function was called.
        Debug.Log("QUITTING GAME");

        // This line will close the game. If you're playing the game, it will stop. If you're testing the game in the editor, it will stop the play mode.
        Application.Quit();
    }
}
