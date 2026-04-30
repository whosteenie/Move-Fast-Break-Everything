using UnityEngine;

public class Enemy : MonoBehaviour
{
    private EnemyStats stats;
    private float moveSpeed = 0.5f;
    [SerializeField] private GameObject xpOrbPrefab;
    [SerializeField] private SoundDefinition hurtSound;
    [SerializeField] private int minXpOrbDrops = 1;
    [SerializeField] private int maxXpOrbDrops = 2;
    [SerializeField] private float xpDropRadius = 0.6f;
    [SerializeField] private float minXpOrbSpacing = 0.35f;
    [Header("Movement")]
    [SerializeField] private float separationRadius = 1.1f;
    [SerializeField] private float separationStrength = 1.5f;
    [SerializeField] private float steeringSmoothness = 8f;
    [SerializeField] private LayerMask enemyLayerMask = ~0;

    private Transform playerLocation;
    private float currentHealth;
    private Rigidbody2D _rb;
    private readonly Collider2D[] _nearbyEnemyResults = new Collider2D[16];
    private ContactFilter2D _enemyContactFilter;
    private Vector2 _currentMoveDirection = Vector2.zero;

    // public int damageMultiplier;
    //damage mult will be increased when enemy levls up using similar level up system to player, but for now just a base damage

    private void Start()
    {
        if (stats != null)
        {
            currentHealth = stats.GetMaxHealth();
        }
        else
        {
            currentHealth = 10;
        }
    }
    private void Awake()
    {
        stats = GetComponent<EnemyStats>();
        _rb = GetComponent<Rigidbody2D>();
        _enemyContactFilter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = enemyLayerMask,
            useTriggers = true
        };
    }
    
    public void TakeDamage(int damageTaken, float pierce)
    {
        int finalDamage = damageTaken;

        if (stats != null)
        {
            finalDamage = stats.CalculateDamageTaken(damageTaken, pierce);
        }
        if (finalDamage <= 0)
        {
            finalDamage = stats.CalculateDamageTaken(damageTaken, pierce);
        }

        currentHealth -= finalDamage;
        currentHealth = Mathf.Max(currentHealth, 0);
        SoundManager.Play(hurtSound);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        SpawnXpOrbs();
        Destroy(gameObject);
    }

    private void SpawnXpOrbs()
    {
        if (xpOrbPrefab == null)
        {
            return;
        }

        var orbTemplate = xpOrbPrefab.GetComponent<XPOrb>();
        if (orbTemplate == null)
        {
            return;
        }

        int dropCount = Random.Range(minXpOrbDrops, maxXpOrbDrops + 1);
        Vector3 deathPosition = transform.position;
        int enemyLevel = GameManager.CurrentEnemyLevel;
        Vector3[] placedPositions = new Vector3[dropCount];

        for (int i = 0; i < dropCount; i++)
        {
            Vector3 spawnPosition = FindXpOrbSpawnPosition(deathPosition, placedPositions, i);
            placedPositions[i] = spawnPosition;
            var orbObject = Instantiate(xpOrbPrefab, spawnPosition, xpOrbPrefab.transform.rotation);
            var spawnedOrb = orbObject.GetComponent<XPOrb>();
            if (spawnedOrb != null)
            {
                spawnedOrb.InitTier(enemyLevel);
            }
        }
    }

    private Vector3 FindXpOrbSpawnPosition(Vector3 center, Vector3[] placedPositions, int placedCount)
    {
        const int MaxAttempts = 12;

        for (int attempt = 0; attempt < MaxAttempts; attempt++)
        {
            Vector2 offset = placedCount == 0
                ? Random.insideUnitCircle * (xpDropRadius * 0.5f)
                : Random.insideUnitCircle * xpDropRadius;
            Vector3 candidate = center + new Vector3(offset.x, offset.y, 0f);

            if (IsFarEnoughFromOtherDrops(candidate, placedPositions, placedCount))
            {
                return candidate;
            }
        }

        float angle = placedCount * Mathf.PI;
        Vector2 fallbackOffset = new(Mathf.Cos(angle), Mathf.Sin(angle));
        return center + new Vector3(fallbackOffset.x, fallbackOffset.y, 0f) * minXpOrbSpacing;
    }

    private bool IsFarEnoughFromOtherDrops(Vector3 candidate, Vector3[] placedPositions, int placedCount)
    {
        for (int i = 0; i < placedCount; i++)
        {
            if (Vector2.Distance(candidate, placedPositions[i]) < minXpOrbSpacing)
            {
                return false;
            }
        }

        return true;
    }

    public void UpdateMaxHealth(int newMaxHealth)
    {
        currentHealth = newMaxHealth;
        Debug.Log("Enemy max hp is now: " + newMaxHealth);
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        float speed = (stats != null) ? stats.GetSpeed() : moveSpeed;

        if (playerLocation == null)
        {
            var player = FindAnyObjectByType<TestMovement>();
            if (player != null)
            {
                playerLocation = player.transform;
            }
        }

        if (playerLocation == null || _rb == null)
        {
            return;
        }

        // This keeps enemies moving toward the player by default.
        var chaseDirection = ((Vector2)playerLocation.position - _rb.position).normalized;
        // Nearby enemies add a small push so the group spreads out instead of stacking.
        var separationDirection = GetSeparationDirection();
        var targetMoveDirection = chaseDirection + separationDirection * separationStrength;

        if (targetMoveDirection.sqrMagnitude > 0.0001f)
        {
            targetMoveDirection.Normalize();
        }
        else
        {
            targetMoveDirection = chaseDirection;
        }

        // Smooth steering cuts down on visible jitter when neighbors keep shifting around.
        _currentMoveDirection = Vector2.Lerp(_currentMoveDirection, targetMoveDirection, steeringSmoothness * Time.fixedDeltaTime);

        if (_currentMoveDirection.sqrMagnitude > 0.0001f)
        {
            _currentMoveDirection.Normalize();
        }
        else
        {
            _currentMoveDirection = chaseDirection;
        }

        var newPosition = _rb.position + _currentMoveDirection * (speed * Time.fixedDeltaTime);

        _rb.MovePosition(newPosition);
    }

    private Vector2 GetSeparationDirection()
    {
        var separation = Vector2.zero;
        var nearbyEnemyCount = Physics2D.OverlapCircle(transform.position, separationRadius, _enemyContactFilter, _nearbyEnemyResults);

        for (var i = 0; i < nearbyEnemyCount; i++)
        {
            var nearbyEnemy = _nearbyEnemyResults[i];
            var otherEnemy = nearbyEnemy.GetComponent<Enemy>();

            if (otherEnemy == null || otherEnemy == this || nearbyEnemy.attachedRigidbody == _rb)
            {
                continue;
            }

            var offset = _rb.position - (Vector2)nearbyEnemy.transform.position;
            var distance = offset.magnitude;

            if (distance <= 0.01f)
            {
                continue;
            }

            // Enemies closer to this one push harder than enemies near the edge of the radius.
            var distancePercent = 1f - Mathf.Clamp01(distance / separationRadius);
            separation += offset.normalized * distancePercent;
        }

        return separation;
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        int damage = (stats != null) ? stats.GetDamage() : 1;
        if (collision.gameObject != null && collision.gameObject.CompareTag("Player"))
        {
            Player player = collision.gameObject.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }
            //Leads to fun lose screen by accident, all the enemies just fall down.
        }
    }
}
