using UnityEngine;

public static class ShopPowerUpProgress
{
    private const string RankKeyPrefix = "Shop.PowerUp.";
    private const string RankKeySuffix = ".Rank";

    public static int GetRank(ShopPowerUpDefinition item)
    {
        if (item == null)
        {
            return 0;
        }

        return GetRank(GetPowerUpId(item));
    }

    public static int GetRank(string powerUpId)
    {
        if (string.IsNullOrWhiteSpace(powerUpId))
        {
            return 0;
        }

        return Mathf.Max(0, PlayerPrefs.GetInt(GetRankKey(powerUpId), 0));
    }

    public static void SetRank(ShopPowerUpDefinition item, int rank)
    {
        if (item == null)
        {
            return;
        }

        PlayerPrefs.SetInt(GetRankKey(item), Mathf.Max(0, rank));
    }

    public static string GetRankKey(ShopPowerUpDefinition item)
    {
        return GetRankKey(GetPowerUpId(item));
    }

    private static string GetRankKey(string powerUpId)
    {
        return $"{RankKeyPrefix}{powerUpId}{RankKeySuffix}";
    }

    private static string GetPowerUpId(ShopPowerUpDefinition item)
    {
        return string.IsNullOrWhiteSpace(item.PowerUpId) ? item.name : item.PowerUpId;
    }
}
