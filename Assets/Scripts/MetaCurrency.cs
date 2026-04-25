using UnityEngine;

public static class MetaCurrency
{
    private const string TotalCoinsKey = "MetaCurrency.TotalCoins";
    private const int DebugMinimumCoins = 5000;

    public static int TotalCoins
    {
        get
        {
            EnsureDebugMinimumCoins();
            return PlayerPrefs.GetInt(TotalCoinsKey, 0);
        }
    }

    public static void AddCoins(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        var newTotal = TotalCoins + amount;
        PlayerPrefs.SetInt(TotalCoinsKey, newTotal);
        PlayerPrefs.Save();
        Debug.Log($"Added {amount} coins. Total coins: {newTotal}");
    }

    public static bool TrySpendCoins(int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        if (TotalCoins < amount)
        {
            return false;
        }

        var newTotal = TotalCoins - amount;
        PlayerPrefs.SetInt(TotalCoinsKey, newTotal);
        PlayerPrefs.Save();
        return true;
    }

    private static void EnsureDebugMinimumCoins()
    {
        var currentCoins = PlayerPrefs.GetInt(TotalCoinsKey, 0);
        if (currentCoins >= DebugMinimumCoins)
        {
            return;
        }

        // TODO: Remove this debug coin bootstrap after shop testing is complete.
        Debug.LogWarning($"DEBUG SHOP TESTING: Raising meta coin total to {DebugMinimumCoins}. Remove this before release.");
        PlayerPrefs.SetInt(TotalCoinsKey, DebugMinimumCoins);
        PlayerPrefs.Save();
    }
}
