using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CanvasGroup))]
public class FadeScreen : MonoBehaviour
{
    public float defaultDuration = 1.0f;
    CanvasGroup cg;

    void Awake()
    {
        cg = GetComponent<CanvasGroup>();
        if (!cg) cg = gameObject.AddComponent<CanvasGroup>();
        // Ensure starting transparent unless you want a fade-in on scene start
        cg.alpha = Mathf.Clamp01(cg.alpha);
    }

    public IEnumerator FadeIn(float duration = -1f)
    {
        if (duration <= 0f) duration = defaultDuration;
        yield return Fade(1f, 0f, duration);
    }

    public IEnumerator FadeOut(float duration = -1f)
    {
        if (duration <= 0f) duration = defaultDuration;
        yield return Fade(0f, 1f, duration);
    }

    public IEnumerator FadeOutAndLoad(string sceneName, float duration = -1f)
    {
        if (duration <= 0f) duration = defaultDuration;
        yield return FadeOut(duration);
        // Load after fully black
        SceneManager.LoadScene(sceneName);
    }

    IEnumerator Fade(float from, float to, float duration)
    {
        float t = 0f;
        cg.alpha = from;

        cg.blocksRaycasts = true;
        cg.interactable = true;

        while (t < duration)
        {
         
            t += Time.unscaledDeltaTime;
            float a = Mathf.Lerp(from, to, t / duration);
            cg.alpha = a;
            yield return null;
        }
        cg.alpha = to;
    }
}