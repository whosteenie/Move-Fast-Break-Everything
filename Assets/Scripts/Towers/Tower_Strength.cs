using UnityEngine;

public class Tower_Strength : Tower_Base
{
    public float pierceBoost = 0.2f;
    public float thornsBoost = 0.2f;

    protected override void ApplyEffect(Stats stats)
    {

        //change it to upgrade thorns damage
        stats.IncreasePierce(pierceBoost);
        stats.IncreaseThorns(thornsBoost);
        Debug.Log("Pierce and Throns upgraded");
        Destroy(gameObject);
    }
}
