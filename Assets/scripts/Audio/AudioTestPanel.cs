using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

// Only placed in AudioTestClinic. F8 switches between gameplay and audio controls.
public class AudioTestPanel : MonoBehaviour
{
    [SerializeField] private AudioEngine engine;
    [SerializeField] private MusicDirector music;
    [SerializeField] private AudioCueEmitter spatialPreview;
    [SerializeField] private AudioCueEmitter interactionPreview;
    [SerializeField] private movementController player;
    private mouseController playerLook;
    private UniversalAudioTrigger footsteps;
    private bool editing;
    private bool restoreMovement;
    private bool restoreLook;
    private CursorLockMode previousCursorLock;
    private bool previousCursorVisible;
    private bool overrideDetection;
    private float testDetection;

    private void Awake()
    {
        if (player != null)
        {
            playerLook = player.GetComponent<mouseController>();
            footsteps = player.GetComponent<UniversalAudioTrigger>();
        }
    }

    private IEnumerator Start()
    {
        // Existing feedback scripts create their sources in Start. Route them locally
        // so the test scene's master and SFX controls also cover existing effects.
        yield return null;
        foreach (AudioSource source in FindObjectsByType<AudioSource>(FindObjectsInactive.Include))
        {
            bool belongsToScene = source.gameObject.scene == gameObject.scene;
            bool belongsToProgression = ProgressionState.Instance != null &&
                source.gameObject == ProgressionState.Instance.gameObject;
            if ((belongsToScene || belongsToProgression) && source.outputAudioMixerGroup == null)
                source.outputAudioMixerGroup = engine.EffectsGroup;
        }
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.f8Key.wasPressedThisFrame)
            SetEditing(!editing);
    }

    private void LateUpdate()
    {
        // CharacterController retains its last velocity when movement is disabled.
        if (footsteps != null && player != null) footsteps.enabled = player.isActiveAndEnabled;
    }

    private void SetEditing(bool open)
    {
        if (open == editing) return;
        editing = open;
        if (editing)
        {
            bool wasPaused = player != null && player.pausePanel != null && player.pausePanel.activeSelf;
            restoreMovement = player != null && (wasPaused || player.enabled);
            restoreLook = playerLook != null && (wasPaused || playerLook.enabled);
            previousCursorLock = wasPaused ? CursorLockMode.Locked : Cursor.lockState;
            previousCursorVisible = !wasPaused && Cursor.visible;
            if (wasPaused) player.pausePanel.SetActive(false);
            if (player != null) player.enabled = false;
            if (playerLook != null) playerLook.enabled = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            if (player != null) player.enabled = restoreMovement;
            if (playerLook != null) playerLook.enabled = restoreLook;
            Cursor.lockState = previousCursorLock;
            Cursor.visible = previousCursorVisible;
        }
    }

    private void OnGUI()
    {
        if (engine == null || music == null) return;
        Matrix4x4 previousMatrix = GUI.matrix;
        float uiScale = Mathf.Max(1f, Screen.height / 720f);
        GUI.matrix = Matrix4x4.Scale(new Vector3(uiScale, uiScale, 1f));
        if (!editing)
        {
            GUI.Box(new Rect(16f, 16f, 250f, 28f), "F8: open audio controls");
            GUI.matrix = previousMatrix;
            return;
        }

        GUILayout.BeginArea(new Rect(16f, 16f, 330f, 390f), GUI.skin.box);
        GUILayout.Label("AUDIO TEST CLINIC");
        GUILayout.Label("F8 returns to gameplay.");
        Realm realm = ProgressionState.Instance != null ? ProgressionState.Instance.CurrentRealm : Realm.Reality;
        GUILayout.Label($"Realm: {realm}   Detection: {music.Detection:F0}%   Speed: {music.PlaybackSpeed:F2}x");
        float master = Slider("Master", engine.MasterVolume);
        float musicVolume = Slider("Music", engine.MusicVolume);
        float effects = Slider("SFX / interactions", engine.SfxVolume);
        if (master != engine.MasterVolume) engine.SetMasterVolume(master);
        if (musicVolume != engine.MusicVolume) engine.SetMusicVolume(musicVolume);
        if (effects != engine.SfxVolume) engine.SetSfxVolume(effects);
        music.RealityVolume = Slider("Reality mix", music.RealityVolume);
        music.DreamVolume = Slider("Dream mix", music.DreamVolume);

        overrideDetection = GUILayout.Toggle(overrideDetection, "Preview detection (either realm)");
        if (overrideDetection)
            testDetection = Slider("Detection", testDetection / 100f) * 100f;
        music.DetectionOverride = overrideDetection ? testDetection : (float?)null;

        if (GUILayout.Button("Preview positional door sound")) spatialPreview.Play();
        if (GUILayout.Button("Preview puzzle confirmation")) interactionPreview.Play();
        GUILayout.Label("Return to gameplay to move or switch realms.");
        if (GUILayout.Button("Return to gameplay")) SetEditing(false);
        GUILayout.EndArea();
        GUI.matrix = previousMatrix;
    }

    private static float Slider(string label, float value)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label, GUILayout.Width(140f));
        float result = GUILayout.HorizontalSlider(value, 0f, 1f, GUILayout.Width(125f));
        GUILayout.Label($"{result:P0}", GUILayout.Width(42f));
        GUILayout.EndHorizontal();
        return result;
    }

    private void OnDisable()
    {
        SetEditing(false);
        if (footsteps != null && player != null) footsteps.enabled = player.isActiveAndEnabled;
        if (music != null) music.DetectionOverride = null;
    }
}
