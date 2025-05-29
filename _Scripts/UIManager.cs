using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI livesText; // Assign a TMP text for lives display

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

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

    public void RestartGame()
    {
        // Reset stats before reloading
        ScoreManager.Instance?.ResetScore();
        PlayerStatsManager.Instance?.ResetLives();
        Time.timeScale = 1f; // Ensure time scale is normal
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f; // Ensure time scale is normal
        SceneManager.LoadScene("StartMenu"); // Assuming your menu scene is "StartMenu"
    }
}