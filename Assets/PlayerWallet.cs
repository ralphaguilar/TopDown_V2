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
            if (Current > 0) Instance.Add(Current);
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        
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

}
