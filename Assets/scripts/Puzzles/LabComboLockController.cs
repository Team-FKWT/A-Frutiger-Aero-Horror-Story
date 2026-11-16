using UnityEngine;
using UnityEngine.UIElements;

/// Three-dial combo lock puzzle for the lab. Uses the team's ComboLockUI screen, but is a
/// separate script from ComboLockUIController, so changes here never affect that one.
/// The right combination marks the puzzle solved in ProgressionState, optionally triggers
/// a PuzzleActor, shows a message, closes the view and hides the lock. If the puzzle is
/// already solved when the scene loads (e.g. after a stealth reset), the lock starts hidden.
/// Put this on the object with the PanelRenderer (the child of the lock body).
public class LabComboLockController : MonoBehaviour
{
    [Header("Puzzle")]
    [Tooltip("Puzzle ID marked solved on the right combination, e.g. S3_Lock.")]
    [SerializeField] private string puzzleId = "S3_Lock";
    [Tooltip("The combination as three digits from 1 to 9, e.g. 472.")]
    [SerializeField] private string answer = "472";
    [Tooltip("Optional: triggered when solved, e.g. a DoorController.")]
    [SerializeField] private PuzzleActor puzzleActor;

    [Header("Links")]
    [Tooltip("The InteractiveViewController on the lock body, used to close the view.")]
    [SerializeField] private MonoBehaviour viewController;
    [Tooltip("Hidden once solved, e.g. the lock body, so the player can't open it again.")]
    [SerializeField] private GameObject hideWhenSolved;

    [Header("Messages")]
    [SerializeField, TextArea]
    private string solvedMessage =
        "Click. The sample drawer unlocks.\nYou feel like you need to take another pill.";
    [SerializeField] private string wrongMessage = "The lock doesn't budge.";

    private readonly Label[] dials = new Label[3];

    private void Awake() => GetComponent<PanelRenderer>().RegisterUIReloadCallback(OnUIReload);

    private void OnDestroy() => GetComponent<PanelRenderer>().UnregisterUIReloadCallback(OnUIReload);

    private void Start()
    {
        // Already opened before a reload: restore the result quietly.
        if (IsSolved())
        {
            if (puzzleActor != null) puzzleActor.OnSolve();
            if (hideWhenSolved != null) hideWhenSolved.SetActive(false);
        }
    }

    private void OnUIReload(PanelRenderer panelRenderer, VisualElement root, int version)
    {
        string missing = "";
        for (int i = 0; i < 3; i++)
        {
            int index = i; // copy for the button actions below
            dials[i] = root.Q<Label>($"Dial{i + 1}");
            Button up = root.Q<Button>($"Dial{i + 1}Up");
            Button down = root.Q<Button>($"Dial{i + 1}Down");
            if (dials[i] == null || up == null || down == null) { missing += $" Dial{i + 1}"; continue; }
            // Assigning a new Clickable replaces any old action, so reloads never double up.
            up.clickable = new Clickable(() => Step(index, +1));
            down.clickable = new Clickable(() => Step(index, -1));
        }
        Button shackle = root.Q<Button>("LockShackle");
        if (shackle == null) missing += " LockShackle";
        else shackle.clickable = new Clickable(TryOpen);

        if (missing.Length > 0)
            Debug.LogError("LabComboLockController: ComboLockUI.uxml is missing:" + missing, this);
    }

    // Turns one dial up or down, wrapping between 1 and 9.
    private void Step(int index, int direction)
    {
        if (dials[index] == null) return;
        int value = int.TryParse(dials[index].text, out int v) ? v : 1;
        value = (value - 1 + direction + 9) % 9 + 1;
        dials[index].text = value.ToString();
    }

    private void TryOpen()
    {
        if (IsSolved()) return;
        string combo = "";
        foreach (Label dial in dials) combo += dial != null ? dial.text : "?";

        if (combo == answer)
        {
            if (ProgressionState.Instance != null) ProgressionState.Instance.MarkPuzzleSolved(puzzleId);
            if (puzzleActor != null) puzzleActor.OnSolve();
            Close();
            ScreenMessage.Show(solvedMessage);
            if (hideWhenSolved != null) hideWhenSolved.SetActive(false);
        }
        else
        {
            ScreenMessage.Show(wrongMessage, 2f);
        }
    }

    private bool IsSolved() =>
        ProgressionState.Instance != null && ProgressionState.Instance.IsPuzzleSolved(puzzleId);

    private void Close()
    {
        if (viewController != null)
            viewController.SendMessage("DisengageInteract", SendMessageOptions.DontRequireReceiver);
    }
}