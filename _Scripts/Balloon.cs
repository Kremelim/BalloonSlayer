using UnityEngine;

public class Balloon : MonoBehaviour
{
    // This will be assigned by the BalloonSpawner via the Initialize method
    public BalloonTypeData typeData { get; private set; }

    private int currentHitPoints;
    private float currentFloatSpeed; // Actual speed after modifiers

    // Component References
    private AudioSource audioSource;
    private SpriteRenderer spriteRenderer;
    private Collider2D col2D;

    // --- Public fields from your original script that are now handled by TypeData or dynamically ---
    // public float floatSpeed = 2f; // Now from typeData.baseFloatSpeed + modifier
    // public int scoreValue = 10;   // Now from typeData.scoreValue
    public AudioClip popSoundEffect;  // Still useful if you want a generic pop sound,
                                      // or BalloonTypeData could also have a specific sound
                                      // ---------------------------------------------------------------------------------------------

    private float timeSinceLastDirectionChange = 0f;
    private int horizontalDirection = 1; // 1 for right, -1 for left
    private float initialXPosition; // To keep zigzag centered or based on spawn

    void Awake()
    {
        // Get references to components
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("Balloon is missing a SpriteRenderer component!", gameObject);
        }

        col2D = GetComponent<Collider2D>();
        if (col2D == null)
        {
            Debug.LogError("Balloon is missing a Collider2D component!", gameObject);
        }
    }

    // Call this method from BalloonSpawner after instantiating the balloon
    public void Initialize(BalloonTypeData data, float speedModifier = 1f)
    {
        typeData = data;

        if (typeData == null)
        {
            Debug.LogError("Balloon initialized with null TypeData! Destroying.", gameObject);
            Destroy(gameObject);
            return;
        }

        // Set properties from BalloonTypeData
        currentHitPoints = typeData.hitPoints;
        currentFloatSpeed = typeData.baseFloatSpeed * speedModifier;
        // scoreValue is now implicitly typeData.scoreValue when popping
        // popSoundEffect could also be set from typeData if desired:
        // if (typeData.specificPopSound != null) this.popSoundEffect = typeData.specificPopSound;

        // Apply visual properties
        if (spriteRenderer != null)
        {
            if (typeData.sprite != null)
            {
                spriteRenderer.sprite = typeData.sprite;
            }
            spriteRenderer.color = typeData.colorTint; // Applies tint over the sprite
        }
        
        // Store initial X position for some movement patterns
        initialXPosition = transform.position.x;
        // Randomize initial horizontal direction for variety
        horizontalDirection = (Random.value > 0.5f) ? 1 : -1;
        timeSinceLastDirectionChange = 0f;

        // Set GameObject name for easier debugging in Hierarchy
        gameObject.name = typeData.balloonName + "_Instance (" + typeData.movementType.ToString() + ")";
    }

    void Update()
    {
        // Don't do anything if not properly initialized
        if (typeData == null) return;

        // --- Vertical Movement (common to all) ---
        float verticalMovementDelta = currentFloatSpeed * Time.deltaTime;

        // --- Handle Movement Based on Pattern ---
        if (typeData.movementType == MovementPattern.SinWave)
        {
            // For SinWave, we directly set the X position based on time and initial spawn X
            // The Y position is still translated upwards.
            float newX = initialXPosition + typeData.waveAmplitude * Mathf.Sin(Time.time * typeData.waveFrequency);
            transform.position = new Vector3(newX, transform.position.y + verticalMovementDelta, transform.position.z);
        }
        else // For StraightUp and ZigZag, use Translate
        {
            float horizontalMovementDelta = 0f;
            if (typeData.movementType == MovementPattern.ZigZag)
            {
                timeSinceLastDirectionChange += Time.deltaTime;
                if (timeSinceLastDirectionChange >= typeData.directionChangeInterval)
                {
                    horizontalDirection *= -1;
                    timeSinceLastDirectionChange = 0f;
                }
                horizontalMovementDelta = horizontalDirection * typeData.horizontalSpeed * Time.deltaTime;
            }
            // For StraightUp, horizontalMovementDelta remains 0.
            transform.Translate(new Vector3(horizontalMovementDelta, verticalMovementDelta, 0));
        }

        CheckOffScreen(); // This still needs to be called for all movement types
    }

    void CheckOffScreen()
    {
        if (transform.position.y > 10f) // Adjust '10f' based on your camera's top view
        {
            if (GameModeManager.Instance != null && GameModeManager.Instance.CurrentGameMode == GameModeManager.Mode.Classic)
            {
                PlayerStatsManager.Instance?.LoseLife();
            }
            Destroy(gameObject);
        }
    }

    // This public method is called when the balloon is "hit" by a projectile or click
    public void TakeDamage(int damageAmount = 1) // Assuming each hit does 1 damage for now
    {
        if (typeData == null || currentHitPoints <= 0) return; // Already popped or not initialized

        currentHitPoints -= damageAmount;
        Debug.Log(gameObject.name + " took damage. HP left: " + currentHitPoints);

        // Optional: Visual feedback for taking a hit (e.g., flash color, animation)
        // if (spriteRenderer != null) {
        //     spriteRenderer.color = Color.Lerp(typeData.colorTint, Color.white, 0.5f); // Example flash
        //     Invoke("ResetVisualsAfterHit", 0.1f);
        // }

        if (currentHitPoints <= 0)
        {
            ActuallyPop();
        }
    }

    // void ResetVisualsAfterHit()
    // {
    //     if (spriteRenderer != null && typeData != null)
    //     {
    //         spriteRenderer.color = typeData.colorTint;
    //     }
    // }

    private void ActuallyPop()
    {
        // Award score based on the balloon's typeData
        ScoreManager.Instance?.AddScore(typeData.scoreValue);

        // Play pop sound, if assigned
        if (popSoundEffect != null && audioSource != null)
        {
            audioSource.PlayOneShot(popSoundEffect);
        }
        else if (typeData.popSound != null && audioSource != null) // Check if typeData has a specific sound
        {
            audioSource.PlayOneShot(typeData.popSound);
        }


        // Important: Disable the balloon visually and physically immediately
        if (spriteRenderer != null) spriteRenderer.enabled = false;
        if (col2D != null) col2D.enabled = false;

        // Destroy the balloon GameObject.
        // If there's a sound, delay destruction slightly to let the sound play.
        float destroyDelay = 0.05f; // Default small delay
        AudioClip soundToPlay = popSoundEffect != null ? popSoundEffect : (typeData != null ? typeData.popSound : null);

        if (soundToPlay != null && audioSource != null)
        {
            destroyDelay = soundToPlay.length;
        }
        Destroy(gameObject, destroyDelay);
    }

    // Note: Your CrosshairController's HandleShoot() method should now call:
    // Balloon balloon = hit.collider.GetComponent<Balloon>();
    // if (balloon != null)
    // {
    //     balloon.TakeDamage(1); // Or whatever damage your "shot" does
    // }
}