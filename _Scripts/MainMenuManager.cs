using UnityEngine;
using UnityEngine.SceneManagement; // Required for scene loading

public class MainMenuManager : MonoBehaviour
{
    public string gameSceneName = "MainGame"; // Set this in Inspector if your game scene has a different name

    public void StartClassicMode()
    {
    GameModeManager.Instance?.SetGameMode(GameModeManager.Mode.Classic);
    SceneManager.LoadScene(gameSceneName);
    }

    public void StartZenMode()
    {
    GameModeManager.Instance?.SetGameMode(GameModeManager.Mode.Zen);
    SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();

        // If running in the Unity Editor
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}