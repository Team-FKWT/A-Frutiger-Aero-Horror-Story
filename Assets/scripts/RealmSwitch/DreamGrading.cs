using UnityEngine;
using UnityEngine.Rendering;

/// Fades a post-processing Volume in while the player is in the Dream and out in
/// Reality. Put it on a Global Volume that holds the Dream color grading profile
/// (pink, purple and green). Listens to ProgressionState like the other realm scripts.

[RequireComponent(typeof(Volume))]
public class DreamGrading : MonoBehaviour
{
    [Tooltip("Seconds to fade the grading in or out on a realm switch.")]
    [SerializeField, Min(0f)] private float fadeTime = 0.6f;

    private Volume volume;
    private ProgressionState state;
    private bool subscribed;
    private float target;

    private void Awake()
    {
        volume = GetComponent<Volume>();
        volume.weight = 0f;
    }

    private void OnEnable() => Subscribe();
    // Second chance in case ProgressionState was not ready during OnEnable.
    private void Start() => Subscribe();

    private void OnDisable()
    {
        if (subscribed && state != null) state.RealmChanged -= OnRealmChanged;
        subscribed = false;
    }

    private void Subscribe()
    {
        if (subscribed) return;
        state = ProgressionState.Instance;
        if (state == null) return;
        state.RealmChanged += OnRealmChanged;
        subscribed = true;
        // Match the current realm right away, with no fade.
        target = state.CurrentRealm == Realm.Dream ? 1f : 0f;
        volume.weight = target;
    }

    private void OnRealmChanged(Realm realm) => target = realm == Realm.Dream ? 1f : 0f;

    private void Update()
    {
        if (Mathf.Approximately(volume.weight, target)) return;
        float step = fadeTime <= 0f ? 1f : Time.unscaledDeltaTime / fadeTime;
        volume.weight = Mathf.MoveTowards(volume.weight, target, step);
    }
}