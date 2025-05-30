using UnityEngine;
using UnityEngine.Audio; // Required for AudioMixer
using UnityEngine.UI;    // Required for Slider and Toggle UI elements
using UnityEngine.SceneManagement; // Needed if you want to re-hook on scene load

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public AudioMixer mainMixer; // Assign your Master AudioMixer asset here
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;

    public const string MUSIC_VOL_KEY = "MusicVolume";
    public const string SFX_VOL_KEY = "SFXVolume";
    private bool uiInitializedThisScene = false; // To prevent multiple listener setups

    public AudioSource backgroundMusicSource; // Assign this in Inspector
    private float originalMusicVolume = -1f; // To store volume before pausing

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Make AudioManager persist
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Call this method when your Settings Panel is opened/activated
    public void RefreshSettingsUI(Slider musicSlider, Slider sfxSlider)
    {
        Debug.Log("AudioManager: Refreshing Settings UI links.");
        this.musicVolumeSlider = musicSlider;
        this.sfxVolumeSlider = sfxSlider;

        // Remove old listeners before adding new ones to prevent multiple calls
        if (this.musicVolumeSlider != null) this.musicVolumeSlider.onValueChanged.RemoveAllListeners();
        if (this.sfxVolumeSlider != null) this.sfxVolumeSlider.onValueChanged.RemoveAllListeners();

        // Load current settings from PlayerPrefs and apply them to the UI
        LoadAndApplyVolumeSettingsToUI();

        // Add new listeners
        SetupUIListeners(); // Re-adds listeners to the newly assigned UI elements
        uiInitializedThisScene = true;
    }

    void SetupUIListeners()
    {
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
            Debug.Log("Music slider listener added.");
        }
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
            Debug.Log("SFX slider listener added.");
        }
    }

    public void SetMusicVolume(float volume) // volume is 0.0 to 1.0 from slider
    {
        // AudioMixer uses decibels (logarithmic). Convert linear slider value.
        // Slider value 0.0001 (almost silent) to 1.0 (full volume)
        // maps to -80dB (silent) to 0dB (full volume)
        mainMixer.SetFloat("MusicVolParam", Mathf.Log10(Mathf.Max(volume, 0.0001f)) * 20f);
        PlayerPrefs.SetFloat(MUSIC_VOL_KEY, volume);
    }

    public void SetSFXVolume(float volume)
    {
        mainMixer.SetFloat("SFXVolParam", Mathf.Log10(Mathf.Max(volume, 0.0001f)) * 20f);
        PlayerPrefs.SetFloat(SFX_VOL_KEY, volume);
    }

    public void LoadAndApplyVolumeSettingsToUI() // Public so it can be called externally if needed
    {
        float musicVol = PlayerPrefs.GetFloat(MUSIC_VOL_KEY, 0.75f);
        float sfxVol = PlayerPrefs.GetFloat(SFX_VOL_KEY, 0.75f);

        Debug.Log($"Loading settings - Music: {musicVol}, SFX: {sfxVol}");

        if (musicVolumeSlider != null) musicVolumeSlider.value = musicVol; else Debug.LogWarning("Music slider not assigned for loading.");
        if (sfxVolumeSlider != null) sfxVolumeSlider.value = sfxVol; else Debug.LogWarning("SFX slider not assigned for loading.");

        // Apply to mixer immediately as well, in case this is the first load
        mainMixer.SetFloat("MusicVolParam", Mathf.Log10(Mathf.Max(musicVol, 0.0001f)) * 20f);
        mainMixer.SetFloat("SFXVolParam", Mathf.Log10(Mathf.Max(sfxVol, 0.0001f)) * 20f);
    }
    void Start()
    {
        // Apply initial loaded settings to the mixer directly when AudioManager first starts.
        // The UI will be updated when the settings panel is opened.
        float musicVol = PlayerPrefs.GetFloat(MUSIC_VOL_KEY, 0.75f);
        float sfxVol = PlayerPrefs.GetFloat(SFX_VOL_KEY, 0.75f);

        mainMixer.SetFloat("MusicVolParam", Mathf.Log10(Mathf.Max(musicVol, 0.0001f)) * 20f);
        mainMixer.SetFloat("SFXVolParam", Mathf.Log10(Mathf.Max(sfxVol, 0.0001f)) * 20f);

        if (backgroundMusicSource == null)
        {
            // Try to find it if it's tagged or named uniquely
            GameObject musicObj = GameObject.FindWithTag("BackgroundMusic"); // Example
            if (musicObj != null) backgroundMusicSource = musicObj.GetComponent<AudioSource>();
        }
        if (backgroundMusicSource == null) Debug.LogWarning("AudioManager: BackgroundMusicSource not found/assigned.");
    }
    
    public void PauseAllSounds()
    {
        Debug.Log("AudioManager: PauseAllSounds() called.");
        // Pause background music
        if (backgroundMusicSource != null && backgroundMusicSource.isPlaying)
        {
            originalMusicVolume = backgroundMusicSource.volume; // Store current volume
            // backgroundMusicSource.Pause(); // Option 1: True pause
            backgroundMusicSource.volume = 0f; // Option 2: Mute by setting volume to 0
                                               // This might be better if Time.timeScale = 0 affects .Pause()
            Debug.Log("AudioManager: Background music paused/muted.");
        }

        // To pause ALL AudioSources in the scene (more drastic, might not be needed if Time.timeScale=0):
        // AudioSource[] allSources = FindObjectsOfType<AudioSource>();
        // foreach (AudioSource source in allSources)
        // {
        //     if (source != backgroundMusicSource) // Don't double-affect BGM
        //     {
        //          source.Pause(); // Or set volume to 0 if they should be silent
        //     }
        // }
        // Generally, Time.timeScale=0 handles most in-game SFX if they are event-driven.
    }

    public void ResumeAllSounds()
    {
        // Resume background music
        if (backgroundMusicSource != null)
        {
            // if (!backgroundMusicSource.isPlaying) backgroundMusicSource.UnPause(); // Option 1
            if (originalMusicVolume != -1f) // Check if volume was stored
            {
                backgroundMusicSource.volume = originalMusicVolume; // Option 2: Restore volume
                originalMusicVolume = -1f; // Reset flag
            }
            Debug.Log("AudioManager: Background music resumed.");
        }

        // AudioSource[] allSources = FindObjectsOfType<AudioSource>();
        // foreach (AudioSource source in allSources)
        // {
        //     if (source != backgroundMusicSource) source.UnPause();
        // }
    }

    // You might want a StopAllSounds for when quitting to menu vs just pausing
    public void StopBackgroundMusic()
    {
        if (backgroundMusicSource != null)
        {
            backgroundMusicSource.Stop();
            Debug.Log("AudioManager: Background music stopped.");
        }
    }
}