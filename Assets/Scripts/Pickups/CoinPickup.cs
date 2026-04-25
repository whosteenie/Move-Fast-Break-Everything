using UnityEngine;

public class CoinPickup : MagneticPickup
{
    [SerializeField] private int minCoinValue = 1;
    [SerializeField] private int maxCoinValue = 3;

    protected override bool TryCollect(GameObject playerObject)
    {
        var minValue = Mathf.Max(0, minCoinValue);
        var maxValue = Mathf.Max(minValue, maxCoinValue);
        var coinAmount = Random.Range(minValue, maxValue + 1);

        if (coinAmount <= 0)
        {
            return false;
        }

        MetaCurrency.AddCoins(coinAmount);
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddRunCoins(coinAmount);
        }
        return true;
    }
}
