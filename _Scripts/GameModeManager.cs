using UnityEngine;

public class GameModeManager : MonoBehaviour
{
    public static GameModeManager Instance { get; private set; }

    public enum Mode { Classic, Zen }
    public Mode CurrentGameMode { get; private set; } = Mode.Classic; // Default to Classic

    // References to different backgrounds (Sprite or Material for a Skybox)
    public Sprite classicBackgroundSprite;
    public Sprite zenBackgroundSprite;
    public SpriteRenderer mainBackgroundRenderer; // Assign your main background's SpriteRenderer

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Make this persist between menu and game scene
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetGameMode(Mode mode)
    {
        CurrentGameMode = mode;
        Debug.Log("Game Mode Set: " + CurrentGameMode);
        // ApplyModeSettings(); // Call this when the game scene loads
    }

    // Call this method when the game scene (MainGame) starts
    public void ApplyModeSettings()
    {
        if (mainBackgroundRenderer == null)
        {
            // Try to find it if not assigned - you might need a more robust way
            GameObject bgObject = GameObject.Find("BackgroundGameObject"); // Name your background object consistently
            if (bgObject != null) mainBackgroundRenderer = bgObject.GetComponent<SpriteRenderer>();
        }

        if (mainBackgroundRenderer != null)
        {
            if (CurrentGameMode == Mode.Zen && zenBackgroundSprite != null)
            {
                mainBackgroundRenderer.sprite = zenBackgroundSprite;
            }
            else if (CurrentGameMode == Mode.Classic && classicBackgroundSprite != null)
            {
                mainBackgroundRenderer.sprite = classicBackgroundSprite; // Default or classic
            }
        }
        else
        {
            Debug.LogWarning("MainBackgroundRenderer not assigned or found for GameModeManager.");
        }

        // Reset lives if it's classic mode, or effectively give infinite for Zen
        if (PlayerStatsManager.Instance != null)
        {
            if (CurrentGameMode == Mode.Classic) PlayerStatsManager.Instance.ResetLives();
            // For Zen, the LoseLife check in Balloon.cs will prevent life loss.
        }
         if (ScoreManager.Instance != null) ScoreManager.Instance.ResetScore();

        // Hide/Show Lives UI based on mode
        if (UIManager.Instance != null && UIManager.Instance.livesText != null)
        {
            UIManager.Instance.livesText.gameObject.SetActive(CurrentGameMode == Mode.Classic);
        }
    }
}