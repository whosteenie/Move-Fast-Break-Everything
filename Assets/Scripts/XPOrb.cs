using UnityEngine;

public class XPOrb : MagneticPickup {
    private enum XPOrbTier {
        Tier1,
        Tier2,
        Tier3
    }

    [SerializeField] private XPOrbTier tier = XPOrbTier.Tier1;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color tier1Color = new(0.09f, 0.52f, 0.85f, 1f);
    [SerializeField] private Color tier2Color = new(0.22f, 0.82f, 0.33f, 1f);
    [SerializeField] private Color tier3Color = new(0.88f, 0.24f, 0.24f, 1f);
    [SerializeField] private int tier1Reward = 20;
    [SerializeField] private int tier2Reward = 35;
    [SerializeField] private int tier3Reward = 50;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float launchSpeed = 5f;
    [SerializeField] private float launchDuration = 0.15f;
    [SerializeField] private float magnetSpeed = 10f;
    [SerializeField] private float magnetAcceleration = 30f;
    [SerializeField] private SoundDefinition collectSound;

    private int RewardAmount => GetRewardAmount(tier);

    protected override void Awake() {
        base.Awake();

        if(spriteRenderer == null) {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        ApplyTierVisuals();
    }

    protected override void OnValidate() {
        base.OnValidate();

        if(spriteRenderer == null) {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        ApplyTierVisuals();
    }

    private void ApplyTierVisuals() {
        if(spriteRenderer == null) {
            return;
        }

        spriteRenderer.color = GetTierColor(tier);
    }

    private int GetRewardAmount(XPOrbTier orbTier) {
        return orbTier switch {
            XPOrbTier.Tier2 => tier2Reward,
            XPOrbTier.Tier3 => tier3Reward,
            _ => tier1Reward
        };
    }

    protected override bool TryCollect(GameObject playerObject) {
        var playerLevelUp = playerObject.GetComponent<PlayerLevelUp>();
        if(playerLevelUp == null) {
            return false;
        }

        playerLevelUp.AddXp(RewardAmount);
        SoundManager.Play(collectSound);
        Destroy(gameObject);
    }

    private Vector2 GetCurrentPosition() {
        return rb != null ? rb.position : transform.position;
    }

    private void MoveOrb(Vector2 delta) {
        if(rb != null) {
            rb.MovePosition(rb.position + delta);
            return;
        }

        transform.position += (Vector3)delta;
    }

    private bool IsTouchingPickupCollider(Collider2D otherCollider) {
        return pickupCollider != null && pickupCollider.Distance(otherCollider).isOverlapped;
    }

    private Color GetTierColor(XPOrbTier orbTier) {
        return orbTier switch {
            XPOrbTier.Tier2 => tier2Color,
            XPOrbTier.Tier3 => tier3Color,
            _ => tier1Color
        };
    }
}
