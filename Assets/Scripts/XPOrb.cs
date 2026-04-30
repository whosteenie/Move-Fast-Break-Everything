using UnityEngine;

public class XPOrb : MagneticPickup {
    public enum XPOrbTier {
        Tier1,
        Tier2,
        Tier3
    }

    [SerializeField] private XPOrbTier tier = XPOrbTier.Tier1;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color tier1Color = new(0.09f, 0.52f, 0.85f, 1f);
    [SerializeField] private Color tier2Color = new(0.22f, 0.82f, 0.33f, 1f);
    [SerializeField] private Color tier3Color = new(0.88f, 0.24f, 0.24f, 1f);
    [SerializeField] private int tier1BaseReward = 5;
    [SerializeField] private int tier2Multiplier = 4;
    [SerializeField] private int tier3Multiplier = 12;
    [SerializeField] private SoundDefinition collectSound;

    public int RewardAmount => GetRewardAmount(tier, tier1BaseReward, tier2Multiplier, tier3Multiplier);
    public XPOrbTier Tier => tier;
    public int Tier1RewardValue => Mathf.Max(1, tier1BaseReward);
    public int Tier2RewardValue => GetRewardAmount(XPOrbTier.Tier2, tier1BaseReward, tier2Multiplier, tier3Multiplier);
    public int Tier3RewardValue => GetRewardAmount(XPOrbTier.Tier3, tier1BaseReward, tier2Multiplier, tier3Multiplier);

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

    public void SetTier(XPOrbTier newTier) {
        tier = newTier;
        ApplyTierVisuals();
    }

    public static int GetRewardAmount(XPOrbTier orbTier, int tier1BaseReward, int tier2Multiplier, int tier3Multiplier) {
        int clampedBaseReward = Mathf.Max(1, tier1BaseReward);
        int clampedTier2Multiplier = Mathf.Max(2, tier2Multiplier);
        int clampedTier3Multiplier = Mathf.Max(clampedTier2Multiplier + 1, tier3Multiplier);

        return orbTier switch {
            XPOrbTier.Tier2 => clampedBaseReward * clampedTier2Multiplier,
            XPOrbTier.Tier3 => clampedBaseReward * clampedTier3Multiplier,
            _ => clampedBaseReward
        };
    }

    protected override bool TryCollect(GameObject playerObject) {
        var playerLevelUp = playerObject.GetComponent<PlayerLevelUp>();
        if(playerLevelUp == null) {
            return false;
        }

        playerLevelUp.AddXp(RewardAmount);
        SoundManager.Play(collectSound);
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
