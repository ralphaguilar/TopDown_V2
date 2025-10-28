using UnityEngine;
using UnityEngine.SceneManagement;

public class NightGameManager : MonoBehaviour
{
    public NightClock clock;
    public FadeScreen fader;         
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
    
        if (pauseGameplayDuringFade)
            Time.timeScale = 0f;  

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
            //load immediately
            SceneManager.LoadScene(nextSceneName);
        }

        // restore timescale for the next scene
        Time.timeScale = 1f;
    }
}