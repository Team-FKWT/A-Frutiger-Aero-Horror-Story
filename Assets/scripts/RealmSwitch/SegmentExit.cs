using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// Moves the player to the next segment's scene, but only once its requirements are met.
/// Three ways to use it: an elevator panel calls CanUse() and Go(); pressing E on it
/// (Interactable layer + trigger collider); or walking into it (tick Walk In To Use).
/// Until the requirements are met it refuses with a message, so a segment can't be
/// skipped. Progress carries over because ProgressionState survives scene loads.
public class SegmentExit : MonoBehaviour
{
    [Header("Where it goes")]
    [Tooltip("Scene to load, e.g. S3_Lab. Must be in File > Build Profiles > Scene List.")]
    [SerializeField] private string nextSceneName = "S3_Lab";
    [Tooltip("Segment number the player is in after the move.")]
    [SerializeField, Min(1)] private int nextSegment = 3;

    [Header("Requirements")]
    [Tooltip("Every puzzle that must be solved before this exit works.")]
    [SerializeField] private List<string> requiredPuzzleIds = new List<string> { "S2_Login", "S2_Override" };
    [Tooltip("Only works in Reality (the Dream version of the exit is blocked).")]
    [SerializeField] private bool realityOnly = true;

    [Header("Messages")]
    [SerializeField, TextArea]
    private string lockedMessage =
        "B1 to B2: AUTHORIZED PERSONNEL ONLY.\nDirector's override required.";
    [SerializeField, TextArea]
    private string dreamMessage =
        "The screens won't let you leave.";

    [Header("Using it without a panel")]
    [Tooltip("Off: press E on it. On: walking into it uses it. Needs a trigger collider; the player must be tagged Player.")]
    [SerializeField] private bool walkInToUse = false;

    private bool leaving;

    /// True once every required puzzle is solved (ignores which realm the player is in).
    public bool IsUnlocked => GetMissingRequirement() == null;

    // Called by the player's interact raycast (E), when this sits on an Interactable object.
    public void OnInteract(object playerReference)
    {
        if (!walkInToUse) TryUse();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (walkInToUse && other.CompareTag("Player")) TryUse();
    }

    /// Returns a short description of the first unmet requirement, or null if all are met.
    public string GetMissingRequirement()
    {
        ProgressionState state = ProgressionState.Instance;
        if (state == null) return "no ProgressionState in the scene";
        foreach (string id in requiredPuzzleIds)
            if (!state.IsPuzzleSolved(id)) return $"puzzle {id} not solved";
        return null;
    }

    /// Checks every rule. Returns true if the player may leave now; otherwise
    /// message says why, ready to show on screen.
    public bool CanUse(out string message)
    {
        ProgressionState state = ProgressionState.Instance;
        if (realityOnly && state != null && state.CurrentRealm != Realm.Reality)
        {
            message = dreamMessage;
            return false;
        }
        string missing = GetMissingRequirement();
        if (missing != null)
        {
            Debug.Log($"[SegmentExit] Blocked: {missing}");
            message = lockedMessage;
            return false;
        }
        if (!Application.CanStreamedLevelBeLoaded(nextSceneName))
        {
            Debug.LogWarning($"[SegmentExit] Scene '{nextSceneName}' is missing from the build's scene list.");
            message = $"(Dev) {nextSceneName} isn't in the build's scene list yet.";
            return false;
        }
        message = null;
        return true;
    }

    /// Leaves for the next segment. Call CanUse first.
    public void Go()
    {
        if (leaving) return;
        leaving = true;
        if (ProgressionState.Instance != null) ProgressionState.Instance.SetSegment(nextSegment);
        SceneManager.LoadScene(nextSceneName);
    }

    // Used by E and walk-in: shows the reason at the bottom of the screen if it can't go.
    private void TryUse()
    {
        if (CanUse(out string message)) Go();
        else ScreenMessage.Show(message);
    }
}