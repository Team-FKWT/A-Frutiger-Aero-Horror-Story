using UnityEngine;

/// Makes an object exist in only one realm. In its realm it is visible and solid;
/// in the other realm its renderers, colliders and lights are turned off, so it can
/// act as a barrier in one realm and an open path in the other (or a platform that
/// only exists in one realm). The object itself stays active, so it keeps listening
/// for realm changes. Put this on the object, or on a parent to control all its children.
public class RealmDependentObject : MonoBehaviour
{
    [Tooltip("The realm this object exists in. In the other realm it is invisible and can be walked through.")]
    [SerializeField] private Realm existsIn = Realm.Dream;

    private Renderer[] renderers;
    private Collider[] colliders;
    private Light[] lights;
    private bool subscribed;

    /// The realm this object exists in.
    public Realm ExistsIn => existsIn;

    /// True if the object is currently visible and solid.
    public bool IsPresent { get; private set; }

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
        colliders = GetComponentsInChildren<Collider>(true);
        lights = GetComponentsInChildren<Light>(true);
    }

    private void OnEnable() => Subscribe();

    // Second chance in case ProgressionState was not ready during OnEnable.
    private void Start() => Subscribe();

    private void OnDisable()
    {
        if (subscribed && ProgressionState.Instance != null)
            ProgressionState.Instance.RealmChanged -= Apply;
        subscribed = false;
    }

    private void Subscribe()
    {
        ProgressionState state = ProgressionState.Instance;
        if (state == null)
        {
            Apply(Realm.Reality);
            return;
        }
        if (!subscribed)
        {
            state.RealmChanged += Apply;
            subscribed = true;
        }
        Apply(state.CurrentRealm);
    }

    private void Apply(Realm current)
    {
        bool present = current == existsIn;
        IsPresent = present;
        foreach (Renderer r in renderers) if (r != null) r.enabled = present;
        foreach (Collider c in colliders) if (c != null) c.enabled = present;
        foreach (Light l in lights) if (l != null) l.enabled = present;
    }
}