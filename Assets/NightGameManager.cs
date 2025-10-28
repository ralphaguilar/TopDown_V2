using UnityEngine;
using UnityEngine.SceneManagement;

public class NightGameManager : MonoBehaviour
{
    public NightClock clock;
    public FadeScreen fader;          // drag your FadeScreen here
    public string nextSceneName = "NextSceneName";
    public float fadeDuration = 1.0f;
    public bool pauseGameplayDuringFade = true;

    void OnEnable()
    {
        if (clock) clock.OnNightEnded += HandleNightWin;
    }
    void OnDisable()
    {
        if (clock) clock.OnNightEnded -= HandleNightWin;
    }

    void HandleNightWin()
    {
        // Optional: stop enemy spawns, disable player input, etc.
        if (pauseGameplayDuringFade)
            Time.timeScale = 0f;  // we’re using unscaled time in FadeScreen, so this is safe

        StartCoroutine(DoFadeThenLoad());
    }

    System.Collections.IEnumerator DoFadeThenLoad()
    {
        if (fader)
        {
            // Fade to black, then load
            yield return fader.FadeOutAndLoad(nextSceneName, fadeDuration);
        }
        else
        {
            // Fallback: load immediately
            SceneManager.LoadScene(nextSceneName);
        }

        // Important: restore timescale for the next scene
        Time.timeScale = 1f;
    }
}