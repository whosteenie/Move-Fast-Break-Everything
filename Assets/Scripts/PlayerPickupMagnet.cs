using UnityEngine;

public class PlayerPickupMagnet : MonoBehaviour
{
    private const string MagnetPowerUpId = "magnet";

    [SerializeField] private CircleCollider2D magnetCollider;
    [SerializeField] private Transform magnetTarget;
    [SerializeField, Min(0f)] private float basePickupRadius = 2f;
    [SerializeField, Min(0f)] private float radiusIncreasePerRank = 0.25f;

    public float PickupRadius => magnetCollider != null ? magnetCollider.radius : basePickupRadius;

    private void Start()
    {
        ApplyPurchasedPowerUps();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TriggerPickupMagnet(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TriggerPickupMagnet(other);
    }

    public void SetPickupRadius(float radius)
    {
        if (magnetCollider == null)
        {
            return;
        }

        magnetCollider.radius = Mathf.Max(0f, radius);
    }

    public void AddPickupRadius(float amount)
    {
        SetPickupRadius(PickupRadius + amount);
    }

    private void TriggerPickupMagnet(Collider2D other)
    {
        var pickup = other.GetComponent<MagneticPickup>();
        if (pickup == null)
        {
            pickup = other.GetComponentInParent<MagneticPickup>();
        }

        if (pickup == null)
        {
            return;
        }

        pickup.TryStartMagnetSequence(magnetTarget);
    }

    private void ApplyPurchasedPowerUps()
    {
        var magnetRank = ShopPowerUpProgress.GetRank(MagnetPowerUpId);
        SetPickupRadius(basePickupRadius * (1f + magnetRank * radiusIncreasePerRank));
    }
}
