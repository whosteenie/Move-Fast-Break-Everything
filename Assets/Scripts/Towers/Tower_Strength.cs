using UnityEngine;

public class Tower_Strength : Tower_Base
{
    public float pierceBoost = 0.2f;
    public float thornsBoost = 0.2f;
    private bool hasBeenUSed = false;
    protected override void ApplyEffect(Stats stats)
    {
        if (hasBeenUSed) return;
        //change it to upgrade thorns damage
        stats.IncreasePierce(pierceBoost);
        stats.IncreaseThorns(thornsBoost);
        Debug.Log("Pierce and Throns upgraded");
        hasBeenUSed = true;
    }
}
