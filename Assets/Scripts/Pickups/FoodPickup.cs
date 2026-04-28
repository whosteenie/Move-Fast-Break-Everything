using UnityEngine;

public class FoodPickup : MagneticPickup
{
    [SerializeField] private int healAmount = 1;

    protected override bool TryCollect(GameObject playerObject)
    {
        var player = playerObject.GetComponent<Player>();
        if (player == null)
        {
            return false;
        }

        player.Heal(Mathf.Max(0, healAmount));
        return true;
    }
}
