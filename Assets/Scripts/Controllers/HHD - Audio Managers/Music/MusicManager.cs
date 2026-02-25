using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Library")]
    [SerializeField] private MusicLibrary library;

    [Header("Crossfade")]
    [SerializeField] private float crossfadeSeconds = 1.0f;

    [Header("Initial Delay (first music start only)")]
    [SerializeField] private float initialPlayDelaySeconds = 0f;

    private AudioSource a;
    private AudioSource b;
    private AudioSource current;
    private AudioSource next;

    private bool hasStartedPlayback;
    private Coroutine switchRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        a = gameObject.AddComponent<AudioSource>();
        b = gameObject.AddComponent<AudioSource>();

        a.playOnAwake = false;
        b.playOnAwake = false;

        current = a;
        next = b;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void Start()
    {
        // Immediate handling on first scene
        PlayForScene(SceneManager.GetActiveScene().name);
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayForScene(scene.name);
    }

    public void PlayForScene(string sceneName)
    {
        if (library == null) return;
        if (!library.TryGet(sceneName, out var entry)) return;

        if (current.isPlaying && current.clip == entry.clip)
            return;

        if (switchRoutine != null)
        {
            StopCoroutine(switchRoutine);
            switchRoutine = null;
        }

        current.Stop();
        next.Stop();

        switchRoutine = StartCoroutine(SwitchTo(entry));
    }

    private IEnumerator SwitchTo(SceneMusicEntry entry)
    {
        // Prepare next source
        next.clip = entry.clip;
        next.loop = entry.loop;

        // Force start at the beginning
        next.time = 0f;
        next.volume = 0f;

        double dspStartTime = AudioSettings.dspTime;
        double scheduledTime = dspStartTime;

        if (!hasStartedPlayback && initialPlayDelaySeconds > 0f)
            scheduledTime += initialPlayDelaySeconds;

        next.PlayScheduled(scheduledTime);

        if (!hasStartedPlayback && initialPlayDelaySeconds > 0f)
            yield return new WaitForSecondsRealtime(initialPlayDelaySeconds);

        hasStartedPlayback = true;

        yield return Crossfade(current, next, entry.volume, crossfadeSeconds);

        current.Stop();
        current.clip = null;
        current.volume = 0f;

        var temp = current;
        current = next;
        next = temp;

        switchRoutine = null;
    }

    private static IEnumerator Crossfade(AudioSource from, AudioSource to, float toTargetVolume, float seconds)
    {
        if (seconds <= 0f)
        {
            if (from != null) from.volume = 0f;
            if (to != null) to.volume = toTargetVolume;
            yield break;
        }

        float t = 0f;
        float fromStart = (from != null) ? from.volume : 0f;

        while (t < seconds)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / seconds);

            if (from != null) from.volume = Mathf.Lerp(fromStart, 0f, k);
            if (to != null) to.volume = Mathf.Lerp(0f, toTargetVolume, k);

            yield return null;
        }

        if (from != null) from.volume = 0f;
        if (to != null) to.volume = toTargetVolume;
    }
}

// Old Music Manager
//public class MusicManager : MonoBehaviour
//{
//    [SerializeField] AudioSource audioSource;
//    void Start()
//    {
//        audioSource.PlayDelayed(2f);
//    }
//}
