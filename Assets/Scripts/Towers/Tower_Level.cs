using UnityEngine;

public class Tower_Level : Tower_Base
{
    [Header("Stat Bonuses")]
    [SerializeField] private float speedIncrease = 0.1f;
    [SerializeField] private float damageIncrease = 0.1f;
    [SerializeField] private int flatHealthIncrease = 2;
    [SerializeField] private float healthPercentIncrease = 0.1f;
    [SerializeField] private float dexterityIncrease = 0.1f;
    [SerializeField] private float defenseIncrease = 0.5f;
    private bool hasBeenUSed = false;

    protected override void ApplyEffect(Stats stats)
    {
        if (hasBeenUSed) return;
        stats.IncreaseSpeed(speedIncrease);

        stats.IncreaseRangedDamage(damageIncrease);

        stats.IncreaseFlatHealth(flatHealthIncrease);

        stats.IncreaseHealthPercent(healthPercentIncrease);

        stats.IncreaseDexterity(dexterityIncrease);

        stats.IncreaseDefense(defenseIncrease);

        Player player = stats.GetComponent<Player>();

        if (player != null)
        {
            player.UpdateMaxHealth(stats.GetMaxHealth());
        }

        Debug.Log("All stats increased!");
        hasBeenUSed = true;
    }
}
