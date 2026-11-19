using System.Collections.Generic;
using UnityEngine;

public class EvidenceLog : MonoBehaviour
{
    [SerializeField] private EvidenceEntry[] startingEntries;
    [Tooltip("World evidence available in every scene's log, restored after it has been collected.")]
    [SerializeField] private EvidenceEntry[] collectibleEntries;
    [SerializeField] private EvidenceEntry pillBottleEntry;
    [SerializeField] private string pillBottlePuzzleId = "S1_PillBottle";

    public IReadOnlyList<EvidenceEntry> Entries => entries;

    private readonly List<EvidenceEntry> entries = new List<EvidenceEntry>();
    private ProgressionState progression;

    private void OnEnable()
    {
        entries.Clear();
        if (startingEntries != null)
        {
            foreach (EvidenceEntry entry in startingEntries)
                AddEntry(entry);
        }

        progression = ProgressionState.Instance;
        if (progression == null) return;
        progression.PuzzleSolved += OnPuzzleSolved;
        if (progression.IsPuzzleSolved(pillBottlePuzzleId))
            AddEntry(pillBottleEntry);
        if (collectibleEntries != null)
        {
            foreach (EvidenceEntry entry in collectibleEntries)
            {
                if (entry != null && progression.IsPuzzleSolved(EvidenceKey(entry)))
                    AddEntry(entry);
            }
        }
    }

    private void OnDisable()
    {
        if (progression != null) progression.PuzzleSolved -= OnPuzzleSolved;
    }

    private void OnPuzzleSolved(string puzzleId)
    {
        if (puzzleId == pillBottlePuzzleId) AddEntry(pillBottleEntry);
        if (collectibleEntries == null) return;
        foreach (EvidenceEntry entry in collectibleEntries)
        {
            if (entry != null && puzzleId == EvidenceKey(entry)) AddEntry(entry);
        }
    }

    public bool Collect(EvidenceEntry entry)
    {
        if (entry == null || string.IsNullOrWhiteSpace(entry.Id) || progression == null) return false;
        bool added = AddEntry(entry);
        progression.MarkPuzzleSolved(EvidenceKey(entry));
        return added;
    }

    private static string EvidenceKey(EvidenceEntry entry) => "Evidence." + entry.Id;

    private bool AddEntry(EvidenceEntry entry)
    {
        if (entry == null || string.IsNullOrWhiteSpace(entry.Id)) return false;
        foreach (EvidenceEntry existing in entries)
        {
            if (existing.Id == entry.Id) return false;
        }
        entries.Add(entry);
        return true;
    }
}
