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
    private int clicked;

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
            // Set up button and set callback function on click
            chapterButtonElement.Init(ch1icon, i);
            // chapterButtonElement.RegisterCallback<ClickEvent>(LoadChapter);
        }
        
        // chapterButton1.RegisterCallback<ClickEvent>(PrintClickMsg);
    }

    private void LoadChapter(ClickEvent e)
    {
        // TODO add chapter load function
    }
    
    // TODO remove testing method
    private void PrintClickMsg(ClickEvent e)
    {
        clicked++;
        Debug.Log("clicked: " + clicked + " times");
    }
    

}