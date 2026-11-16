using UnityEngine;

[CreateAssetMenu(menuName = "Evidence/Evidence Entry")]
public class EvidenceEntry : ScriptableObject
{
    [SerializeField] private string entryId;
    [SerializeField] private string title;
    [SerializeField, TextArea(2, 8)] private string description;
    [SerializeField] private Texture2D document;

    public string Id => entryId;
    public string Title => title;
    public string Description => description;
    public Texture2D Document => document;
}
