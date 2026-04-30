using UnityEngine;

public class Tower_Defense : Tower_Base
{
    private float defenseBoost = 0.2f;


    protected override void ApplyEffect(Stats stats)
    {

        stats.IncreaseDefense(defenseBoost);
        Debug.Log("Defense upgraded: " + stats.defense);
        Destroy(gameObject);
    }
}

