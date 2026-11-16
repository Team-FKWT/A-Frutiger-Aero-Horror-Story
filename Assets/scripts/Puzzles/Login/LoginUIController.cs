using UnityEngine;
using UnityEngine.UIElements;

/// Login screen puzzle for an office workstation. The player types a password
/// (found elsewhere in the level); the right one marks the puzzle solved in
/// ProgressionState and shows a message that points them to their next goal.
/// Works on a computer screen (ComputerController, world-space panel) or as an
/// overlay (InteractiveViewController, screen-space panel). Put this on the
/// object with the PanelRenderer. After a correct login the screen switches to a
/// simple desktop with the success message in a notice window.
public class LoginUIController : MonoBehaviour
{
    [Header("Progression")]
    [Tooltip("Puzzle ID marked solved on a correct login, e.g. S2_Login.")]
    [SerializeField] private string puzzleId = "S2_Login";
    [Tooltip("Optional: something to trigger on success, such as a DoorController.")]
    [SerializeField] private PuzzleActor puzzleActor;

    [Header("Account")]
    [SerializeField] private string userName = "jpark";
    [Tooltip("Not case-sensitive.")]
    [SerializeField] private string password = "sunnydays";

    [Header("Messages")]
    [SerializeField, TextArea]
    private string successMessage =
        "Welcome back, J. Park.\nNotice: B1 elevator clearance is issued from the Director's office.";
    [SerializeField, TextArea]
    private string failMessage =
        "Incorrect password. Forgot it? Check your desk area.";

    [Header("Closing")]
    [Tooltip("The ComputerController or InteractiveViewController that opens this screen. The X button and Esc close it.")]
    [SerializeField] private MonoBehaviour viewController;

    private TextField passwordField;
    private Button loginButton;
    private Button closeButton;
    private Label statusText;
    private Label userLabel;
    private VisualElement loginWindow;
    private VisualElement desktop;
    private Label desktopMessage;
    private Button desktopCloseButton;
    private bool viewOpen;

    private void Awake() => GetComponent<PanelRenderer>().RegisterUIReloadCallback(OnUIReload);

    private void OnDestroy()
    {
        GetComponent<PanelRenderer>().UnregisterUIReloadCallback(OnUIReload);
        TextInputGuard.SetTyping(false);
    }

    private void OnUIReload(PanelRenderer panelRenderer, VisualElement root, int version)
    {
        // Unhook the old elements so a reload never registers anything twice.
        if (loginButton != null) loginButton.clicked -= TryLogin;
        if (closeButton != null) closeButton.clicked -= Close;
        if (desktopCloseButton != null) desktopCloseButton.clicked -= Close;
        if (passwordField != null)
        {
            passwordField.UnregisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
            passwordField.UnregisterCallback<FocusInEvent>(OnFocusIn);
            passwordField.UnregisterCallback<FocusOutEvent>(OnFocusOut);
        }

        passwordField = root.Q<TextField>("PasswordField");
        loginButton = root.Q<Button>("LoginButton");
        closeButton = root.Q<Button>("CloseButton");
        statusText = root.Q<Label>("StatusText");
        userLabel = root.Q<Label>("UserName");
        loginWindow = root.Q<VisualElement>("LoginWindow");
        desktop = root.Q<VisualElement>("Desktop");
        desktopMessage = root.Q<Label>("DesktopMessage");
        desktopCloseButton = root.Q<Button>("DesktopCloseButton");

        // The login parts are required. The desktop parts are optional: without them, the
        // success message just shows under the Log In button.
        string missing = "";
        if (passwordField == null) missing += " PasswordField";
        if (loginButton == null) missing += " LoginButton";
        if (closeButton == null) missing += " CloseButton";
        if (statusText == null) missing += " StatusText";
        if (userLabel == null) missing += " UserName";
        if (missing.Length > 0)
        {
            Debug.LogError("LoginUIController: LoginUI.uxml is missing:" + missing, this);
            return;
        }
        string missingDesktop = "";
        if (loginWindow == null) missingDesktop += " LoginWindow";
        if (desktop == null) missingDesktop += " Desktop";
        if (desktopMessage == null) missingDesktop += " DesktopMessage";
        if (desktopCloseButton == null) missingDesktop += " DesktopCloseButton";
        if (missingDesktop.Length > 0)
            Debug.LogWarning("LoginUIController: no desktop view, missing:" + missingDesktop, this);

        userLabel.text = userName;
        passwordField.isPasswordField = true;
        passwordField.maskChar = '*';
        loginButton.clicked += TryLogin;
        closeButton.clicked += Close;
        if (desktopCloseButton != null) desktopCloseButton.clicked += Close;
        passwordField.RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
        // While the box has focus, game shortcuts (M, Q, E, debug keys) stand down.
        passwordField.RegisterCallback<FocusInEvent>(OnFocusIn);
        passwordField.RegisterCallback<FocusOutEvent>(OnFocusOut);

        if (IsSolved()) ShowSolved();
    }

