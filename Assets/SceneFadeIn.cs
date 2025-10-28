using UnityEngine;
using System.Collections;

public class SceneFadeIn : MonoBehaviour
{
    public float fadeInTime = 1f;

    void Start()
    {
        var fade = FindFirstObjectByType<FadeScreen>();
        if (fade)
            StartCoroutine(fade.FadeIn(fadeInTime));
    }
}
