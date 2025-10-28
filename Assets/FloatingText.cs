using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    public float moveUpSpeed = 20f;
    public float lifetime = 1f;
    public float fadeTime = 0.8f;

    TMP_Text txt;
    float timer;
    Color startColor;

    void Awake()
    {
        txt = GetComponent<TMP_Text>();
        startColor = txt.color;
    }

    public void SetText(string text)
    {
        txt.text = text;
    }

    void Update()
    {
        // Move upward
        transform.position += Vector3.up * moveUpSpeed * Time.deltaTime;

        timer += Time.deltaTime;

        // Fade out
        float alpha = Mathf.Lerp(startColor.a, 0, timer / fadeTime);
        txt.color = new Color(startColor.r, startColor.g, startColor.b, alpha);

        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}
