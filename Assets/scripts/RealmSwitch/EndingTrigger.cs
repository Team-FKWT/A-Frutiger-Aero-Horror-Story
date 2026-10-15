using System.Collections.Generic;
using UnityEngine;

/// Put this on the final exit with a trigger collider. When the player walks in
/// with every required puzzle solved and in the required segment, the ending
/// starts. Otherwise nothing happens, so the ending can't be reached by
/// skipping a segment. The player object must be tagged "Player".
 
[RequireComponent(typeof(Collider))]
public class EndingTrigger : MonoBehaviour
{
    [Tooltip("Every puzzle that must be solved before the ending. List the required puzzles from every shipped segment.")]
    [SerializeField] private List<string> requiredPuzzleIds = new List<string>();
    [Tooltip("The player must have reached at least this segment.")]
    [SerializeField, Min(1)] private int requiredSegment = 1;

    // Runs when the component is added in the editor: makes the collider a trigger.
    private void Reset() => GetComponent<Collider>().isTrigger = true;

    /// Returns true if the player has everything needed to reach the ending.
    public bool RequirementsMet() => GetMissingRequirement() == null;

    /// Returns a short description of the first unmet requirement, or null if all are met.
    public string GetMissingRequirement()
    {
        ProgressionState state = ProgressionState.Instance;
        if (state.CurrentSegment < requiredSegment)
            return $"in segment {state.CurrentSegment}, needs segment {requiredSegment}";
        foreach (string id in requiredPuzzleIds)
        {
            if (!state.IsPuzzleSolved(id)) return $"puzzle {id} not solved";
        }
        return null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        string missing = GetMissingRequirement();
        if (missing == null) ProgressionState.Instance.StartEnding();
        else Debug.Log($"[Ending] Blocked: {missing}");
    }
}
