using UnityEngine;
using TMPro; // Make sure to import TextMeshPro

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; } // Singleton property

    public TextMeshProUGUI scoreTextDisplay; // Assign your UI Text element here in the Inspector

    private int currentScore = 0;

    void Awake()
    {
        // Singleton pattern setup
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Optional: if you want score to persist between scenes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
        }
    }

    void Start()
    {
        UpdateScoreDisplay(); // Initialize display
    }

    public void AddScore(int amount)
    {
        currentScore += amount;
        UpdateScoreDisplay();
        // Debug.Log("Score: " + currentScore);
    }

    void UpdateScoreDisplay()
    {
        if (scoreTextDisplay != null)
        {
            scoreTextDisplay.text = "Score: " + currentScore;
        }
        else
        {
            Debug.LogWarning("ScoreTextDisplay not assigned in ScoreManager!");
        }
    }

    public int GetCurrentScore()
    {
        return currentScore;
    }

    public void ResetScore()
    {
        currentScore = 0;
        UpdateScoreDisplay();
    }
}