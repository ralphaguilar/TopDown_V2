using System;
using UnityEngine;

public class PlayerWallet : MonoBehaviour
{
    public static PlayerWallet Instance { get; private set; }

    [Header("Starting Money (only used on first instance)")]
    public int startingMoney = 0;

    public int Current { get; private set; }

    public event Action<int> onMoneyChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // If a new wallet spawns in the next scene, destroy it,
            // BUT first, copy its money into the persistent one if needed.
            if (Current > 0) Instance.Add(Current);
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Initialize once
        Current = Mathf.Max(0, startingMoney);
        onMoneyChanged?.Invoke(Current);
    }

    public void Add(int amount)
    {
        if (amount <= 0) return;
        Current += amount;
        onMoneyChanged?.Invoke(Current);
    }

    public bool CanAfford(int amount) => amount <= Current;

    public bool Spend(int amount)
    {
        if (amount <= 0) return true;
        if (!CanAfford(amount)) return false;
        Current -= amount;
        onMoneyChanged?.Invoke(Current);
        return true;
    }

    // Optional: save/load between game sessions
    public void SaveToPrefs(string key = "Wallet")
    {
        PlayerPrefs.SetInt(key, Current);
        PlayerPrefs.Save();
    }
    public void LoadFromPrefs(string key = "Wallet")
    {
        if (PlayerPrefs.HasKey(key))
        {
            Current = PlayerPrefs.GetInt(key, 0);
            onMoneyChanged?.Invoke(Current);
        }
    }
}