    private void OnFocusIn(FocusInEvent evt) => TextInputGuard.SetTyping(true);

    private void OnFocusOut(FocusOutEvent evt) => TextInputGuard.SetTyping(false);

    // Sent by the ComputerController when the player sits down at the computer.
    private void OnViewOpened()
    {
        viewOpen = true;
        if (passwordField == null || IsSolved()) return;
        // Wait a moment before focusing, so the E that opened the computer isn't typed into the box.
        passwordField.schedule.Execute(() =>
        {
            if (!viewOpen || IsSolved()) return; // the player already left
            passwordField.value = "";
            passwordField.Focus();
        }).StartingIn(150);
    }

    // Sent by the ComputerController when the player leaves, so walking keys don't type into the box.
    private void OnViewClosed()
    {
        viewOpen = false;
        if (passwordField != null) passwordField.Blur();
        TextInputGuard.SetTyping(false);
    }

    private void OnKeyDown(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
        {
            TryLogin();
            evt.StopPropagation();
        }
        else if (evt.keyCode == KeyCode.Escape)
        {
            Close();
            evt.StopPropagation();
        }
    }

    private bool IsSolved() =>
        ProgressionState.Instance != null && ProgressionState.Instance.IsPuzzleSolved(puzzleId);

    private void TryLogin()
    {
        if (IsSolved()) return;

        string typed = passwordField.value == null ? "" : passwordField.value.Trim();
        if (string.Equals(typed, password, System.StringComparison.OrdinalIgnoreCase))
        {
            if (ProgressionState.Instance != null) ProgressionState.Instance.MarkPuzzleSolved(puzzleId);
            if (puzzleActor != null) puzzleActor.OnSolve();
            ShowSolved();
        }
        else
        {
            statusText.text = failMessage;
            statusText.style.color = new Color(0.75f, 0.2f, 0.15f);
            passwordField.value = "";
            passwordField.Focus();
        }
    }

    // Swaps the login window for the desktop, with the success message in a notice window.
    private void ShowSolved()
    {
        statusText.text = successMessage;
        statusText.style.color = new Color(0.1f, 0.5f, 0.25f);
        if (desktopMessage != null && loginWindow != null && desktop != null)
        {
            desktopMessage.text = successMessage;
            loginWindow.style.display = DisplayStyle.None;
            desktop.style.display = DisplayStyle.Flex;
        }
        passwordField.value = "";
        // Give up focus first, so E closes the computer again once logged in.
        passwordField.Blur();
        TextInputGuard.SetTyping(false);
        passwordField.SetEnabled(false);
        loginButton.SetEnabled(false);
    }

    private void Close()
    {
        if (passwordField != null) passwordField.Blur();
        TextInputGuard.SetTyping(false);
        if (viewController != null)
            viewController.SendMessage("DisengageInteract", SendMessageOptions.DontRequireReceiver);
    }
}