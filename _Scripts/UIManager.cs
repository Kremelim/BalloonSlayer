using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    public GameObject pauseMenuPanel; // Assign this in Inspector
    private bool isPaused = false;

    // Reference to PlayerControls if UIManager handles pause input directly
    private PlayerControls playerControls; // You might already have this for other UI if needed
    public Color classicScoreColor = Color.white; // Or whatever your default is
    public Color zenScoreColor = Color.yellow;    // Example: yellow for Zen mode
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI livesText; // Assign a TMP text for lives display

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        playerControls = new PlayerControls();
    }

    private void OnEnable() // If UIManager handles input
    {
        playerControls.Gameplay.Enable();
        playerControls.Gameplay.Pause.performed += ctx => TogglePauseGame();
    }

    private void OnDisable() // If UIManager handles input
    {
        playerControls.Gameplay.Pause.performed -= ctx => TogglePauseGame();
        playerControls.Gameplay.Disable();
    }

    public BalloonSpawner balloonSpawnerInstance; // Assign this in Inspector

    void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false); // Ensure it's hidden
        // Subscribe to events if PlayerStatsManager exists
        if (PlayerStatsManager.Instance != null)
        {
            PlayerStatsManager.Instance.onPlayerDied.AddListener(ShowGameOverPanel);
            PlayerStatsManager.Instance.onLivesChanged.AddListener(UpdateLivesDisplay);
            // Initialize lives display
            UpdateLivesDisplay(PlayerStatsManager.Instance.GetCurrentLives());
        }

        if (GameModeManager.Instance != null)
        {
            GameModeManager.Instance.ApplyModeSettings(); // Tell it to apply settings now
        }

        if (balloonSpawnerInstance == null)
        {
            balloonSpawnerInstance = FindAnyObjectByType<BalloonSpawner>();
        }
        if (balloonSpawnerInstance == null)
        {
            Debug.LogError("UIManager: BalloonSpawner instance not found!");
        }

        Cursor.visible = false;
    }

    void OnDestroy() // Important to unsubscribe
    {
        if (PlayerStatsManager.Instance != null)
        {
            PlayerStatsManager.Instance.onPlayerDied.RemoveListener(ShowGameOverPanel);
            PlayerStatsManager.Instance.onLivesChanged.RemoveListener(UpdateLivesDisplay);
        }
    }

    public void ShowGameOverPanel()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (finalScoreText != null && ScoreManager.Instance != null)
            {
                finalScoreText.text = "Final Score: " + ScoreManager.Instance.GetCurrentScore();
            }
            Cursor.visible = true;
        }

        // --- STOP BALLOON SPAWNING ---
        if (balloonSpawnerInstance != null)
        {
            balloonSpawnerInstance.StopSpawning();
        }
        Time.timeScale = 0f;
        Debug.Log("UIManager: Attempting to pause sounds...");
        AudioManager.Instance?.PauseAllSounds();
        Debug.Log("UIManager: Call to PauseAllSounds completed.");
    }

    // Modify or add to your ApplyModeSettings or a dedicated method
    public void UpdateScoreboardColor(GameModeManager.Mode mode)
    {
        if (ScoreManager.Instance != null && ScoreManager.Instance.scoreTextDisplay != null)
        {
            if (mode == GameModeManager.Mode.Zen)
            {
                ScoreManager.Instance.scoreTextDisplay.color = zenScoreColor;
            }
            else // Classic or any other mode
            {
                ScoreManager.Instance.scoreTextDisplay.color = classicScoreColor;
            }
        }
    }

    public void UpdateLivesDisplay(int currentLives)
    {
        if (livesText != null)
        {
            livesText.text = "Lives: " + currentLives;
            // Or use heart icons if you prefer more complex UI
        }
    }

    public void TogglePauseGame()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f; // Pause game time
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
            // Optionally: Disable other game inputs if needed, e.g., CrosshairController
            Cursor.visible = true;
            AudioManager.Instance?.PauseAllSounds(); // PAUSE audio
        }
        else
        {
            Time.timeScale = 1f; // Resume game time
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
            // Optionally: Re-enable other game inputs
            Cursor.visible = false;
            AudioManager.Instance?.ResumeAllSounds(); // RESUME audio
        }
    }

    public void RestartGame()
    {
        // Reset stats before reloading
        ScoreManager.Instance?.ResetScore();
        PlayerStatsManager.Instance?.ResetLives();
        Time.timeScale = 1f; // Ensure time scale is normal
        AudioManager.Instance?.ResumeAllSounds(); // Resume sounds before scene reload or state reset
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Cursor.visible = false;
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f; // Ensure time scale is normal
        AudioManager.Instance?.StopBackgroundMusic(); // Or ResumeAllSounds() if menu has its own music
        SceneManager.LoadScene("StartMenu"); // Assuming your menu scene is "StartMenu"
        Cursor.visible = true;
    }

    public void ResumeGame() // New method for the Resume button
    {
        if (isPaused)
        {
            TogglePauseGame(); // Will unpause
        }
    }
}