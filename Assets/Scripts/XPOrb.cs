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
        return true;
    }

    private Color GetTierColor(XPOrbTier orbTier) {
        return orbTier switch {
            XPOrbTier.Tier2 => tier2Color,
            XPOrbTier.Tier3 => tier3Color,
            _ => tier1Color
        };
    }
}
