using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

public class IntroMenuController : MonoBehaviour
{
    // One button on the menu: which scene it loads, what it says, and its icon
    [Serializable]
    public class MenuScene
    {
        public string label = "Chapter";
        public string sceneName;
        public Texture2D icon;
    }

    // References to assets for chapter selection buttons
    [SerializeField] public VisualTreeAsset chapterButtonTemplate;
    // Every scene shown on the menu, in order (add more in the Inspector)
    [SerializeField] public MenuScene[] scenes;

    // References to other UI elements
    private VisualElement centredDiv;
    private Button quitButton;
    private Button creditsButton;

    // Reference to credits screen
    [SerializeField] public PanelRenderer creditsScreenRenderer;
    // Reference to loading screen
    [SerializeField] public GameObject loadingScreenPanel;
    // Alpha: show every scene so testers can reach them all
    [SerializeField] public bool unlockAllChapters = true;

    void Awake()
    {
        // Free the mouse when arriving back at the menu from a level
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        GetComponent<PanelRenderer>().RegisterUIReloadCallback(OnUIReload);
    }

    void OnDestroy()
    {
        GetComponent<PanelRenderer>().UnregisterUIReloadCallback(OnUIReload);
    }

    private void OnUIReload(PanelRenderer panelRenderer, VisualElement root, int version)
    {
        // get div that holds all chapter buttons
        centredDiv = root.Q<VisualElement>("CentredDiv");
        // remove old buttons so they don't double up after Credits > Back
        centredDiv.Clear();

        // Alpha shows every scene, otherwise only as many as the player has unlocked
        int count = unlockAllChapters ? scenes.Length
            : Mathf.Min(scenes.Length, ProgressionState.Instance.CurrentSegment);

        for (int i = 0; i < count; i++)
        {
            MenuScene entry = scenes[i];
            // Instantiate template button and add it to the centre div
            var buttonContainer = chapterButtonTemplate.Instantiate();
            var chapterButtonElement = buttonContainer.Q<ChapterButtonElement>();
            centredDiv.Add(chapterButtonElement);
            // Set up button, then replace "Chapter N" with this scene's label
            chapterButtonElement.Init(entry.icon, i + 1, entry.sceneName, loadingScreenPanel);
            chapterButtonElement.Q<Label>("ChapterLabel").text = entry.label;
        }

        // Set quit action for quit button
        quitButton = root.Q<Button>("QuitButton");
        quitButton.clicked += Application.Quit;

        // Set credits menu action for credits button
        creditsButton = root.Q<Button>("CreditsButton");
        creditsButton.clicked += () =>
        {
            creditsScreenRenderer.GetComponent<PanelRenderer>().enabled = true;
            panelRenderer.enabled = false;
        };
    }
}