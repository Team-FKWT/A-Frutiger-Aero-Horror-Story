using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[DisallowMultipleComponent]
public class AudioCueEmitter : MonoBehaviour
{
    [SerializeField] private AudioCue cue;
    [SerializeField, Range(0f, 1f)] private float spatialBlend;
    [SerializeField, Min(0.01f)] private float minDistance = 1f;
    [SerializeField, Min(0.01f)] private float maxDistance = 18f;
    private AudioSource source;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        if (cue == null || cue.Clip == null)
        {
            Debug.LogError("AudioCueEmitter needs an audio cue.", this);
            enabled = false;
            return;
        }

        cue.Configure(source);
        source.loop = false;
        source.spatialBlend = spatialBlend;
        source.dopplerLevel = 0f;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.minDistance = minDistance;
        source.maxDistance = Mathf.Max(minDistance, maxDistance);
    }

    public void Play()
    {
        if (isActiveAndEnabled) source.PlayOneShot(cue.Clip);
    }
}
