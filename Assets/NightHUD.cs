using TMPro;
using UnityEngine;

public class NightHUD : MonoBehaviour
{
    public NightClock clock;
    public TMP_Text timeText;     
    public TMP_Text progressText;  

    void Update()
    {
        if (!clock) return;

        clock.GetDisplayTime(out int h12, out int minute, out bool isAM);
        if (timeText) timeText.text = $"{h12}:{minute:00} {(isAM ? "AM" : "PM")}";

        if (progressText) progressText.text = $"Night {(clock.Progress01 * 100f):0}%";
    }
}
