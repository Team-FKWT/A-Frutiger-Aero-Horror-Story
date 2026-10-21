using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

public class PauseMenuController : MonoBehaviour
{
    private Button exitButton;
    private Button resumeButton;
    
    private movementController movementController;
    private mouseController mouseController;

    [SerializeField] private string IntroScene;
    
    void Awake()
    {
        GetComponent<PanelRenderer>().RegisterUIReloadCallback(OnUIReload);
    }

    void OnDestroy()
    {
        GetComponent<PanelRenderer>().UnregisterUIReloadCallback(OnUIReload);
    }

    private void OnUIReload(PanelRenderer panelRenderer, VisualElement root, int version)
    {
        // TODO take over movement and mouse control on every activation
        movementController = gameObject.GetComponentInParent<movementController>();
        mouseController = gameObject.GetComponentInParent<mouseController>();
        movementController.enabled = false;
        mouseController.enabled = false;
        // Enable cursor
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        
        exitButton = root.Q<Button>("ExitButton");
        resumeButton = root.Q<Button>("ResumeButton");
        // When exit is clicked, load IntroMenu scene
        exitButton.clicked += () =>
        {
            // Check that the specified scene exists
            if (Application.CanStreamedLevelBeLoaded(IntroScene))
                SceneManager.LoadScene(IntroScene);
            else
                Debug.Log("Couldn't load intro menu scene (" + IntroScene + ")");
        };
        // When resume button is clicked, dismiss pause menu
        resumeButton.clicked += () =>
        {
            // Relinquish movement and mouse control
            movementController.enabled = true;
            mouseController.enabled = true;
            // Capture cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            panelRenderer.enabled = false;
        };
    }
}
