using System.Collections.Generic;
using UnityEngine;

/// Puts a scene back into its "already solved" state when it loads. Use it for puzzles
/// whose own scripts don't restore themselves (like the S1 keycard reader), so that after
/// a stealth reset reloads the scene, nothing looks undone and nothing has to be repeated.
/// If the puzzle is solved: every listed PuzzleActor is triggered again (e.g. unlocking a
/// door) and every listed object is hidden (e.g. a keycard that was already picked up).
/// Put it on any object in the scene, such as GameManager.
public class PuzzleStateRestorer : MonoBehaviour
{
    [Tooltip("The puzzle to check, e.g. S1_Keycards.")]
    [SerializeField] private string puzzleId = "S1_Keycards";
    [Tooltip("Triggered again if the puzzle is already solved, e.g. a DoorController to unlock.")]
    [SerializeField] private List<PuzzleActor> actorsToTrigger = new List<PuzzleActor>();
    [Tooltip("Hidden if the puzzle is already solved, e.g. keycards already used.")]
    [SerializeField] private List<GameObject> objectsToHide = new List<GameObject>();

    private void Start()
    {
        ProgressionState state = ProgressionState.Instance;
        if (state == null || !state.IsPuzzleSolved(puzzleId)) return;

        foreach (PuzzleActor actor in actorsToTrigger)
            if (actor != null) actor.OnSolve();
        foreach (GameObject obj in objectsToHide)
            if (obj != null) obj.SetActive(false);

        Debug.Log($"[PuzzleStateRestorer] {puzzleId} already solved: scene restored.");
    }
}