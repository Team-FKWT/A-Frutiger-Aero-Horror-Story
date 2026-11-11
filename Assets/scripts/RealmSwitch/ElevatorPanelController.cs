using UnityEngine;
using UnityEngine.UIElements;

/// Elevator floor panel. Opened with E through an InteractiveViewController on the
/// elevator (same setup as the keycard reader). Floor B1 uses a SegmentExit: it shows
/// as locked until the exit's required puzzles are solved, and once unlocked it takes
/// the player to the next segment. The lock state refreshes whenever the panel opens
/// and whenever a puzzle is solved. Put this on the object with the PanelRenderer.
public class ElevatorPanelController : MonoBehaviour
{
    [Header("Links")]
    [Tooltip("The SegmentExit that the B1 button uses.")]
    [SerializeField] private SegmentExit downExit;
    [Tooltip("The InteractiveViewController on the elevator. The X button closes the panel.")]
    [SerializeField] private MonoBehaviour viewController;

    [Header("Text")]
    [SerializeField] private string lockedLabel = "LOCKED: Authorized personnel only";
    [SerializeField] private string unlockedLabel = "Clearance accepted";
    [SerializeField, TextArea] private string floor1Message = "No reason to go back down there now.";
    [SerializeField, TextArea] private string currentFloorMessage = "You're already on this floor.";

    private PanelRenderer panel;
    private bool wasShowing;
    private bool subscribed;

    private Button floor1Button;
    private Button floor2Button;
    private Button floorB1Button;
    private Button closeButton;
    private Label b1LockLabel;
    private Label statusText;

    private static readonly Color Normal = new Color(0.16f, 0.27f, 0.43f);
    private static readonly Color Bad = new Color(0.75f, 0.2f, 0.15f);
    private static readonly Color Good = new Color(0.1f, 0.5f, 0.25f);

    private void Awake()
    {
        panel = GetComponent<PanelRenderer>();
        panel.RegisterUIReloadCallback(OnUIReload);
    }

    private void Start()
    {
        // Refresh the lock label the moment any puzzle is solved.
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

    private void OnUIReload(PanelRenderer panelRenderer, VisualElement root, int version)
    {
        if (floor1Button != null) floor1Button.clicked -= OnFloor1;
        if (floor2Button != null) floor2Button.clicked -= OnFloor2;
        if (floorB1Button != null) floorB1Button.clicked -= OnFloorB1;
        if (closeButton != null) closeButton.clicked -= Close;

        floor1Button = root.Q<Button>("Floor1Button");
        floor2Button = root.Q<Button>("Floor2Button");
        floorB1Button = root.Q<Button>("FloorB1Button");
        closeButton = root.Q<Button>("CloseButton");
        b1LockLabel = root.Q<Label>("B1LockLabel");
        statusText = root.Q<Label>("StatusText");

        string missing = "";
        if (floor1Button == null) missing += " Floor1Button";
        if (floor2Button == null) missing += " Floor2Button";
        if (floorB1Button == null) missing += " FloorB1Button";
        if (closeButton == null) missing += " CloseButton";
        if (b1LockLabel == null) missing += " B1LockLabel";
        if (statusText == null) missing += " StatusText";
        if (missing.Length > 0)
        {
            Debug.LogError("ElevatorPanelController: ElevatorUI.uxml is missing:" + missing, this);
            return;
        }

        floor1Button.clicked += OnFloor1;
        floor2Button.clicked += OnFloor2;
        floorB1Button.clicked += OnFloorB1;
        closeButton.clicked += Close;
        Refresh();
    }

    private void Refresh()
    {
        if (b1LockLabel == null || statusText == null) return;
        bool unlocked = downExit != null && downExit.IsUnlocked;
        b1LockLabel.text = unlocked ? unlockedLabel : lockedLabel;
        b1LockLabel.style.color = unlocked ? Good : Bad;
        SetStatus("Select a floor.", Normal);
    }

    private void OnFloor1() => SetStatus(floor1Message, Normal);

    private void OnFloor2() => SetStatus(currentFloorMessage, Normal);

    private void OnFloorB1()
    {
        if (downExit == null)
        {
            Debug.LogError("ElevatorPanelController: no SegmentExit linked for B1.", this);
            return;
        }
        if (downExit.CanUse(out string message))
        {
            SetStatus("Going down...", Good);
            Close();          // give the player their controls and cursor back first
            downExit.Go();
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