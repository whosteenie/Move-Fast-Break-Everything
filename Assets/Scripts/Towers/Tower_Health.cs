using UnityEngine;

public class Tower_Health : Tower_Base
{
    public float healthBoost = 0.2f;

    protected override void ApplyEffect(Stats stats)
    {

        stats.IncreaseHealthPercent(healthBoost);
        Debug.Log("Health upgraded");
        Destroy(gameObject);
    }
}
