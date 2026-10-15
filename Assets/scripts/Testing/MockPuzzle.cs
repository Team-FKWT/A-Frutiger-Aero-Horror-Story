using UnityEngine;

/// TEST ONLY. Stands in for a real puzzle. When the player walks into this
/// object, it tells ProgressionState the puzzle is solved and turns green.
 
[RequireComponent(typeof(Collider))]
public class MockPuzzle : MonoBehaviour
{
    [SerializeField] private string puzzleId = "S1_Door";

    // Runs when the component is added in the editor: makes the collider a trigger.
    private void Reset() => GetComponent<Collider>().isTrigger = true;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        ProgressionState.Instance.MarkPuzzleSolved(puzzleId);
        Renderer rend = GetComponent<Renderer>();
        if (rend != null) rend.material.color = Color.green;
    }
}
