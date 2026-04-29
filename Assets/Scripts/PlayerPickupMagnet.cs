using UnityEngine;

public class PlayerPickupMagnet : MonoBehaviour
{
    [SerializeField] private CircleCollider2D magnetCollider;
    [SerializeField] private Transform magnetTarget;
    [SerializeField] private float pickupRadius = 2f;

    public float PickupRadius => pickupRadius;

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
        pickupRadius = Mathf.Max(0f, radius);
        ApplyPickupRadius();
    }

    public void AddPickupRadius(float amount)
    {
        SetPickupRadius(pickupRadius + amount);
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

    private void ApplyPickupRadius()
    {
        if (magnetCollider == null)
        {
            return;
        }

        magnetCollider.radius = Mathf.Max(0f, pickupRadius);
    }
}
