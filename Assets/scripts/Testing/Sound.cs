using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class UniversalSoundTrigger : MonoBehaviour
{
    public enum TriggerMode
    {
        OnTriggerEnterZone,
        OnInteraction,
        ManualCallOnly
    }

    [Header("Audio Settings")]
    [Tooltip("List of clips to randomly pick from. If only one is provided, it plays that one.")]
    public AudioClip[] soundClips;
    [Range(0f, 1f)] public float volume = 1.0f;
    [Range(0.5f, 1.5f)] public float pitch = 1.0f;
    public bool playOnce = false;

    [Header("Trigger Behavior")]
    public TriggerMode triggerMode = TriggerMode.OnTriggerEnterZone;
    [Tooltip("Optional tag filter for OnTriggerEnter (leave blank to allow any object)")]
    public string requiredTag = "Player";

    private AudioSource audioSource;
    private bool hasTriggered = false;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1.0f; // 1 = 3D sound, 0 = 2D sound
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggerMode != TriggerMode.OnTriggerEnterZone) return;
        if (playOnce && hasTriggered) return;

        if (string.IsNullOrEmpty(requiredTag) || other.CompareTag(requiredTag))
        {
            PlaySound();
        }
    }

    /// <summary>
    /// Can be called directly from UI buttons, UnityEvents, or other scripts (e.g., Interaction system)
    /// without modifying this script.
    /// </summary>
    public void InteractOrTrigger()
    {
        if (triggerMode == TriggerMode.OnInteraction || triggerMode == TriggerMode.ManualCallOnly)
        {
            if (playOnce && hasTriggered) return;
            PlaySound();
        }
    }

    public void PlaySound()
    {
        if (soundClips == null || soundClips.Length == 0) return;

        AudioClip clip = soundClips[Random.Range(0, soundClips.Length)];
        if (clip != null)
        {
            audioSource.volume = volume;
            audioSource.pitch = pitch;
            audioSource.PlayOneShot(clip);

            if (playOnce)
            {
                hasTriggered = true;
            }
        }
    }
}
