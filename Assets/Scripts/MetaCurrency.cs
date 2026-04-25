using UnityEngine;

public static class MetaCurrency
{
    private const string TotalCoinsKey = "MetaCurrency.TotalCoins";

    public static int TotalCoins => PlayerPrefs.GetInt(TotalCoinsKey, 0);

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
}
