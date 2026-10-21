using UnityEngine;

public class KeycardPickup : MonoBehaviour
{
    [SerializeField] private KeycardColor cardColor;
    [SerializeField] private KeycardPuzzleState puzzleState;

    private bool collected;

    public void OnInteract(object playerReference)
    {
        if (collected || puzzleState == null)
            return;

        puzzleState.CollectCard(cardColor);
        collected = true;

        gameObject.SetActive(false);
    }
}
