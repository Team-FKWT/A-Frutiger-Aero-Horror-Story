using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class EvidenceLogUI : MonoBehaviour
{
    [SerializeField] private EvidenceLog log;
    [SerializeField] private movementController player;

    public bool IsOpen => isOpen;

    private mouseController playerLook;
    private bool isOpen;
    private bool restoreMovement;
    private bool restoreLook;
    private CursorLockMode previousCursorLock;
    private bool previousCursorVisible;
    private Coroutine restoreRoutine;
    private int selectedEntry;
    private Vector2 documentScroll;
    private Vector2 entryScroll;
    private GUIStyle headingStyle;
    private GUIStyle bodyStyle;
    private GUIStyle titleStyle;
    private GUIStyle entryButtonStyle;

    private void Awake()
    {
        if (player != null) playerLook = player.GetComponent<mouseController>();
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;
        if (isOpen)
        {
            if (keyboard.jKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame)
                Close();
            return;
        }

        if (keyboard.jKey.wasPressedThisFrame && !TextInputGuard.IsTyping)
            Open();
    }

    public void Open()
    {
        // Another gameplay panel owns the controls while movement is disabled.
        if (isOpen || log == null || player == null || !player.isActiveAndEnabled ||
            restoreRoutine != null || TextInputGuard.IsTyping)
            return;

        restoreMovement = player.enabled;
        restoreLook = playerLook != null && playerLook.enabled;
        previousCursorLock = restoreLook ? CursorLockMode.Locked : Cursor.lockState;
        previousCursorVisible = !restoreLook && Cursor.visible;
        player.enabled = false;
        if (playerLook != null) playerLook.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        isOpen = true;
    }

    public void Close()
    {
        if (!isOpen) return;
        isOpen = false;
        // Let the closing Escape finish before the movement script can open Pause.
        restoreRoutine = StartCoroutine(RestoreNextFrame());
    }

    private IEnumerator RestoreNextFrame()
    {
        yield return null;
        RestoreControls();
        restoreRoutine = null;
    }

    private void RestoreControls()
    {
        if (player != null) player.enabled = restoreMovement;
        if (playerLook != null) playerLook.enabled = restoreLook;
        Cursor.lockState = previousCursorLock;
        Cursor.visible = previousCursorVisible;
    }

    private void OnDisable()
    {
        if (restoreRoutine != null)
        {
            StopCoroutine(restoreRoutine);
            restoreRoutine = null;
            RestoreControls();
        }
        if (isOpen)
        {
            isOpen = false;
            RestoreControls();
        }
    }

    private void OnGUI()
    {
        if (log == null) return;
        Matrix4x4 previousMatrix = GUI.matrix;
        float scale = Mathf.Max(1f, Screen.height / 720f);
        GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
        float screenWidth = Screen.width / scale;
        float screenHeight = Screen.height / scale;

        if (!isOpen)
        {
            GUI.Box(new Rect(16f, screenHeight - 44f, 260f, 28f),
                $"J: Evidence log ({log.Entries.Count} entries)");
            GUI.matrix = previousMatrix;
            return;
        }

        InitializeStyles();
        float width = Mathf.Min(1000f, screenWidth - 32f);
        float height = Mathf.Min(620f, screenHeight - 64f);
        Rect window = new Rect((screenWidth - width) * 0.5f, (screenHeight - height) * 0.5f, width, height);
        Color previousColor = GUI.color;
        GUI.color = new Color(0.08f, 0.18f, 0.3f);
        GUI.DrawTexture(window, Texture2D.whiteTexture);
        GUI.color = new Color(0.94f, 0.96f, 0.97f);
        GUI.DrawTexture(new Rect(window.x + 3f, window.y + 3f, width - 6f, height - 6f), Texture2D.whiteTexture);
        GUI.color = new Color(0.12f, 0.34f, 0.65f);
        GUI.DrawTexture(new Rect(window.x + 3f, window.y + 3f, width - 6f, 40f), Texture2D.whiteTexture);
        GUI.color = previousColor;
        GUI.Label(new Rect(window.x + 16f, window.y + 9f, width - 150f, 28f), "Evidence log", titleStyle);
        if (GUI.Button(new Rect(window.xMax - 140f, window.y + 10f, 124f, 26f), "Close (J / Esc)"))
            Close();

        Rect contents = new Rect(window.x + 16f, window.y + 56f, width - 32f, height - 72f);
        GUILayout.BeginArea(contents);
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical(GUILayout.Width(230f));
        GUILayout.Label($"Collected evidence ({log.Entries.Count})", headingStyle);
        entryScroll = GUILayout.BeginScrollView(entryScroll);
        for (int index = 0; index < log.Entries.Count; index++)
        {
            string marker = index == selectedEntry ? "> " : "";
            if (GUILayout.Button(marker + log.Entries[index].Title, entryButtonStyle, GUILayout.MinHeight(44f)))
            {
                selectedEntry = index;
                documentScroll = Vector2.zero;
            }
        }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
        GUILayout.Space(20f);
        GUILayout.BeginVertical();
        if (log.Entries.Count > 0)
        {
            selectedEntry = Mathf.Clamp(selectedEntry, 0, log.Entries.Count - 1);
            EvidenceEntry entry = log.Entries[selectedEntry];
            GUILayout.Label(entry.Title, headingStyle);
            documentScroll = GUILayout.BeginScrollView(documentScroll);
            GUILayout.Label(entry.Description, bodyStyle);
            if (entry.Document != null)
            {
                GUILayout.Space(12f);
                float imageWidth = Mathf.Max(1f, contents.width - 286f);
                float imageHeight = imageWidth * entry.Document.height / entry.Document.width;
                Rect image = GUILayoutUtility.GetRect(imageWidth, imageHeight,
                    GUILayout.Width(imageWidth), GUILayout.Height(imageHeight));
                GUI.DrawTexture(image, entry.Document, ScaleMode.ScaleToFit, true);
            }
            GUILayout.EndScrollView();
        }
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
        GUI.matrix = previousMatrix;
    }

    private void InitializeStyles()
    {
        if (headingStyle != null) return;
        headingStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            wordWrap = true
        };
        headingStyle.normal.textColor = new Color(0.08f, 0.16f, 0.24f);
        bodyStyle = new GUIStyle(GUI.skin.label) { fontSize = 15, wordWrap = true };
        bodyStyle.normal.textColor = headingStyle.normal.textColor;
        titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 20, fontStyle = FontStyle.Bold };
        titleStyle.normal.textColor = Color.white;
        entryButtonStyle = new GUIStyle(GUI.skin.button) { fontSize = 15, wordWrap = true };
    }
}
