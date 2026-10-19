using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class DesktopWindowElement: VisualElement
{
    // Contents of window to display.
    [SerializeField] public VisualTreeAsset windowContents;
    
    // Components of the window.
    private Label windowTitle => this.Q<Label>("WindowTitle");
    private Button closeButton => this.Q<Button>("CloseButton");
    private VisualElement contentArea => this.Q<VisualElement>("WindowContents");
    // Default window height: 1100x500

    public void Init(string title, VisualTreeAsset contents)
    {
        windowTitle.text = title;
        // Add provided contents into window
        windowContents = contents;
        var contentContainer = contents.Instantiate();
        contentContainer.style.width = Length.Percent(100);
        contentContainer.style.height = Length.Percent(100);
        contentArea.Add(contentContainer);
        // Close button functionality
        closeButton.clicked += () =>
        {
            this.Clear();
        };
    }
}