using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// "Reality resists" feedback. When a switch attempt is refused, TV static
/// fades in over the screen, holds, and fades out, with a matching static hiss.
/// Put this on the same GameObject as ProgressionState. It builds its own
/// overlay and sound, so no scene setup is needed.
 
public class SwitchDeniedFeedback : MonoBehaviour
{
    [Header("Static look")]
    [Tooltip("How strong the static gets at its peak. 0 = invisible, 1 = fully covers the screen.")]
    [SerializeField, Range(0f, 1f)] private float maxStrength = 0.55f;
    [Tooltip("Tint applied to the static. White = classic grey static.")]
    [SerializeField] private Color tint = Color.white;
    [Tooltip("Width of the static texture in pixels. Lower = chunkier grain.")]
    [SerializeField, Range(64, 640)] private int grainResolution = 320;
    [Tooltip("How many times per second the static pattern changes.")]
    [SerializeField, Range(6f, 60f)] private float flickerRate = 30f;

    [Header("Timing (seconds)")]
    [SerializeField, Range(0.01f, 1f)] private float fadeIn = 0.12f;
    [SerializeField, Range(0f, 1f)] private float hold = 0.15f;
    [SerializeField, Range(0.01f, 2f)] private float fadeOut = 0.4f;

    [Header("Audio")]
    [Tooltip("Sound to play. If left empty, a static hiss matching the timing above is generated.")]
    [SerializeField] private AudioClip deniedSound;
    [SerializeField, Range(0f, 1f)] private float volume = 0.5f;

    private ProgressionState state;
    private RawImage overlay;
    private AudioSource audioSource;
    private Coroutine staticRoutine;

    private void Start()
    {
        state = ProgressionState.Instance;
        if (state == null)
        {
            Debug.LogError("SwitchDeniedFeedback needs a ProgressionState in the scene.");
            enabled = false;
            return;
        }
        BuildOverlay();
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound: same volume wherever the player stands
        if (deniedSound == null) deniedSound = MakeStaticHiss();
        state.SwitchDenied += OnSwitchDenied;
    }

    private void OnDestroy()
    {
        if (state != null) state.SwitchDenied -= OnSwitchDenied;
    }

    /// Plays the static and hiss. Runs whenever ProgressionState refuses a switch.
    private void OnSwitchDenied(SwitchBlockReason reason)
    {
        if (staticRoutine != null) StopCoroutine(staticRoutine);
        staticRoutine = StartCoroutine(PlayStatic());
        audioSource.Stop();
        audioSource.PlayOneShot(deniedSound, volume);
    }

    private IEnumerator PlayStatic()
    {
        float total = fadeIn + hold + fadeOut;
        float elapsed = 0f;
        float nextFlicker = 0f;
        overlay.enabled = true;

        while (elapsed < total)
        {
            // Jump to a random spot in the noise texture so the static "moves".
            if (elapsed >= nextFlicker)
            {
                overlay.uvRect = new Rect(Random.value, Random.value, 1f, 1f);
                nextFlicker = elapsed + 1f / flickerRate;
            }

            Color c = tint;
            c.a = Envelope(elapsed) * maxStrength;
            overlay.color = c;

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        overlay.enabled = false;
        staticRoutine = null;
    }

    // 0 to 1 strength over time: ramp up, hold, ramp down.
    private float Envelope(float t)
    {
        if (t < fadeIn) return t / fadeIn;
        if (t < fadeIn + hold) return 1f;
        return Mathf.Clamp01(1f - (t - fadeIn - hold) / fadeOut);
    }

    // Creates a full-screen static image that sits on top of everything.
    private void BuildOverlay()
    {
        GameObject canvasObject = new GameObject("SwitchDeniedOverlay");
        canvasObject.transform.SetParent(transform);
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        GameObject imageObject = new GameObject("Static");
        imageObject.transform.SetParent(canvasObject.transform, false);
        overlay = imageObject.AddComponent<RawImage>();
        overlay.texture = MakeNoiseTexture();
        overlay.raycastTarget = false; // never blocks clicks on other UI
        overlay.enabled = false;

        RectTransform rect = overlay.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    // A tiling texture of random grey pixels. Point filtering keeps the grain crisp.
    private Texture2D MakeNoiseTexture()
    {
        int width = grainResolution;
        int height = Mathf.Max(1, grainResolution * 9 / 16);
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Repeat;

        Color32[] pixels = new Color32[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            byte v = (byte)Random.Range(0, 256);
            pixels[i] = new Color32(v, v, v, 255);
        }
        texture.SetPixels32(pixels);
        texture.Apply();
        return texture;
    }

    // White noise shaped by the same fade-in, hold, fade-out as the picture.
    private AudioClip MakeStaticHiss()
    {
        const int sampleRate = 44100;
        float length = fadeIn + hold + fadeOut;
        int count = Mathf.CeilToInt(sampleRate * length);
        float[] samples = new float[count];
        for (int i = 0; i < count; i++)
        {
            float t = (float)i / sampleRate;
            samples[i] = Random.Range(-1f, 1f) * Envelope(t) * 0.5f;
        }
        AudioClip clip = AudioClip.Create("StaticHiss", count, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
