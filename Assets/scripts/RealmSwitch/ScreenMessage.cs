using UnityEngine;

/// Shows a short message near the bottom of the screen for a few seconds.
/// Any script can call ScreenMessage.Show("text"). It creates itself the first
/// time it's used and survives scene loads, so nothing needs to be placed in a scene.
public class ScreenMessage : MonoBehaviour
{
    private static ScreenMessage instance;
    private string text;
    private float hideAt;
    private GUIStyle style;

    /// Shows the message for the given number of seconds, replacing any message already showing.
    public static void Show(string message, float seconds = 3.5f)
    {
        if (instance == null)
        {
            GameObject go = new GameObject("ScreenMessage");
            instance = go.AddComponent<ScreenMessage>();
            DontDestroyOnLoad(go);
        }
        instance.text = message;
        instance.hideAt = Time.unscaledTime + seconds;
    }

    private void OnGUI()
    {
        if (string.IsNullOrEmpty(text) || Time.unscaledTime > hideAt) return;

        if (style == null)
        {
            style = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true,
                richText = true
            };
            style.normal.textColor = Color.white;
        }
        style.fontSize = Mathf.Max(16, Screen.height / 32);

        float width = Mathf.Min(Screen.width * 0.7f, 900f);
        float height = style.CalcHeight(new GUIContent(text), width) + 24f;
        Rect box = new Rect((Screen.width - width) / 2f, Screen.height * 0.78f - height / 2f, width, height);
        GUI.Box(box, text, style);
    }
}