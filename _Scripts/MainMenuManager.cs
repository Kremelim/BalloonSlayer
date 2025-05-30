using UnityEngine;
using UnityEngine.SceneManagement; // Required for scene loading
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public string gameSceneName = "MainGame"; // Set this in Inspector if your game scene has a different name
    public GameObject settingsPanel; // Assign SettingsPanel_UI from StartMenu scene

    public Sprite[] availableCrosshairs; // Assign your crosshair sprites in Inspector
    public Image currentCrosshairPreview; // Optional: An Image UI element to show selected crosshair
    public GameObject crosshairSelectionUIParent; // Parent for dynamically created buttons OR static buttons
    public const string SELECTED_CROSSHAIR_KEY = "SelectedCrosshairIndex";

    // ADD THESE LINES IF THEY AREN'T THERE or are mistyped:
    [Header("UI References for Settings Panel")] // Optional: for better organization in Inspector
    public Slider musicSliderInSettingsPanel;
    public Slider sfxSliderInSettingsPanel;

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

    public void OpenSettingsPanel()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            InitializeCrosshairSelection();
            // Now, tell AudioManager to use these specific UI elements and refresh them
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.RefreshSettingsUI(
                    musicSliderInSettingsPanel,
                    sfxSliderInSettingsPanel
                );
            }
            else
            {
                Debug.LogError("AudioManager.Instance is null when trying to open settings!");
            }
        }
        // Optionally: Deactivate main menu buttons panel
    }


    public void CloseSettingsPanel()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        // Optionally: Reactivate main menu buttons panel
    }
    
    public void InitializeCrosshairSelection()
    {
        // For now, let's assume Option 2: Static buttons are set up, and we just update the preview image.
        // If you have no preview image, this part might not do much visually until a button is clicked.
        int currentlySelected = PlayerPrefs.GetInt(SELECTED_CROSSHAIR_KEY, 0);
        if (currentCrosshairPreview != null && availableCrosshairs != null && availableCrosshairs.Length > currentlySelected)
        {
            currentCrosshairPreview.sprite = availableCrosshairs[currentlySelected];
        }
        else if (currentCrosshairPreview != null && (availableCrosshairs == null || availableCrosshairs.Length == 0))
        {
            Debug.LogWarning("MainMenuManager: 'Available Crosshairs' array is not assigned or empty, cannot set preview.");
        }
    }

    public void SelectCrosshair(int index) // This is the core logic
    {
        if (availableCrosshairs == null || index < 0 || index >= availableCrosshairs.Length)
        {
            Debug.LogWarning("MainMenuManager: SelectCrosshair called with invalid index or unassigned 'Available Crosshairs'. Index: " + index);
            return;
        }

        PlayerPrefs.SetInt(SELECTED_CROSSHAIR_KEY, index);
        if (currentCrosshairPreview != null)
        {
            currentCrosshairPreview.sprite = availableCrosshairs[index];
        }
        Debug.Log("Selected Crosshair Index: " + index + ", Name: " + availableCrosshairs[index].name);
    }

    // Helper for UI buttons if using static buttons with an int parameter in their onClick event
    public void SelectCrosshairFromButton(int index)
    {
        SelectCrosshair(index);
    }
}