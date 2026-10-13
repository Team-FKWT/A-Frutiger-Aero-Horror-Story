using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[UxmlElement]
public partial class ChapterButtonElement: VisualElement
{
    // References to ChapterButton elements
    private VisualElement ChapterIcon => this.Q("ChapterIcon");
    private Label ChapterLabel => this.Q<Label>("ChapterLabel");
    public Button ChapterButton => this.Q<Button>("ChapterButton");

    // Sets button properties when instantiated.
    public void Init(Texture2D icon, int chapter)
    {
        ChapterIcon.style.backgroundImage = icon;
        ChapterLabel.text = "Chapter " + chapter;
        // TODO set behaviour to load correct chapter
        ChapterButton.clicked += () =>
        {
            SceneManager.LoadScene("SampleScene");
        };
    }

    public ChapterButtonElement() {}
}