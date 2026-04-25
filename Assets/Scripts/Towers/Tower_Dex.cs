using UnityEngine;

public class Tower_Dex : Tower_Base
{
    private float dexterityBoost = 0.2f;

    protected override void ApplyEffect(Stats stats)
    {
        stats.IncreaseDexterity(dexterityBoost);
        Debug.Log("Dexterity upgrade: " + stats.dexterityMultiplier);
    }
}
