
public class Tower_Defense : Tower_Base
{
    private float defenseBoost = 0.2f;
    private bool hasBeenUSed = false;

    protected override void ApplyEffect(Stats stats)
    {
        if (hasBeenUSed) return;
        stats.IncreaseDefense(defenseBoost);

        hasBeenUSed = true;
    }
}

