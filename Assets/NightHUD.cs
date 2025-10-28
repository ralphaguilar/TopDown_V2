using TMPro;
using UnityEngine;

public class NightHUD : MonoBehaviour
{
    public NightClock clock;
    public TMP_Text timeText;      // e.g., "12:40 AM"
    public TMP_Text progressText;  // optional: "Night 73%"

    void Update()
    {
        if (!clock) return;

        clock.GetDisplayTime(out int h12, out int minute, out bool isAM);
        if (timeText) timeText.text = $"{h12}:{minute:00} {(isAM ? "AM" : "PM")}";

        if (progressText) progressText.text = $"Night {(clock.Progress01 * 100f):0}%";
    }
}
