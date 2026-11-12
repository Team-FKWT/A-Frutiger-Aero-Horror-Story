using UnityEngine;
using UnityEngine.Audio;

// Scene-owned mixer controls. Settings UI can call these setters with values from 0 to 1.
[DefaultExecutionOrder(-50)]
public class AudioEngine : MonoBehaviour
{
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private AudioMixerGroup effectsGroup;

    public static AudioEngine Instance { get; private set; }
    public float MasterVolume { get; private set; }
    public float MusicVolume { get; private set; }
    public float SfxVolume { get; private set; }
    public AudioMixerGroup EffectsGroup => effectsGroup;

    private bool ready;
    private bool settingsChanged;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => Instance = null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Only one AudioEngine should be active.", this);
            enabled = false;
            return;
        }

        Instance = this;
        MasterVolume = ReadVolume("MasterVolume");
        MusicVolume = ReadVolume("MusicVolume");
        SfxVolume = ReadVolume("SfxVolume");
    }

    private void Start()
    {
        if (mixer == null)
        {
            Debug.LogError("AudioEngine needs its audio mixer assigned.", this);
            enabled = false;
            return;
        }

        // Unity mixer parameters must be applied in Start or later.
        ready = true;
        ApplyVolume("MasterVolume", MasterVolume);
        ApplyVolume("MusicVolume", MusicVolume);
        ApplyVolume("SfxVolume", SfxVolume);
    }

    public void SetMasterVolume(float volume)
    {
        MasterVolume = Mathf.Clamp01(volume);
        StoreVolume("MasterVolume", MasterVolume);
    }

    public void SetMusicVolume(float volume)
    {
        MusicVolume = Mathf.Clamp01(volume);
        StoreVolume("MusicVolume", MusicVolume);
    }

    public void SetSfxVolume(float volume)
    {
        SfxVolume = Mathf.Clamp01(volume);
        StoreVolume("SfxVolume", SfxVolume);
    }

    private static float ReadVolume(string parameter)
    {
        return Mathf.Clamp01(PlayerPrefs.GetFloat("Audio." + parameter, 1f));
    }

    private void StoreVolume(string parameter, float volume)
    {
        PlayerPrefs.SetFloat("Audio." + parameter, volume);
        settingsChanged = true;
        if (ready) ApplyVolume(parameter, volume);
    }

    private void ApplyVolume(string parameter, float volume)
    {
        float decibels = volume <= 0f ? -80f : Mathf.Max(-80f, 20f * Mathf.Log10(volume));
        if (!mixer.SetFloat(parameter, decibels))
            Debug.LogError("Missing exposed mixer parameter: " + parameter, this);
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused) SaveSettings();
    }

    private void OnDisable()
    {
        SaveSettings();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void SaveSettings()
    {
        if (!settingsChanged) return;
        PlayerPrefs.Save();
        settingsChanged = false;
    }
}
