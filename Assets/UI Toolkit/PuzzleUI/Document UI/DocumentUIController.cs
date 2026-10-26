using UnityEngine;
using UnityEngine.UIElements;

public class DocumentUIController : MonoBehaviour
{
    // References to UI elements
    private Button backButton;
    private Button forwardButton;
    private Image documentView;
    
    // Document pages to show
    public Texture2D[] pages;
    private int currentPage = 0;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
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
        // Get references to UI elements
        documentView = root.Q<Image>("DocumentView");
        backButton = root.Q<Button>("backButton");
        forwardButton = root.Q<Button>("forwardButton");
        
        // By default, back button should be hidden at first page anyways
        backButton.style.visibility = Visibility.Hidden;
        // If there's only one page, hide the other button too
        if (pages.Length - currentPage == 1)
            forwardButton.style.visibility = Visibility.Hidden;
        
        documentView.image = pages[currentPage];

        backButton.clicked += () =>
        {
            currentPage--;
            // Hide back button if no pages before
            if (currentPage == 0)
                backButton.style.visibility = Visibility.Hidden;
            // Re-enable forward button
            forwardButton.style.visibility = Visibility.Visible;
            // Refresh document view
            documentView.image = pages[currentPage];
        };

        forwardButton.clicked += () =>
        {
            currentPage++;
            // Re-enable back button
            backButton.style.visibility = Visibility.Visible;
            // Hide forward button if no pages before
            if (currentPage == pages.Length - 1)
                forwardButton.style.visibility = Visibility.Hidden;
            // Refresh document view
            documentView.image = pages[currentPage];
        };
    }
}
