using UnityEngine;

// Place beside the object's normal interaction so reading it records the evidence.
public class EvidenceInteraction : MonoBehaviour
{
    [SerializeField] private EvidenceLog log;
    [SerializeField] private EvidenceEntry entry;

    public void OnInteract(object playerReference)
    {
        if (playerReference is GameObject && log != null && log.Collect(entry))
            ScreenMessage.Show("Evidence recorded. Press J to view the log.");
    }
}
