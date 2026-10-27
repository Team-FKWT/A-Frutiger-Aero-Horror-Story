using System;
using System.Collections.Generic;
using UnityEngine;

/// The two versions of the world the player can be in.
public enum Realm { Reality, Dream }

/// Why a switch attempt was refused. None means switching is allowed.
public enum SwitchBlockReason { None, NotAtSwitchPoint, PuzzleUnsolved, NoPillsLeft}

/// The two choices the player can make at the final exit.
public enum EndingChoice { ExposeTheTruth, RemainIgnorant }


/// Single source of truth for game progress: current realm, current segment,
/// and which puzzles are solved. Survives realm switches and scene loads.
/// Everything teammates need is a public method or event on
/// ProgressionState.Instance.
 
[DefaultExecutionOrder(-100)] // Awake runs before other scripts, so Instance is ready in their OnEnable
public class ProgressionState : MonoBehaviour
{
    /// The one ProgressionState in the game.
    public static ProgressionState Instance { get; private set; }

    [Header("Live state (watch these in the Inspector during play)")]
    [SerializeField] private Realm currentRealm = Realm.Reality;
    [SerializeField] private int currentSegment = 1;
    [SerializeField] private List<string> solvedPuzzles = new List<string>();
    [SerializeField] private SwitchPoint activeSwitchPoint;
    [SerializeField] private bool endingStarted;
    [SerializeField] private int pillsLeft = 0;

    /// Fired after the realm changes. Argument is the new realm.
    public event Action<Realm> RealmChanged;
    /// Fired when a switch attempt is refused ("reality resists").
    public event Action<SwitchBlockReason> SwitchDenied;
    /// Fired the first time a puzzle is marked solved. Argument is its ID.
    public event Action<string> PuzzleSolved;
    /// Fired when the player moves to a different segment.
    public event Action<int> SegmentChanged;
    /// Fired once when the ending begins. The ending screen listens for this to show the final choice.
    public event Action EndingStarted;
    /// Fired when the player makes the final choice. Argument is the chosen ending.
    public event Action<EndingChoice> EndingChosen;

    /// The realm the player is currently in.
    public Realm CurrentRealm => currentRealm;
    /// The segment the player is currently in (1-based).
    public int CurrentSegment => currentSegment;
    /// True once the ending has begun.
    public bool HasEndingStarted => endingStarted;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => Instance = null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // ---------- Puzzles ----------

    /// Marks a puzzle as solved. Safe to call more than once.
    /// <param name="puzzleId">The puzzle's ID, e.g. "S1_Door".</param>
    public void MarkPuzzleSolved(string puzzleId)
    {
        if (string.IsNullOrEmpty(puzzleId) || solvedPuzzles.Contains(puzzleId)) return;
        solvedPuzzles.Add(puzzleId);
        PuzzleSolved?.Invoke(puzzleId);
    }

    /// Returns true if the puzzle with this ID has been solved.
    public bool IsPuzzleSolved(string puzzleId) => solvedPuzzles.Contains(puzzleId);

    // ---------- Segments ----------

    /// Sets the segment the player is in. Call when they cross into a new one.
    public void SetSegment(int segment)
    {
        if (segment == currentSegment) return;
        currentSegment = segment;
        SegmentChanged?.Invoke(currentSegment);
    }

    // ---------- Ending ----------

    /// Starts the ending hand-off and fires EndingStarted. Called by EndingTrigger
    /// at the final exit (or by the debug overlay). Ignored if the ending already started.
    
    public void StartEnding()
    {
        if (endingStarted) return;
        endingStarted = true;
        EndingStarted?.Invoke();
    }

    /// Records the player's final choice and fires EndingChosen. Called by the ending screen.
    /// <param name="choice">The ending the player picked.</param>
    public void ChooseEnding(EndingChoice choice)
    {
        if (!endingStarted) return;
        EndingChosen?.Invoke(choice);
    }

    // ---------- Switching ----------

    /// Returns why switching is blocked right now, or None if it is allowed.
    public SwitchBlockReason GetSwitchBlockReason()
    {
        if (activeSwitchPoint == null) return SwitchBlockReason.NotAtSwitchPoint;
        if (pillsLeft <= 0) return SwitchBlockReason.NoPillsLeft;
        if (!activeSwitchPoint.IsUnlocked) return SwitchBlockReason.PuzzleUnsolved;
        return SwitchBlockReason.None;
    }

    /// Returns true if the player is allowed to switch realms right now.
    public bool CanSwitch() => GetSwitchBlockReason() == SwitchBlockReason.None;

    /// Call this when the player presses the switch key. If switching is allowed,
    /// flips the realm, fires RealmChanged and returns true. If not, fires
    /// SwitchDenied and returns false.
     
    public bool TrySwitch()
    {
        SwitchBlockReason reason = GetSwitchBlockReason();
        if (reason != SwitchBlockReason.None)
        {
            SwitchDenied?.Invoke(reason);
            return false;
        }
        currentRealm = currentRealm == Realm.Reality ? Realm.Dream : Realm.Reality;
        RealmChanged?.Invoke(currentRealm);
        return true;
    }

    /// Called by SwitchPoint when the player walks in. Teammates don't need this.
    public void EnterSwitchPoint(SwitchPoint point) => activeSwitchPoint = point;

    /// Called by SwitchPoint when the player walks out. Teammates don't need this.
    public void ExitSwitchPoint(SwitchPoint point)
    {
        if (activeSwitchPoint == point) activeSwitchPoint = null;
    }
    
    //// Called when interacting with pill bottles.
    public void GetPills(int count)
    {
        pillsLeft += count;
    }
}
