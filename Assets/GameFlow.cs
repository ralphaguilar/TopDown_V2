using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameFlow : MonoBehaviour
{
    public static GameFlow Instance { get; private set; }

    [Header("Scenes")]
    public string nightSceneName = "GameScene";
    public string casinoSceneName = "Casino";

    [Header("Night Progression")]
    public int currentNight = 1;
    public float fadeDuration = 1f;

    private FadeScreen fadeScreen;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        fadeScreen = FindFirstObjectByType<FadeScreen>(UnityEngine.FindObjectsInactive.Include);
    }

    public void StartNewRun()
    {
        currentNight = 1;
        LoadNightScene();
    }

    public void GoToCasino()
    {
        StartCoroutine(LoadSceneSmooth(casinoSceneName));
    }

    public void ProceedToNextNight()
    {
        currentNight++;
        LoadNightScene();
    }

    void LoadNightScene()
    {
        StartCoroutine(LoadSceneSmooth(nightSceneName));
    }

    IEnumerator LoadSceneSmooth(string scene)
    {
        if (!fadeScreen)
            fadeScreen = FindFirstObjectByType<FadeScreen>(UnityEngine.FindObjectsInactive.Include);

        if (fadeScreen)
            yield return fadeScreen.FadeOut(fadeDuration);

        yield return SceneManager.LoadSceneAsync(scene);

        if (fadeScreen)
            yield return fadeScreen.FadeIn(fadeDuration);
    }
}