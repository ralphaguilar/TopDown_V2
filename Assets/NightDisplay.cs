using TMPro;
using UnityEngine;

public class NightDisplay : MonoBehaviour
{
    [Header("References")]
    public TMP_Text nightLabel;

    void Start()
    {
        if (!nightLabel)
            nightLabel = GetComponent<TMP_Text>();

        if (nightLabel && GameFlow.Instance)
            nightLabel.text = $"Night {GameFlow.Instance.currentNight}";
        else if (nightLabel)
            nightLabel.text = "Night ?";
    }
}