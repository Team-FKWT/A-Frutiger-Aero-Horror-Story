using UnityEngine;
using UnityEngine.UIElements;

/// Elevator floor panel, reused on every floor. Opened with E through an
/// InteractiveViewController on the elevator (same setup as the keycard reader).
/// Set Current Floor for the scene it's in. Each floor button can have a SegmentExit:
/// that floor shows as locked until the exit's required puzzles are solved, and once
/// unlocked it takes the player there. A floor with no exit just shows a message.
/// The labels refresh whenever the panel opens and whenever a puzzle is solved.
/// Put this on the object with the PanelRenderer.
public class ElevatorPanelController : MonoBehaviour
{
    public enum Floor { One, Two, B1 }

    [Header("This elevator")]
    [Tooltip("The floor this scene is on. Its button shows as 'you are here'.")]
    [SerializeField] private Floor currentFloor = Floor.Two;

    [Header("Exits (leave empty for floors you can't go to from here)")]
    [SerializeField] private SegmentExit floor1Exit;
    [SerializeField] private SegmentExit floor2Exit;
    [SerializeField] private SegmentExit floorB1Exit;

    [Header("Floors that are always locked from here (no exit, shown as LOCKED)")]
    [SerializeField] private bool floor1AlwaysLocked;
    [SerializeField] private bool floor2AlwaysLocked;
    [SerializeField] private bool floorB1AlwaysLocked;

    [Header("Links")]
    [Tooltip("The InteractiveViewController on the elevator. The X button closes the panel.")]
    [SerializeField] private MonoBehaviour viewController;

    [Header("Floor names")]
    [SerializeField] private string floor1Name = "Sleep Clinic and Reception";
    [SerializeField] private string floor2Name = "Offices";
    [SerializeField] private string floorB1Name = "Research Laboratory";

    [Header("Text")]
    [SerializeField] private string lockedLabel = "LOCKED: Authorized personnel only";
    [SerializeField] private string unlockedLabel = "Clearance accepted";
    [SerializeField, TextArea] private string noExitMessage = "No reason to go back there now.";
    [SerializeField, TextArea] private string currentFloorMessage = "You're already on this floor.";

    private PanelRenderer panel;
    private bool wasShowing;
    private bool subscribed;

    private readonly Button[] buttons = new Button[3];
    private readonly Label[] descs = new Label[3];
    private readonly Label[] locks = new Label[3];
    private Button closeButton;
    private Label floorDisplay;
    private Label statusText;

    private static readonly string[] Keys = { "1", "2", "B1" };
    private static readonly int[] Heights = { 1, 2, -1 }; // for "going up" vs "going down"
    private static readonly Color Normal = new Color(0.16f, 0.27f, 0.43f);
    private static readonly Color Bad = new Color(0.75f, 0.2f, 0.15f);
    private static readonly Color Good = new Color(0.1f, 0.5f, 0.25f);
    private static readonly Color CurrentFill = new Color(0.24f, 0.63f, 0.31f);
    private static readonly Color OtherFill = new Color(0.92f, 0.96f, 1f);

    private void Awake()
    {
        panel = GetComponent<PanelRenderer>();
        panel.RegisterUIReloadCallback(OnUIReload);
    }

    private void Start()
    {
        // Refresh the lock labels the moment any puzzle is solved.
        if (ProgressionState.Instance != null)
        {
            ProgressionState.Instance.PuzzleSolved += OnPuzzleSolved;
            subscribed = true;
        }
    }

    private void OnDestroy()
    {
        if (panel != null) panel.UnregisterUIReloadCallback(OnUIReload);
        if (subscribed && ProgressionState.Instance != null)
            ProgressionState.Instance.PuzzleSolved -= OnPuzzleSolved;
    }

    private void Update()
    {
        // Refresh each time the panel opens (the elevator turns the PanelRenderer on).
        bool showing = panel != null && panel.enabled;
        if (showing && !wasShowing) Refresh();
        wasShowing = showing;
    }

    private void OnPuzzleSolved(string puzzleId) => Refresh();

