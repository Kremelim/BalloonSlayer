// Create PlayerStatsManager.cs or add these to ScoreManager.cs
using UnityEngine;
using UnityEngine.Events; // For UnityEvents

public class PlayerStatsManager : MonoBehaviour // Or merge into ScoreManager
{
    public static PlayerStatsManager Instance { get; private set; }

    public int maxLives = 3;
    private int currentLives;

    public UnityEvent onPlayerDied; // Event to trigger when lives reach 0
    public UnityEvent<int> onLivesChanged; // Event to pass current lives to UI

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        currentLives = maxLives;
        onLivesChanged?.Invoke(currentLives);
    }

    public void LoseLife()
    {
        if (currentLives > 0)
        {
            currentLives--;
            onLivesChanged?.Invoke(currentLives);
            Debug.Log("Lost a life! Lives remaining: " + currentLives);

            if (currentLives <= 0)
            {
                onPlayerDied?.Invoke(); // Trigger game over event
                Debug.Log("Game Over!");
            }
        }
    }

    public void ResetLives()
    {
        currentLives = maxLives;
        onLivesChanged?.Invoke(currentLives);
    }

    public int GetCurrentLives()
    {
        return currentLives;
    }
}