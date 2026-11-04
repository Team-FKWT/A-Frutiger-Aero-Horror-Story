using UnityEngine;

/// Sets how many pills the player has in this segment and shows the count on
/// screen. Put one on the GameManager in each playable segment scene. The number
/// is set here, not on ProgressionState, because the ProgressionState from the
/// menu is the one that stays alive when the player starts from a build.
public class SegmentPills : MonoBehaviour
{
    [Tooltip("How many pills the player has when this segment starts or reloads.")]
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
        state.SetPills(startingPills);
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
