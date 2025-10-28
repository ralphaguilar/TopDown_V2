using TMPro;
using UnityEngine;

public class MoneyHUD : MonoBehaviour
{
    public PlayerWallet wallet;
    public TMP_Text moneyText;
    public string prefix = "$";

    void OnEnable()
    {
        if (!wallet) wallet = FindFirstObjectByType<PlayerWallet>();
        if (wallet != null)
        {
            wallet.onMoneyChanged += UpdateText;
            UpdateText(wallet.Current); // init
        }
    }

    void OnDisable()
    {
        if (wallet != null) wallet.onMoneyChanged -= UpdateText;
    }

    void UpdateText(int value)
    {
        if (moneyText) moneyText.text = $"{prefix}{value}";
    }
}
