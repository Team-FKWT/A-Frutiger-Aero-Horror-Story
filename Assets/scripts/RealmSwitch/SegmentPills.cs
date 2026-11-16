using UnityEngine;

/// Shows the player's pill count on screen. The count itself lives in ProgressionState,
/// which survives scene loads, so pills carry over from segment to segment. Pills are
/// gained by picking things up (e.g. a PuzzlePickup with Pills Given set).
/// Put one on the GameManager in each playable segment scene.
public class SegmentPills : MonoBehaviour
{
    [Tooltip("Testing only: set the pill count to Starting Pills whenever this scene loads. Leave off for normal play, so pills carry over.")]
    [SerializeField] private bool resetPillsOnStart = false;
    [Tooltip("Used only when Reset Pills On Start is ticked.")]
    [SerializeField, Min(0)] private int startingPills = 5;

    private const float LabelWidth = 200f, LabelHeight = 44f, Margin = 24f;

    private ProgressionState state;
    private GUIStyle labelStyle;

    private void Start()
    {
        state = ProgressionState.Instance;
        if (state == null)
        {
            Debug.LogError("SegmentPills needs a ProgressionState in the scene.");
            enabled = false;
            return;
        }
        if (resetPillsOnStart) state.SetPills(startingPills);
    }

    // Placeholder display in the bottom right corner until the real HUD is built.
    private void OnGUI()
    {
        if (state == null) return;
        if (labelStyle == null)
        {
            labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 28,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleRight
            };
            labelStyle.normal.textColor = Color.white;
        }

        Rect area = new Rect(
            Screen.width - LabelWidth - Margin,
            Screen.height - LabelHeight - Margin,
            LabelWidth,
            LabelHeight);
        GUI.Label(area, "Pills: " + state.Pills, labelStyle);
    }
}