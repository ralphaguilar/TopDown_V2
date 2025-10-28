using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }
    public AudioClip gameTrack;
    public float fadeInTime = 1.0f;
    AudioSource src;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        src = GetComponent<AudioSource>();
        src.loop = true;
        src.playOnAwake = false;
        src.spatialBlend = 0f;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "SampleScene")
        {
            Play(gameTrack, fadeInTime);
        }
        else
        {
            StopMusic();
        }
    }

    public void Play(AudioClip clip, float fadeIn = 1f)
    {
        if (clip == null) return;
        src.clip = clip;
        src.volume = 1f;
        src.Play();
    }

    public void StopMusic()
    {
        if (src.isPlaying)
            src.Stop();
    }
}