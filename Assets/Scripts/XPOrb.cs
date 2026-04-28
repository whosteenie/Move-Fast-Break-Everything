using UnityEngine;

public class XPOrb : MonoBehaviour {
    private enum XPOrbTier {
        Tier1,
        Tier2,
        Tier3
    }

    [SerializeField] private XPOrbTier tier = XPOrbTier.Tier1;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Collider2D pickupCollider;
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
    private Transform _magnetTarget;
    private Vector2 _launchVelocity;
    private float _launchTimer;
    private float _currentMagnetSpeed;
    private bool _isMagnetized;
    private bool _hasTriggeredPickupRange;
    private bool _isCollected;

    private void Awake() {
        if(rb == null) {
            rb = GetComponent<Rigidbody2D>();
        }

        if(pickupCollider == null) {
            pickupCollider = GetComponent<Collider2D>();
        }

        if(spriteRenderer == null) {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        ApplyTierVisuals();
    }

    private void OnValidate() {
        if(rb == null) {
            rb = GetComponent<Rigidbody2D>();
        }

        if(pickupCollider == null) {
            pickupCollider = GetComponent<Collider2D>();
        }

        if(spriteRenderer == null) {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        ApplyTierVisuals();
    }

    private void FixedUpdate() {
        if(_magnetTarget == null) {
            return;
        }

        if(_launchTimer > 0f) {
            _launchTimer -= Time.fixedDeltaTime;
            MoveOrb(_launchVelocity * Time.fixedDeltaTime);
            return;
        }

        if(!_isMagnetized) {
            return;
        }

        _currentMagnetSpeed = Mathf.MoveTowards(_currentMagnetSpeed, magnetSpeed, magnetAcceleration * Time.fixedDeltaTime);

        var directionToPlayer = ((Vector2)_magnetTarget.position - GetCurrentPosition()).normalized;
        MoveOrb(directionToPlayer * (_currentMagnetSpeed * Time.fixedDeltaTime));
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if(_isCollected || !collision.CompareTag("Player")) {
            return;
        }

        if(IsTouchingPickupCollider(collision)) {
            Collect(collision.gameObject);
            return;
        }

        if(_hasTriggeredPickupRange) {
            return;
        }

        StartMagnetSequence(collision.transform);
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

    private void StartMagnetSequence(Transform playerTransform) {
        _magnetTarget = playerTransform;
        _hasTriggeredPickupRange = true;
        _isMagnetized = true;
        _launchTimer = launchDuration;
        _currentMagnetSpeed = 0f;

        var launchDirection = GetCurrentPosition() - (Vector2)playerTransform.position;
        if(launchDirection.sqrMagnitude < 0.0001f) {
            launchDirection = Random.insideUnitCircle.normalized;
        } else {
            launchDirection.Normalize();
        }

        _launchVelocity = launchDirection * launchSpeed;
    }

    private void Collect(GameObject playerObject) {
        var playerLevelUp = playerObject.GetComponent<PlayerLevelUp>();
        if(playerLevelUp == null) {
            return;
        }

        _isCollected = true;
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
