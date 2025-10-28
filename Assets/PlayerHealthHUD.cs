using TMPro;
using UnityEngine;

public class PlayerHealthHUD : MonoBehaviour
{
    public PlayerHealth player;
    public TMP_Text text;

    void OnEnable()
    {
        if (!player)
            player = FindFirstObjectByType<PlayerHealth>();

        if (player)
            player.onHealthChanged += UpdateUI;
    }

    void OnDisable()
    {
        if (player)
            player.onHealthChanged -= UpdateUI;
    }

    void Start()
    {
        // Initialize the UI with the current health at scene start
        if (player)
            UpdateUI(player.CurrentHealth, player.maxHealth);
    }

    void UpdateUI(float hp, float max)
    {
        if (text)
            text.text = $"{hp:0}/{max:0}";
    }
}