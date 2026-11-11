using UnityEngine;

/// An item the player picks up with E. Picking it up marks a puzzle solved in
/// ProgressionState, optionally triggers a PuzzleActor (such as unlocking a
/// DoorController), shows a message, and hides the item. If the puzzle is already
/// solved when the scene loads (for example after a stealth reset), the item starts
/// hidden and the actor is triggered again, so nothing has to be repeated.
/// Put this on an object on the Interactable layer with a collider.
public class PuzzlePickup : MonoBehaviour
{
    [Tooltip("Puzzle ID marked solved on pickup, e.g. S2_Override.")]
    [SerializeField] private string puzzleId = "S2_Override";
    [Tooltip("Optional: triggered on pickup, e.g. a DoorController to unlock.")]
    [SerializeField] private PuzzleActor puzzleActor;
    [SerializeField, TextArea]
    private string pickupMessage =
        "Picked up: Director's elevator override.\nThe office door unlocks from the inside.";

    private void Start()
    {
        // Already picked up before a reload: restore the result quietly.
        if (IsSolved())
        {
            if (puzzleActor != null) puzzleActor.OnSolve();
            gameObject.SetActive(false);
        }
    }

    // Called by the player's interact raycast (E).
    public void OnInteract(object playerReference)
    {
        if (IsSolved()) return;
        if (ProgressionState.Instance != null) ProgressionState.Instance.MarkPuzzleSolved(puzzleId);
        if (puzzleActor != null) puzzleActor.OnSolve();
        if (!string.IsNullOrEmpty(pickupMessage)) ScreenMessage.Show(pickupMessage);
        gameObject.SetActive(false);
    }

    private bool IsSolved() =>
        ProgressionState.Instance != null && ProgressionState.Instance.IsPuzzleSolved(puzzleId);
}