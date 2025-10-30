using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Scene Tracks")]
    public AudioClip gameTrack;    // For SampleScene
    public AudioClip casinoTrack;  // For Casino scene

    [Header("Settings")]
    public float fadeInTime = 1.0f;

    private AudioSource src;
    private string currentSceneName;

    void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Setup AudioSource
        src = GetComponent<AudioSource>();
        src.loop = true;
        src.playOnAwake = false;
        src.spatialBlend = 0f;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentSceneName = scene.name;

        switch (scene.name)
        {
            case "SampleScene":
                Play(gameTrack, fadeInTime);
                break;

            case "Casino":
                Play(casinoTrack, fadeInTime);
                break;

            default:
                StopMusic();
                break;
        }
    }

    public void Play(AudioClip clip, float fadeIn = 1f)
    {
        if (clip == null) return;

        // If already playing the same clip, do nothing
        if (src.clip == clip && src.isPlaying) return;

        StopAllCoroutines();
        StartCoroutine(FadeInClip(clip, fadeIn));
    }

    private System.Collections.IEnumerator FadeInClip(AudioClip clip, float time)
    {
        src.clip = clip;
        src.volume = 0f;
        src.Play();

        float t = 0f;
        while (t < time)
        {
            t += Time.deltaTime;
            src.volume = Mathf.Lerp(0f, 1f, t / time);
            yield return null;
        }
        src.volume = 1f;
    }

    public void StopMusic()
    {
        StopAllCoroutines();
        if (src.isPlaying)
            src.Stop();
    }
}