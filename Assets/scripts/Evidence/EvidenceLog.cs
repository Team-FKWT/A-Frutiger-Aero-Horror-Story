using System.Collections.Generic;
using UnityEngine;

public class EvidenceLog : MonoBehaviour
{
    [SerializeField] private EvidenceEntry[] startingEntries;
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
    }

    private void OnDisable()
    {
        if (progression != null) progression.PuzzleSolved -= OnPuzzleSolved;
    }

    private void OnPuzzleSolved(string puzzleId)
    {
        if (puzzleId == pillBottlePuzzleId) AddEntry(pillBottleEntry);
    }

    private void AddEntry(EvidenceEntry entry)
    {
        if (entry == null || string.IsNullOrWhiteSpace(entry.Id)) return;
        foreach (EvidenceEntry existing in entries)
        {
            if (existing.Id == entry.Id) return;
        }
        entries.Add(entry);
    }
}
