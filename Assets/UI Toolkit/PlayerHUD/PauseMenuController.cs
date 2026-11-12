using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

public class PauseMenuController : MonoBehaviour
{
    private Button exitButton;
    private Button resumeButton;
    private Button settingsButton;
    private Button controlsButton;
    private Button evidenceButton;
    
    private movementController movementController;
    private mouseController mouseController;

    [SerializeField] public GameObject loadingScreenPanel;
    [SerializeField] public string introScene;
    
    [SerializeField] public VisualTreeAsset windowTemplate;
    [SerializeField] public VisualTreeAsset controlsWindowTemplate;
    [SerializeField] public VisualTreeAsset settingsWindowTemplate;
    
    void Awake()
    {
        GetComponent<PanelRenderer>().RegisterUIReloadCallback(OnUIReload);
    }

    void OnDestroy()
    {
        GetComponent<PanelRenderer>().UnregisterUIReloadCallback(OnUIReload);
    }

    void OnEnable()
    {
        // Take over movement and mouse control
        movementController = gameObject.GetComponentInParent<movementController>();
        mouseController = gameObject.GetComponentInParent<mouseController>();
        movementController.enabled = false;
        mouseController.enabled = false;
        // Enable cursor
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    private void OnUIReload(PanelRenderer panelRenderer, VisualElement root, int version)
    {
        exitButton = root.Q<Button>("ExitButton");
        evidenceButton = root.Q<Button>("EvidenceButton");
        resumeButton = root.Q<Button>("ResumeButton");
        settingsButton = root.Q<Button>("SettingsButton");
        controlsButton =  root.Q<Button>("ControlsButton");
        
        // When exit is clicked, load IntroMenu scene
        exitButton.clicked += () =>
        {
            // Tell controller to load the scene
            loadingScreenPanel.GetComponent<LoadingScreenController>().sceneName = introScene;
            loadingScreenPanel.SetActive(true);
        };
        // When resume button is clicked, dismiss pause menu
        resumeButton.clicked += () =>
        {
            // Relinquish movement and mouse control
            gameObject.SetActive(false);
            movementController.enabled = true;
            mouseController.enabled = true;
            // Capture cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        };
        settingsButton.clicked += () =>
        {
            // Open popup window with settings
            var windowInstance = windowTemplate.Instantiate();
            var window = windowInstance.Q<DesktopWindowElement>();
            // Set size and position of window
            root.Add(window);
            window.Init("Settings", settingsWindowTemplate, Length.Percent(50), Length.Percent(40), Length.Percent(20), Length.Percent(20), true);
        };
        controlsButton.clicked += () =>
        {
            // Open popup window with settings
            var windowInstance = windowTemplate.Instantiate();
            var window = windowInstance.Q<DesktopWindowElement>();
            // Set size and position of window
            root.Add(window);
            window.Init("Controls", controlsWindowTemplate, Length.Percent(50), Length.Percent(50), Length.Percent(20), Length.Percent(20), true);
        };
    }
}
