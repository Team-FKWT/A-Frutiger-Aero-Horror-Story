using UnityEngine;

public class PillBottleController : MonoBehaviour
{
    [SerializeField] public int pillsInside = 3;
    
    // All Interactable-layer objects should implement OnInteract() to handle user interaction.
    public void OnInteract(object playerReference)
    {
        ProgressionState.Instance.GetPills(pillsInside);
        DestroyImmediate(gameObject);
    }
}
