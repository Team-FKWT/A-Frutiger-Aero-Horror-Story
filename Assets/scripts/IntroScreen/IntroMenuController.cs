using UnityEngine;
using UnityEngine.UIElements;

public class IntroMenuController: MonoBehaviour
{
    // References to assets for chapter selection buttons
    [SerializeField] public VisualTreeAsset chapterButtonTemplate;
    [SerializeField] public Texture2D ch1icon;
    [SerializeField] public Texture2D ch2icon;
    [SerializeField] public Texture2D ch3icon;
    
    // References to other UI elements
    private VisualElement centredDiv;
    private Button quitButton;
    private Button creditsButton;
    
    // Reference to credits screen
    [SerializeField] public PanelRenderer creditsScreenRenderer;

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
        // get div that holds all chapter buttons
        centredDiv = root.Q<VisualElement>("CentredDiv");
        
        // Get currently unlocked chapters from global state script and create a button for each
        for (int i = 1; i <= ProgressionState.Instance.CurrentSegment; i++)
        {
            // Instantiate template button
            var buttonContainer = chapterButtonTemplate.Instantiate();
            // Get root element of button
            var chapterButtonElement = buttonContainer.Q<ChapterButtonElement>();
            // Add it to our centre div
            centredDiv.Add(chapterButtonElement);
            // Set up button
            switch (i)
            {
                case 1:
                    chapterButtonElement.Init(ch1icon, i);
                    break;
                case 2:
                    chapterButtonElement.Init(ch2icon, i);
                    break;
                case 3:
                    chapterButtonElement.Init(ch3icon, i);
                    break;
            }
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