using UnityEngine;
using System.Collections.Generic; // Required for using Lists

public class BalloonSpawner : MonoBehaviour
{
    [Header("Balloon Settings")]
    public GameObject balloonBasePrefab;  // Assign your base Balloon Prefab (with Balloon.cs)
    public List<BalloonTypeData> availableBalloonTypes; // Assign your BalloonTypeData ScriptableObjects

    [Header("Spawning Area")]
    public float spawnXMin = -7f;
    public float spawnXMax = 7f;
    public float spawnYPosition = -6f;  // Y position where balloons will appear

    [Header("Initial Difficulty Settings")]
    public float initialSpawnRate = 2.0f;     // How often to spawn initially
    public float initialSpeedModifier = 1.0f; // Initial speed multiplier for balloons

    [Header("Difficulty Progression")]
    public float difficultyIncreaseInterval = 20f; // Time in seconds to increase difficulty
    [Space(5)]
    public float minSpawnRate = 0.5f;               // The fastest spawn rate achievable
    public float spawnRateDecreaseFactor = 0.1f;   // Amount to decrease spawnRate by each interval
    [Space(5)]
    public float maxSpeedModifier = 2.5f;           // Maximum speed multiplier
    public float speedModifierIncreaseFactor = 0.1f;// Amount to increase speedModifier by each interval

    // Internal state variables
    private float currentSpawnRate;
    private float currentSpeedModifier;
    private float nextSpawnTime;
    private float timeSinceLastDifficultyIncrease = 0f;

    void Start()
    {
        // Initialize current difficulty settings
        currentSpawnRate = initialSpawnRate;
        currentSpeedModifier = initialSpeedModifier;

        // Set the first spawn time
        nextSpawnTime = Time.time + Random.Range(0, currentSpawnRate * 0.5f); // Spawn a bit sooner at the start

        // Error checks for essential assignments
        if (balloonBasePrefab == null)
        {
            Debug.LogError("Balloon Base Prefab not assigned in BalloonSpawner! Spawning will fail.", this);
        }
        if (availableBalloonTypes == null || availableBalloonTypes.Count == 0)
        {
            Debug.LogError("No BalloonTypeData assigned to BalloonSpawner! Spawning will fail.", this);
        }
    }

    void Update()
    {
        // --- Difficulty Progression ---
        // Only progress difficulty if in Classic mode (or if GameModeManager doesn't exist)
        bool canProgressDifficulty = GameModeManager.Instance == null ||
                                     GameModeManager.Instance.CurrentGameMode == GameModeManager.Mode.Classic;

        if (canProgressDifficulty)
        {
            timeSinceLastDifficultyIncrease += Time.deltaTime;
            if (timeSinceLastDifficultyIncrease >= difficultyIncreaseInterval)
            {
                IncreaseDifficulty();
                timeSinceLastDifficultyIncrease = 0f; // Reset timer
            }
        }

        // --- Balloon Spawning ---
        if (Time.time >= nextSpawnTime)
        {
            SpawnBalloon();
            // Set the next spawn time based on the current (possibly decreased) spawn rate
            nextSpawnTime = Time.time + currentSpawnRate;
        }
    }

    void IncreaseDifficulty()
    {
        // Decrease spawn rate, but not below minSpawnRate
        currentSpawnRate = Mathf.Max(minSpawnRate, currentSpawnRate - spawnRateDecreaseFactor);

        // Increase speed modifier, but not above maxSpeedModifier
        currentSpeedModifier = Mathf.Min(maxSpeedModifier, currentSpeedModifier + speedModifierIncreaseFactor);

        Debug.Log("Difficulty Increased! New Spawn Rate: " + currentSpawnRate.ToString("F2") +
                  ", New Speed Modifier: " + currentSpeedModifier.ToString("F2"));
    }

    void SpawnBalloon()
    {
        // Check again in case something went wrong or lists are empty
        if (balloonBasePrefab == null || availableBalloonTypes == null || availableBalloonTypes.Count == 0)
        {
            // Debug.LogWarning("Cannot spawn balloon - essential references missing.");
            return;
        }

        // 1. Select a random BalloonTypeData from the list
        BalloonTypeData selectedType = availableBalloonTypes[Random.Range(0, availableBalloonTypes.Count)];

        // 2. Determine a random X position for spawning
        float randomX = Random.Range(spawnXMin, spawnXMax);
        Vector3 spawnPosition = new Vector3(randomX, spawnYPosition, 0); // Z is 0 for 2D

        // 3. Instantiate the base balloon prefab
        GameObject newBalloonGO = Instantiate(balloonBasePrefab, spawnPosition, Quaternion.identity);

        // 4. Get the Balloon script component and initialize it
        Balloon balloonScript = newBalloonGO.GetComponent<Balloon>();
        if (balloonScript != null)
        {
            // Pass the selected type data and the current speed modifier
            balloonScript.Initialize(selectedType, currentSpeedModifier);
        }
        else
        {
            Debug.LogError("Spawned balloon (from " + balloonBasePrefab.name + ") does not have a Balloon.cs script component!", newBalloonGO);
            Destroy(newBalloonGO); // Clean up the incorrectly configured balloon
        }
    }
}