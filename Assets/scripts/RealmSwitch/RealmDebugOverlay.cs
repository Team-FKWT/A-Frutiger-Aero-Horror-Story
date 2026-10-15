using System.Collections.Generic;
using System.Text;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// Development-only debug panel. Press the backquote key (`) to show or hide it.
/// Shows the current realm, current segment, each puzzle's solved state, whether
/// switching is allowed right now, the ending state, and the last few events.
/// Press = to force the ending (skips all requirements, for testing only).
/// Removes itself in release builds. Put it on the GameManager next to ProgressionState.
 
public class RealmDebugOverlay : MonoBehaviour
{
    [SerializeField] private bool startVisible = false;
    [Tooltip("Puzzles to always list. Puzzles required by switch points in the scene are found automatically.")]
    [SerializeField] private List<string> extraPuzzleIds = new List<string>();
    [SerializeField, Range(1, 10)] private int eventLogLength = 5;

    private const float PanelWidth = 380f, Margin = 16f, Padding = 12f;

    private ProgressionState state;
    private bool visible;
    private string endingStatus = "not reached";
    private float nextRefresh;
    private readonly List<string> puzzleIds = new List<string>();
    private readonly Queue<string> events = new Queue<string>();
    private readonly StringBuilder text = new StringBuilder();
    private GUIStyle boxStyle, labelStyle;

    private void Awake()
    {
        // Release builds never show debug tools.
        if (!Debug.isDebugBuild)
        {
            Destroy(this);
            return;
        }
        visible = startVisible;
    }

    private void Start()
    {
        state = ProgressionState.Instance;
        if (state == null)
        {
            Debug.LogError("RealmDebugOverlay needs a ProgressionState in the scene.");
            enabled = false;
            return;
        }
        state.RealmChanged += OnRealmChanged;
        state.SwitchDenied += OnSwitchDenied;
        state.PuzzleSolved += OnPuzzleSolved;
        state.SegmentChanged += OnSegmentChanged;
        state.EndingStarted += OnEndingStarted;
        state.EndingChosen += OnEndingChosen;
        RefreshPuzzleList();
    }

    private void OnDestroy()
    {
        if (state == null) return;
        state.RealmChanged -= OnRealmChanged;
        state.SwitchDenied -= OnSwitchDenied;
        state.PuzzleSolved -= OnPuzzleSolved;
        state.SegmentChanged -= OnSegmentChanged;
        state.EndingStarted -= OnEndingStarted;
        state.EndingChosen -= OnEndingChosen;
    }

    private void Update()
    {
        if (TogglePressed()) visible = !visible;

        if (ForceEndingPressed() && !state.HasEndingStarted)
        {
            Log("Ending forced (=, debug)");
            state.StartEnding();
        }

        // Scenes change, so look for switch points again once a second while open.
        if (visible && Time.unscaledTime >= nextRefresh)
        {
            RefreshPuzzleList();
            nextRefresh = Time.unscaledTime + 1f;
        }
    }

    private static bool TogglePressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.backquoteKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.BackQuote);
#endif
    }

    private static bool ForceEndingPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.equalsKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.Equals);
#endif
    }

    // Collects every puzzle ID used by a switch point in the scene, plus the extra list.
    private void RefreshPuzzleList()
    {
        puzzleIds.Clear();
        foreach (SwitchPoint point in FindObjectsByType<SwitchPoint>())
        {
            string id = point.RequiredPuzzleId;
            if (!string.IsNullOrEmpty(id) && !puzzleIds.Contains(id)) puzzleIds.Add(id);
        }
        foreach (string id in extraPuzzleIds)
        {
            if (!string.IsNullOrEmpty(id) && !puzzleIds.Contains(id)) puzzleIds.Add(id);
        }
        puzzleIds.Sort();
    }

    // ---------- Event log ----------

    private void OnRealmChanged(Realm realm) => Log($"Switched to {realm}");
    private void OnSwitchDenied(SwitchBlockReason reason) => Log($"Switch refused: {reason}");
    private void OnPuzzleSolved(string id) => Log($"Puzzle solved: {id}");
    private void OnSegmentChanged(int segment) => Log($"Entered segment {segment}");

    private void OnEndingStarted()
    {
        endingStatus = "choosing";
        Log("Ending started");
    }

    private void OnEndingChosen(EndingChoice choice)
    {
        endingStatus = $"chose {choice}";
        Log($"Ending chosen: {choice}");
    }

    private void Log(string message)
    {
        events.Enqueue($"{Time.time,6:0.0}s  {message}");
        while (events.Count > eventLogLength) events.Dequeue();
    }

    // ---------- Drawing ----------

    private void OnGUI()
    {
        if (!visible || state == null) return;
        if (boxStyle == null) BuildStyles();

        SwitchBlockReason reason = state.GetSwitchBlockReason();
        text.Clear();
        text.AppendLine("<b>REALM DEBUG</b>   <color=#9AA4B2>` to hide</color>");
        text.AppendLine();
        text.AppendLine($"Realm:     {RealmColored(state.CurrentRealm)}");
        text.AppendLine($"Segment:   {state.CurrentSegment}");
        text.AppendLine(reason == SwitchBlockReason.None
            ? "Can switch: <color=#7CFC9A>YES</color>"
            : $"Can switch: <color=#FF8A80>NO</color> ({reason})");
        text.AppendLine($"Ending:    {endingStatus}   <color=#9AA4B2>= to force</color>");
        text.AppendLine();
        text.AppendLine("<b>Puzzles</b>");
        if (puzzleIds.Count == 0) text.AppendLine("  <color=#9AA4B2>none found</color>");
        foreach (string id in puzzleIds)
        {
            text.AppendLine(state.IsPuzzleSolved(id)
                ? $"  <color=#7CFC9A>[x]</color> {id}"
                : $"  <color=#9AA4B2>[ ]</color> {id}");
        }
        text.AppendLine();
        text.AppendLine("<b>Recent events</b>");
        if (events.Count == 0) text.AppendLine("  <color=#9AA4B2>nothing yet</color>");
        foreach (string e in events) text.AppendLine("  " + e);

        GUIContent content = new GUIContent(text.ToString());
        float height = labelStyle.CalcHeight(content, PanelWidth - Padding * 2f) + Padding * 2f;
        Rect box = new Rect(Screen.width - PanelWidth - Margin, Margin, PanelWidth, height);
        GUI.Box(box, GUIContent.none, boxStyle);
        GUI.Label(new Rect(box.x + Padding, box.y + Padding, PanelWidth - Padding * 2f, height - Padding * 2f), content, labelStyle);
    }

    private static string RealmColored(Realm realm) =>
        realm == Realm.Dream ? "<color=#8FD8FF>Dream</color>" : "<color=#FFD58A>Reality</color>";

    private void BuildStyles()
    {
        Texture2D background = new Texture2D(1, 1);
        background.SetPixel(0, 0, new Color(0.05f, 0.07f, 0.1f, 0.82f));
        background.Apply();
        boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.normal.background = background;

        labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 15,
            richText = true,
            wordWrap = true
        };
        labelStyle.normal.textColor = Color.white;
    }
}
