using UnityEngine;

// This attribute allows you to create instances of this ScriptableObject
// from the Assets > Create menu in the Unity Editor.
[CreateAssetMenu(fileName = "NewBalloonType", menuName = "BalloonSlayer/Balloon Type Data")]
public class BalloonTypeData : ScriptableObject
{
    [Header("Identification")]
    public string balloonName = "Default Balloon"; // For editor identification and debugging

    [Header("Visuals")]
    public Sprite sprite;               // The visual representation of the balloon
    public Color colorTint = Color.white; // Tint to apply to the sprite (useful if sprite is white/grayscale)

    [Header("Gameplay Stats")]
    public int hitPoints = 1;           // How many hits to pop this balloon
    public int scoreValue = 10;         // Points awarded when this balloon is popped

    [Header("Movement")]
    public float baseFloatSpeed = 2f;   // Base upward speed before any global speed modifiers

    // [Header("Optional Movement Pattern - Advanced")]
    // public enum MovementPattern { StraightUp, SinWave, ZigZag }
    // public MovementPattern movementType = MovementPattern.StraightUp;
    // public float waveFrequency = 1f;    // For SinWave or ZigZag
    // public float waveAmplitude = 0.5f;  // For SinWave or ZigZag
    // public float changeDirectionInterval = 2f; // For ZigZag

    [Header("Audio")]
    public AudioClip popSound;          // Specific pop sound for this balloon type (optional, can override generic)
    // public AudioClip spawnSound;     // Optional: sound when this type spawns

    // You can add more properties here as needed, for example:
    // public GameObject specialEffectOnPopPrefab;
    // public float sizeModifier = 1f;
    // public bool isSpecial = false; // For unique behaviors
}