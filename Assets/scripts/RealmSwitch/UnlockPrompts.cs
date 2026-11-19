using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Shows short on-screen prompts when the player unlocks something: what a solved
/// puzzle opened, how to use the pills, and the Dream double jump the first time the
/// player enters the Dream. Uses ScreenMessage, so nothing else needs to be placed.
/// Added after Playtest 1: the tester heard the unlock sound and saw the pillar change
/// color but didn't know why, so each unlock now says what happened.
/// Put this on the GameManager next to ProgressionState in every segment.

public class UnlockPrompts : MonoBehaviour
{
    [System.Serializable]
    public class Prompt
    {
        [Tooltip("The puzzle ID that triggers this prompt, e.g. S1_Keycards.")]
        public string puzzleId;
        [TextArea(1, 3)] public string message;
        [Tooltip("Seconds to wait before showing it, so it doesn't cover another message.")]
        public float delay;
        public float duration = 4f;
    }

    [Tooltip("Prompts shown when a puzzle is solved. The defaults cover segments 1 to 3; IDs not in this scene are simply never triggered.")]
    [SerializeField]
    private List<Prompt> prompts = new List<Prompt>
    {
        new Prompt { puzzleId = "S1_PillBottle", delay = 4f,
            message = "Press Q at a glowing switch point to take a pill and enter the Dream." },
        new Prompt { puzzleId = "S1_Keycards",
            message = "You hear a door unlock. A new switch point is open." },
        new Prompt { puzzleId = "S2_Login",
            message = "Login accepted. A switch point on this floor is now open." },
        new Prompt { puzzleId = "S3_Lock", delay = 3.5f,
            message = "A switch point in the lab is now open." },
    };

    [TextArea(1, 3)]
    [SerializeField] private string firstDreamMessage = "In the Dream you can double jump: press Space twice.";
    [SerializeField] private float firstDreamDelay = 1.5f;

    // Shared across scenes, so the Dream tip only shows once per playthrough.
    private static bool dreamTipShown;

    private ProgressionState state;
    private bool subscribed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => dreamTipShown = false;

    private void OnEnable() => Subscribe();
    // Second chance in case ProgressionState was not ready during OnEnable.
    private void Start() => Subscribe();

    private void OnDisable()
    {
        if (subscribed && state != null)
        {
            state.PuzzleSolved -= OnPuzzleSolved;
            state.RealmChanged -= OnRealmChanged;
        }
        subscribed = false;
    }

    private void Subscribe()
    {
        if (subscribed) return;
        state = ProgressionState.Instance;
        if (state == null) return;
        state.PuzzleSolved += OnPuzzleSolved;
        state.RealmChanged += OnRealmChanged;
        subscribed = true;
    }

    private void OnPuzzleSolved(string puzzleId)
    {
        foreach (Prompt prompt in prompts)
        {
            if (prompt.puzzleId == puzzleId && !string.IsNullOrEmpty(prompt.message))
                StartCoroutine(ShowLater(prompt.message, prompt.delay, prompt.duration));
        }
    }

    private void OnRealmChanged(Realm realm)
    {
        if (realm != Realm.Dream || dreamTipShown || string.IsNullOrEmpty(firstDreamMessage)) return;
        dreamTipShown = true;
        StartCoroutine(ShowLater(firstDreamMessage, firstDreamDelay, 4f));
    }

    private static IEnumerator ShowLater(string message, float delay, float duration)
    {
        if (delay > 0f) yield return new WaitForSecondsRealtime(delay);
        ScreenMessage.Show(message, duration);
    }
}