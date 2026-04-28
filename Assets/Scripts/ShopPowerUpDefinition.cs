using UnityEngine;

[CreateAssetMenu(fileName = "ShopPowerUp", menuName = "Shop/Power Up")]
public class ShopPowerUpDefinition : ScriptableObject
{
    [SerializeField] private string powerUpId = "power_up";
    [SerializeField] private string displayName = "Power Up";
    [SerializeField, TextArea] private string description = "Describe this power up.";
    [SerializeField] private Sprite icon;
    [SerializeField, Min(1)] private int baseCost = 100;
    [SerializeField, Min(0)] private int costIncreasePerRank = 75;
    [SerializeField, Min(1)] private int maxRank = 5;

    public string PowerUpId => powerUpId;
    public string DisplayName => displayName;
    public string Description => description;
    public Sprite Icon => icon;
    public int MaxRank => maxRank;

    public int GetCostForRank(int currentRank)
    {
        return baseCost + costIncreasePerRank * Mathf.Max(0, currentRank);
    }
}
