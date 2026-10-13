using UnityEngine;
using UnityEngine.UIElements;

public class IntroMenuController: MonoBehaviour
{
    [SerializeField]
    public VisualTreeAsset chapterButtonTemplate;

    [SerializeField] public Texture2D ch1icon;
    [SerializeField] public Texture2D ch2icon;
    [SerializeField] public Texture2D ch3icon;
    
    private VisualElement centredDiv;

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
    }
}