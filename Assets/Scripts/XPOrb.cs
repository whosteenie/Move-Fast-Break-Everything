using UnityEngine;

public class XPOrb : MonoBehaviour {
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

    private int RewardAmount => GetRewardAmount(tier);

    private void Awake() {
        if(spriteRenderer == null) {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        ApplyTierVisuals();
    }

    private void OnValidate() {
        if(spriteRenderer == null) {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        ApplyTierVisuals();
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if(!collision.gameObject.CompareTag("Player")) {
            return;
        }

        var playerLevelUp = collision.gameObject.GetComponent<PlayerLevelUp>();
        if(playerLevelUp == null) {
            return;
        }

        playerLevelUp.AddXp(RewardAmount);
        Destroy(gameObject);
    }

    private void ApplyTierVisuals() {
        if(spriteRenderer == null) {
            return;
        }

        spriteRenderer.color = GetTierColor(tier);
    }

    private static int GetRewardAmount(XPOrbTier orbTier) {
        return orbTier switch {
            XPOrbTier.Tier2 => 3,
            XPOrbTier.Tier3 => 6,
            _ => 1
        };
    }

    private Color GetTierColor(XPOrbTier orbTier) {
        return orbTier switch {
            XPOrbTier.Tier2 => tier2Color,
            XPOrbTier.Tier3 => tier3Color,
            _ => tier1Color
        };
    }
}
