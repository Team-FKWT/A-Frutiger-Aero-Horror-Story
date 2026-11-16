using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(menuName = "Audio/Audio Cue")]
public class AudioCue : ScriptableObject
{
    [SerializeField] private AudioClip clip;
    [SerializeField] private AudioMixerGroup output;
    [SerializeField, Range(0f, 1f)] private float volume = 1f;

    public AudioClip Clip => clip;
    public float Volume => volume;

    public void Configure(AudioSource source)
    {
        source.clip = clip;
        source.outputAudioMixerGroup = output;
        source.volume = volume;
        source.playOnAwake = false;
    }
}
