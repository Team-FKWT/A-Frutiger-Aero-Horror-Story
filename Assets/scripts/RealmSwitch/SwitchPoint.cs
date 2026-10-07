using UnityEngine;

/// A marked "out of order" spot where the player may switch realms.
/// This goes on a GameObject with a trigger collider covering the area.
/// Make sure player object is tagged "Player"!

[RequireComponent(typeof(Collider))]
public class SwitchPoint : MonoBehaviour
{
    [Tooltip("Puzzle that must be solved before this point works. Leave empty for no requirement.")]
    [SerializeField] private string requiredPuzzleId;

    /// True once the puzzle guarding this switch point is solved.
    public bool IsUnlocked =>
        string.IsNullOrEmpty(requiredPuzzleId) ||
        ProgressionState.Instance.IsPuzzleSolved(requiredPuzzleId);

    // Runs when the component is added in the editor: makes the collider a trigger.
    private void Reset() => GetComponent<Collider>().isTrigger = true;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) ProgressionState.Instance.EnterSwitchPoint(this);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) ProgressionState.Instance.ExitSwitchPoint(this);
    }

    private void OnDisable()
    {
        if (ProgressionState.Instance != null) ProgressionState.Instance.ExitSwitchPoint(this);
    }
}
