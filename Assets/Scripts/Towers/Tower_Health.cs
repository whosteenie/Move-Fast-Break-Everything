using UnityEngine;

public class Tower_Health : Tower_Base
{
    public float healthBoost = 0.2f;
    private bool hasBeenUSed = false;
    protected override void ApplyEffect(Stats stats)
    {
        if (hasBeenUSed) return;
        stats.IncreaseHealthPercent(healthBoost);
        Debug.Log("Health upgraded");
        hasBeenUSed = true;
    }
}
