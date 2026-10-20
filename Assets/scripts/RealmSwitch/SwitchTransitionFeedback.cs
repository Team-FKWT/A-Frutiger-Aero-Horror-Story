using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// Shows a short Windows XP-style dialog while the player crosses into the new realm.
/// Put this on the same GameObject as ProgressionState. It builds its own overlay,
/// so the only optional scene setup is assigning custom transition sounds.
public class SwitchTransitionFeedback : MonoBehaviour
{
    [Header("Transition")]
    [Tooltip("How long the transition dialog remains visible.")]
    [SerializeField, Range(0.25f, 3f)] private float duration = 1.1f;
    [Tooltip("The maximum opacity of the blue screen wash behind the dialog.")]
    [SerializeField, Range(0f, 1f)] private float screenWashOpacity = 0.35f;

    [Header("Audio")]
    [Tooltip("Programmer-assigned sound for switching into the Dream realm. MP3 and other Unity-supported audio files are accepted.")]
    [SerializeField] private AudioClip dreamTransitionSound;
    [Tooltip("Programmer-assigned sound for switching back to Reality. MP3 and other Unity-supported audio files are accepted.")]
    [SerializeField] private AudioClip realityTransitionSound;
    [SerializeField, Range(0f, 1f)] private float volume = 0.65f;

    private ProgressionState state;
    private CanvasGroup overlay;
    private Image progressFill;
    private Text statusText;
    private AudioSource audioSource;
    private Coroutine transitionRoutine;

    private void Start()
    {
        state = ProgressionState.Instance;
        if (state == null)
        {
            Debug.LogError("SwitchTransitionFeedback needs a ProgressionState in the scene.");
            enabled = false;
            return;
        }

        BuildOverlay();
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        state.RealmChanged += OnRealmChanged;
    }

    private void OnDestroy()
    {
        if (state != null) state.RealmChanged -= OnRealmChanged;
    }

    private void OnRealmChanged(Realm newRealm)
    {
        if (transitionRoutine != null) StopCoroutine(transitionRoutine);
        transitionRoutine = StartCoroutine(PlayTransition(newRealm));

        AudioClip customSound = newRealm == Realm.Dream ? dreamTransitionSound : realityTransitionSound;
        audioSource.Stop();
        if (customSound != null)
        {
            audioSource.PlayOneShot(customSound, volume);
        }
        else
        {
            Debug.LogWarning("SwitchTransitionFeedback has no custom audio clip assigned for the " + newRealm + " transition.");
        }
    }

    private IEnumerator PlayTransition(Realm newRealm)
    {
        statusText.text = newRealm == Realm.Dream ? "Switching to Dream..." : "Returning to Reality...";
        overlay.gameObject.SetActive(true);
        progressFill.fillAmount = 0f;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            progressFill.fillAmount = Mathf.SmoothStep(0f, 1f, progress);
            overlay.alpha = Mathf.Clamp01(Mathf.Min(progress * 8f, (1f - progress) * 8f));
            yield return null;
        }

        progressFill.fillAmount = 1f;
        overlay.alpha = 0f;
        overlay.gameObject.SetActive(false);
        transitionRoutine = null;
    }

    private void BuildOverlay()
    {
        GameObject canvasObject = new GameObject("SwitchTransitionOverlay");
        canvasObject.transform.SetParent(transform);

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 101;
        canvasObject.AddComponent<GraphicRaycaster>();

        overlay = canvasObject.AddComponent<CanvasGroup>();
        overlay.alpha = 0f;
        overlay.blocksRaycasts = false;
        overlay.interactable = false;

        Image wash = CreateImage("BlueScreenWash", canvasObject.transform, new Color(0.05f, 0.28f, 0.72f, screenWashOpacity));
        Stretch(wash.rectTransform);
        wash.raycastTarget = false;

        Image dialog = CreateImage("WindowsStyleDialog", canvasObject.transform, new Color(0.93f, 0.93f, 0.93f, 1f));
        SetRect(dialog.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-215f, -85f), new Vector2(215f, 85f));

        Image titleBar = CreateImage("TitleBar", dialog.transform, new Color(0.08f, 0.33f, 0.72f, 1f));
        SetRect(titleBar.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(8f, -32f), new Vector2(-8f, -8f));
        Text title = CreateText("Title", titleBar.transform, "Realm Switch", 15, Color.white, TextAnchor.MiddleLeft);
        Stretch(title.rectTransform, 10f, 0f, -10f, 0f);

        Text message = CreateText("Message", dialog.transform, "Please wait while Windows changes your realm.", 14, new Color(0.1f, 0.1f, 0.1f), TextAnchor.MiddleLeft);
        SetRect(message.rectTransform, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(25f, -15f), new Vector2(-25f, 25f));

        statusText = CreateText("Status", dialog.transform, "Switching...", 12, new Color(0.2f, 0.2f, 0.2f), TextAnchor.MiddleLeft);
        SetRect(statusText.rectTransform, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(25f, -42f), new Vector2(-25f, -12f));

        Image progressBackground = CreateImage("ProgressBackground", dialog.transform, new Color(0.72f, 0.72f, 0.72f, 1f));
        SetRect(progressBackground.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(25f, 28f), new Vector2(-25f, 48f));
        progressFill = CreateImage("ProgressFill", progressBackground.transform, new Color(0.18f, 0.48f, 0.9f, 1f));
        progressFill.type = Image.Type.Filled;
        progressFill.fillMethod = Image.FillMethod.Horizontal;
        progressFill.fillOrigin = 0;
        Stretch(progressFill.rectTransform);
        progressFill.fillAmount = 0f;

        Button closeButton = dialog.gameObject.AddComponent<Button>();
        closeButton.transition = Selectable.Transition.None;
        closeButton.interactable = false;

        overlay.gameObject.SetActive(false);
    }

    private static Image CreateImage(string name, Transform parent, Color color)
    {
        GameObject imageObject = new GameObject(name);
        imageObject.transform.SetParent(parent, false);
        Image image = imageObject.AddComponent<Image>();
        image.color = color;
        return image;
    }

    private static Text CreateText(string name, Transform parent, string content, int size, Color color, TextAnchor alignment)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);
        Text text = textObject.AddComponent<Text>();
        text.text = content;
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = size;
        text.color = color;
        text.alignment = alignment;
        text.raycastTarget = false;
        return text;
    }

    private static void Stretch(RectTransform rect, float left = 0f, float bottom = 0f, float right = 0f, float top = 0f)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(-right, -top);
    }

    private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
    }

}
