using UnityEngine;

public class Tower_Strength : Tower_Base
{
    public float damageBoost = 0.2f;

    protected override void ApplyEffect(Stats stats)
    {
        //change it to upgrade thorns damage
        stats.IncreaseRangedDamage(damageBoost);
        Debug.Log("Damage upgraded");
    }
}
