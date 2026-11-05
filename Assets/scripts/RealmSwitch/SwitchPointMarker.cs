using UnityEngine;

/// The visible marker for a switch point. Shows one color while the switch point
/// is locked and another once it is unlocked, and plays a chime at the moment the
/// player solves the puzzle that unlocks it. Put this on an object with a Renderer
/// and drag the SwitchPoint it belongs to into the Switch Point slot.
[RequireComponent(typeof(Renderer))]
public class SwitchPointMarker : MonoBehaviour
{
    [Tooltip("The switch point this marker shows the state of.")]
    [SerializeField] private SwitchPoint switchPoint;
    [SerializeField] private Color lockedColor = Color.red;
    [SerializeField] private Color unlockedColor = Color.green;

    [Header("Unlock sound")]
    [Tooltip("Sound to play on unlock. If left empty, a short chime is generated.")]
    [SerializeField] private AudioClip unlockSound;
    [SerializeField, Range(0f, 1f)] private float volume = 0.7f;

    private ProgressionState state;
    private Renderer markerRenderer;
    private AudioSource audioSource;

    private void Start()
    {
        state = ProgressionState.Instance;
        if (state == null || switchPoint == null)
        {
            Debug.LogError(name + ": SwitchPointMarker needs a ProgressionState in the scene and a Switch Point assigned.");
            enabled = false;
            return;
        }

        markerRenderer = GetComponent<Renderer>();
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound: heard from anywhere, since the player may be far away
        if (unlockSound == null) unlockSound = MakeChime();

        ShowCurrentState();
        state.PuzzleSolved += OnPuzzleSolved;
    }

    private void OnDestroy()
    {
        if (state != null) state.PuzzleSolved -= OnPuzzleSolved;
    }

    private void OnPuzzleSolved(string puzzleId)
    {
        if (puzzleId != switchPoint.RequiredPuzzleId) return;
        ShowCurrentState();
        audioSource.PlayOneShot(unlockSound, volume);
    }

    private void ShowCurrentState()
    {
        markerRenderer.material.color = switchPoint.IsUnlocked ? unlockedColor : lockedColor;
    }

    // Two rising notes, each fading out.
    private AudioClip MakeChime()
    {
        const int sampleRate = 44100;
        const float noteLength = 0.18f;
        int noteSamples = Mathf.CeilToInt(sampleRate * noteLength);
        float[] samples = new float[noteSamples * 2];

        for (int i = 0; i < samples.Length; i++)
        {
            bool secondNote = i >= noteSamples;
            float frequency = secondNote ? 1046.5f : 784f;
            float t = (float)(secondNote ? i - noteSamples : i) / sampleRate;
            float fade = 1f - t / noteLength;
            samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * fade * 0.5f;
        }

        AudioClip clip = AudioClip.Create("UnlockChime", samples.Length, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
