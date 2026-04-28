using UnityEngine;

public class Tower_Dex : Tower_Base
{
    private float dexterityBoost = 0.2f;
    private bool hasBeenUSed = false;
    protected override void ApplyEffect(Stats stats)
    {
        if (hasBeenUSed) return;
        stats.IncreaseDexterity(dexterityBoost);
        Debug.Log("Dexterity upgrade: " + stats.dexterityMultiplier);
        hasBeenUSed = true;
    }
}