    private SegmentExit ExitFor(int i) => i == 0 ? floor1Exit : i == 1 ? floor2Exit : floorB1Exit;

    private bool AlwaysLocked(int i) => i == 0 ? floor1AlwaysLocked : i == 1 ? floor2AlwaysLocked : floorB1AlwaysLocked;

    private string NameFor(int i) => i == 0 ? floor1Name : i == 1 ? floor2Name : floorB1Name;

    private void OnUIReload(PanelRenderer panelRenderer, VisualElement root, int version)
    {
        string missing = "";
        for (int i = 0; i < 3; i++)
        {
            int index = i; // copy for the button action below
            buttons[i] = root.Q<Button>($"Floor{Keys[i]}Button");
            descs[i] = root.Q<Label>($"Floor{Keys[i]}Desc");
            locks[i] = root.Q<Label>($"Floor{Keys[i]}Lock");
            if (buttons[i] == null || descs[i] == null || locks[i] == null) { missing += $" Floor{Keys[i]}"; continue; }
            // Assigning a new Clickable replaces any old action, so reloads never double up.
            buttons[i].clickable = new Clickable(() => OnFloorPressed(index));
        }
        closeButton = root.Q<Button>("CloseButton");
        floorDisplay = root.Q<Label>("FloorDisplayText");
        statusText = root.Q<Label>("StatusText");
        if (closeButton == null) missing += " CloseButton";
        if (floorDisplay == null) missing += " FloorDisplayText";
        if (statusText == null) missing += " StatusText";
        if (missing.Length > 0)
        {
            Debug.LogError("ElevatorPanelController: ElevatorUI.uxml is missing:" + missing, this);
            return;
        }
        closeButton.clickable = new Clickable(Close);
        Refresh();
    }

    private void Refresh()
    {
        if (statusText == null) return;
        floorDisplay.text = Keys[(int)currentFloor];
        for (int i = 0; i < 3; i++)
        {
            if (buttons[i] == null) continue;
            bool here = i == (int)currentFloor;
            descs[i].text = NameFor(i) + (here ? " (you are here)" : "");
            descs[i].style.unityFontStyleAndWeight = here ? FontStyle.Bold : FontStyle.Normal;
            buttons[i].style.backgroundColor = here ? CurrentFill : OtherFill;
            buttons[i].style.color = here ? Color.white : Normal;

            SegmentExit exit = ExitFor(i);
            if (here)
            {
                locks[i].text = "";
            }
            else if (exit == null)
            {
                // No way there from here: optionally show it as locked.
                locks[i].text = AlwaysLocked(i) ? lockedLabel : "";
                locks[i].style.color = Bad;
            }
            else if (!exit.HasRequirements)
            {
                locks[i].text = ""; // open floor, nothing to unlock
            }
            else
            {
                bool unlocked = exit.IsUnlocked;
                locks[i].text = unlocked ? unlockedLabel : lockedLabel;
                locks[i].style.color = unlocked ? Good : Bad;
            }
        }
        SetStatus("Select a floor.", Normal);
    }

    private void OnFloorPressed(int i)
    {
        if (i == (int)currentFloor) { SetStatus(currentFloorMessage, Normal); return; }

        SegmentExit exit = ExitFor(i);
        if (exit == null)
        {
            if (AlwaysLocked(i)) SetStatus(lockedLabel, Bad);
            else SetStatus(noExitMessage, Normal);
            return;
        }

        if (exit.CanUse(out string message))
        {
            SetStatus(Heights[i] > Heights[(int)currentFloor] ? "Going up..." : "Going down...", Good);
            Close();          // give the player their controls and cursor back first
            exit.Go();
        }
        else
        {
            SetStatus(message, Bad);
        }
    }

    private void SetStatus(string text, Color color)
    {
        if (statusText == null) return;
        statusText.text = text;
        statusText.style.color = color;
    }

    private void Close()
    {
        if (viewController != null)
            viewController.SendMessage("DisengageInteract", SendMessageOptions.DontRequireReceiver);
    }
}
