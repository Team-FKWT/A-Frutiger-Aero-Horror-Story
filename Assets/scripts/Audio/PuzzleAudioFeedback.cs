using UnityEngine;

[RequireComponent(typeof(AudioCueEmitter))]
public class PuzzleAudioFeedback : MonoBehaviour
{
    private ProgressionState progression;
    private AudioCueEmitter emitter;

    private void OnEnable()
    {
        emitter = GetComponent<AudioCueEmitter>();
        progression = ProgressionState.Instance;
        if (progression != null) progression.PuzzleSolved += PlaySolved;
    }

    private void OnDisable()
    {
        if (progression != null) progression.PuzzleSolved -= PlaySolved;
    }

    private void PlaySolved(string puzzleId) => emitter.Play();
}
