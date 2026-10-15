using UnityEngine;

/// Small helper that toggles two alternate dressings of a test room when the
/// player presses a key. Uses ProgressionState.TrySwitch() if available so the
/// same rules (switch points, puzzles) are respected. Player world position
/// and rotation are preserved during the swap so the player does not get
/// teleported or disabled if the dressings are parent objects.

public class SwitchToggle : MonoBehaviour
{
    [Tooltip("Root GameObject for the Reality dressing (enable when in Reality)")]
    [SerializeField] private GameObject realityDressing;

    [Tooltip("Root GameObject for the Dream dressing (enable when in Dream)")]
    [SerializeField] private GameObject dreamDressing;

    [Tooltip("Key used to toggle realms locally if ProgressionState is not present")]
    [SerializeField] private KeyCode toggleKey = KeyCode.Q;

    private void Awake()
    {
        // Initialize active states according to ProgressionState if present,
        // otherwise default to reality active.
        if (ProgressionState.Instance != null)
        {
            ApplyRealm(ProgressionState.Instance.CurrentRealm);
        }
        else
        {
            SetActiveState(true);
        }
    }

    private void OnEnable()
    {
        if (ProgressionState.Instance != null)
            ProgressionState.Instance.RealmChanged += OnRealmChanged;
    }

    private void OnDisable()
    {
        if (ProgressionState.Instance != null)
            ProgressionState.Instance.RealmChanged -= OnRealmChanged;
    }

    private void Update()
    {
        if (!Input.GetKeyDown(toggleKey)) return;

        // Prefer ProgressionState rules if available.
        if (ProgressionState.Instance != null)
        {
            ProgressionState.Instance.TrySwitch();
            return;
        }

        // Otherwise toggle locally.
        ToggleLocalRealm();
    }

    private void OnRealmChanged(Realm newRealm) => ApplyRealm(newRealm);

    private void ApplyRealm(Realm realm)
    {
        SetActiveState(realm == Realm.Reality);
    }

    private void SetActiveState(bool realityActive)
    {
        // Preserve player world transform while toggling so the player isn't
        // moved or deactivated if they are parented under a dressing root.
        var player = GameObject.FindWithTag("Player");
        Vector3 savedPos = Vector3.zero;
        Quaternion savedRot = Quaternion.identity;
        Transform prevParent = null;

        if (player != null)
        {
            savedPos = player.transform.position;
            savedRot = player.transform.rotation;
            prevParent = player.transform.parent;
            // Temporarily unparent so SetActive on dressing roots won't disable player.
            player.transform.SetParent(null, true);
        }

        if (realityDressing != null) realityDressing.SetActive(realityActive);
        if (dreamDressing != null) dreamDressing.SetActive(!realityActive);

        if (player != null)
        {
            player.transform.position = savedPos;
            player.transform.rotation = savedRot;
            // Do not reparent back to a dressing root that may be inactive.
            if (prevParent != null && prevParent.gameObject.activeInHierarchy &&
                prevParent != (realityDressing != null ? realityDressing.transform : null) &&
                prevParent != (dreamDressing != null ? dreamDressing.transform : null))
            {
                player.transform.SetParent(prevParent, true);
            }
        }
    }

    private void ToggleLocalRealm()
    {
        bool realityActive = true;
        if (realityDressing != null) realityActive = realityDressing.activeSelf;
        ApplyRealm(realityActive ? Realm.Dream : Realm.Reality);
    }
}
