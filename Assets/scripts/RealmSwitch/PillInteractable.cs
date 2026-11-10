using UnityEngine;

/// Refills the player's pill counter when interacted with.
public class PillInteractable : MonoBehaviour
{
    private bool collected;

    /// Collects this pill and adds a random amount from 1 through 5.
    public void OnInteract(object playerReference)
    {
        if (collected || ProgressionState.Instance == null) return;

        int refillAmount = Random.Range(1, 6);
        ProgressionState.Instance.AddPills(refillAmount);
        collected = true;
        gameObject.SetActive(false);
    }
}