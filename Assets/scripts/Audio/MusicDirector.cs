using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class MusicDirector : MonoBehaviour
{
    [Header("Stems exported from the same downbeat and loop range")]
    [SerializeField] private AudioCue accompaniment;
    [SerializeField] private AudioCue drums;
    [SerializeField] private movementController player;
    [SerializeField, Min(0.1f)] private float schedulingLeadSeconds = 0.5f;

    [Header("Realm mix")]
    [SerializeField, Range(-80f, 0f)] private float dreamDrumsDb = -10f;
    [SerializeField, Min(0.01f)] private float realmFadeSeconds = 0.3f;
    [SerializeField, Range(0f, 1f)] private float realityVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float dreamVolume = 1f;
    [Tooltip("A gentle high-frequency rolloff on the music in Dream. Reality bypasses the filter.")]
    [SerializeField, Range(1000f, 22000f)] private float dreamLowPassHz = 9000f;

    [Header("Detection in either realm")]
    [Tooltip("Playback rate also raises pitch. 1.12 is 12 percent faster at full detection.")]
    [SerializeField, Range(1f, 1.5f)] private float maxDetectionSpeed = 1.12f;
    [Tooltip("Seconds to move between normal speed and maximum detection speed. Smaller changes take proportionally less time.")]
    [SerializeField, Min(0.01f)] private float detectionRampSeconds = 1f;

    public float RealityVolume { get => realityVolume; set => realityVolume = Mathf.Clamp01(value); }
    public float DreamVolume { get => dreamVolume; set => dreamVolume = Mathf.Clamp01(value); }
    public float? DetectionOverride { get; set; }
    public float Detection => Mathf.Clamp(DetectionOverride ?? (player != null ? player.detectionLevel : 0f), 0f, 100f);
    public float PlaybackSpeed => accompanimentSource != null ? accompanimentSource.pitch : 1f;

    private AudioSource accompanimentSource;
    private AudioSource drumsSource;
    private AudioLowPassFilter musicFilter;
    private ProgressionState progression;
    private Realm realm;
    private Coroutine preparation;
    private float realmGain;
    private float drumGain;
    private bool prepared;

    private void OnEnable()
    {
        if (accompaniment == null || drums == null || accompaniment.Clip == null || drums.Clip == null ||
            accompaniment.Clip.samples != drums.Clip.samples ||
            accompaniment.Clip.frequency != drums.Clip.frequency)
        {
            Debug.LogError("MusicDirector needs two stems with identical sample counts and sample rates.", this);
            enabled = false;
            return;
        }

        if (accompanimentSource == null)
        {
            accompanimentSource = CreateSource();
            drumsSource = CreateSource();
        }
        accompaniment.Configure(accompanimentSource);
        drums.Configure(drumsSource);
        if (musicFilter == null)
        {
            musicFilter = GetComponent<AudioLowPassFilter>();
            if (musicFilter == null) musicFilter = gameObject.AddComponent<AudioLowPassFilter>();
            musicFilter.lowpassResonanceQ = 1f;
        }

        progression = ProgressionState.Instance;
        if (progression != null) progression.RealmChanged += ApplyRealm;
        ApplyRealm(progression != null ? progression.CurrentRealm : Realm.Reality);
        realmGain = RealmVolume();
        drumGain = DrumVolume();
        musicFilter.cutoffFrequency = FilterCutoff();
        musicFilter.enabled = realm == Realm.Dream;
        ApplyMix();
        SetSpeed(DetectionSpeed());
        preparation = StartCoroutine(PreparePlayback());
    }

    private IEnumerator PreparePlayback()
    {
        accompaniment.Clip.LoadAudioData();
        drums.Clip.LoadAudioData();
        while (accompaniment.Clip.loadState == AudioDataLoadState.Loading ||
               drums.Clip.loadState == AudioDataLoadState.Loading)
            yield return null;

        if (accompaniment.Clip.loadState != AudioDataLoadState.Loaded ||
            drums.Clip.loadState != AudioDataLoadState.Loaded)
        {
            Debug.LogError("Music stem loading failed.", this);
            enabled = false;
            yield break;
        }

        double startTime = AudioSettings.dspTime + schedulingLeadSeconds;
        accompanimentSource.PlayScheduled(startTime);
        drumsSource.PlayScheduled(startTime);
        prepared = true;
    }

    private void Update()
    {
        if (!prepared) return;

        float deltaTime = Time.unscaledDeltaTime;
        float blend = 1f - Mathf.Exp(-deltaTime * 5f / Mathf.Max(0.01f, realmFadeSeconds));
        realmGain = Mathf.Lerp(realmGain, RealmVolume(), blend);
        drumGain = Mathf.Lerp(drumGain, DrumVolume(), blend);
        musicFilter.cutoffFrequency = Mathf.Lerp(musicFilter.cutoffFrequency, FilterCutoff(), blend);
        musicFilter.enabled = realm == Realm.Dream || musicFilter.cutoffFrequency < 21999f;
        ApplyMix();

        float speedStep = Mathf.Max(0.01f, maxDetectionSpeed - 1f) * deltaTime /
            Mathf.Max(0.01f, detectionRampSeconds);
        SetSpeed(Mathf.MoveTowards(accompanimentSource.pitch, DetectionSpeed(), speedStep));
    }

    private void ApplyRealm(Realm nextRealm) => realm = nextRealm;
    private float RealmVolume() => realm == Realm.Dream ? dreamVolume : realityVolume;
    private float DrumVolume() => realm == Realm.Dream ? Mathf.Pow(10f, dreamDrumsDb / 20f) : 1f;
    private float FilterCutoff() => realm == Realm.Dream ? dreamLowPassHz : 22000f;
    private float DetectionSpeed() => Mathf.Lerp(1f, maxDetectionSpeed, Detection / 100f);

    private void ApplyMix()
    {
        accompanimentSource.volume = accompaniment.Volume * realmGain;
        drumsSource.volume = drums.Volume * realmGain * drumGain;
    }

    private void SetSpeed(float speed)
    {
        accompanimentSource.pitch = speed;
        drumsSource.pitch = speed;
    }

    private AudioSource CreateSource()
    {
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.loop = true;
        source.spatialBlend = 0f;
        source.dopplerLevel = 0f;
        source.priority = 32;
        return source;
    }

    private void OnDisable()
    {
        if (progression != null) progression.RealmChanged -= ApplyRealm;
        if (preparation != null) StopCoroutine(preparation);
        prepared = false;
        if (musicFilter != null) musicFilter.enabled = false;
        if (accompanimentSource != null)
        {
            accompanimentSource.Stop();
            drumsSource.Stop();
        }
    }
}
