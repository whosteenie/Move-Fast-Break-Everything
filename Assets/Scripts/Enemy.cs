using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private static readonly List<Enemy> ActiveEnemies = new();
    private static readonly RaycastHit2D[] ObstacleHits = new RaycastHit2D[8];
    private static readonly ContactFilter2D ObstacleContactFilter = new()
    {
        useTriggers = false
    };

    private const float DefaultMoveSpeed = 0.5f;
    private const float DefaultSpeedMultiplier = 5f;
    private const float DefaultDamageMultiplier = 2f;
    private const float SteeringSmoothing = 12f;
    private const float ObstacleClearanceRadius = 0.25f;
    private const float ObstacleSideProbeAngle = 55f;
    private const float RepathInterval = 0.2f;
    private const float StuckCheckInterval = 0.45f;
    private const float StuckDistanceThreshold = 0.08f;
    private const float StuckRecoveryTime = 0.6f;
    private const float StuckSideBias = 1.35f;
    private const float WanderFrequency = 1.1f;
    private const float SeparationRandomness = 0.35f;
    private const float ContactDamageInterval = 0.5f;

    [Header("Drops")]
    [SerializeField] private GameObject xpOrbPrefab;
    [SerializeField] private int minXpOrbDrops = 1;
    [SerializeField] private int maxXpOrbDrops = 2;
    [SerializeField] private float xpDropRadius = 0.6f;
    [SerializeField] private float minXpOrbSpacing = 0.35f;

    [Header("Combat")]
    [SerializeField] private int maxHealth = 10;
    [SerializeField] private float baseDamage = 1f;

    [Header("Swarm Movement")]
    [SerializeField] private float moveSpeed = DefaultMoveSpeed;
    [SerializeField] private float acceleration = 18f;
    [SerializeField] private float desiredPlayerDistance = 0.85f;
    [SerializeField] private float orbitStrength = 0.75f;
    [SerializeField] private float separationRadius = 1.2f;
    [SerializeField] private float separationStrength = 1.8f;
    [SerializeField] private float obstacleProbeDistance = 1.2f;
    [SerializeField] private float obstacleAvoidanceStrength = 2.5f;
    [SerializeField] private float wanderStrength = 0.4f;

    private EnemyStats stats;
    private Rigidbody2D rb;
    private Collider2D enemyCollider;
    private Transform playerTransform;

    private int currentHealth;
    private float damageCooldownTimer;
    
    private float _repathTimer;
    private float _stuckTimer;
    private float _recoveryTimer;
    private float _orbitSign;
    private float _wanderSeed;
    private float _separationRadiusMultiplier;
    private float _separationStrengthMultiplier;
    private float _desiredDistanceMultiplier;
    private Vector2 _desiredVelocity;
    private Vector2 _currentVelocity;
    private Vector2 _appliedVelocity;
    private Vector2 _lastStuckPosition;
    private Vector2 _cachedPathDirection;
    private Vector2 _recoveryDirection;

    private void Awake()
    {
        stats = GetComponent<EnemyStats>();
        rb = GetComponent<Rigidbody2D>();
        enemyCollider = GetComponent<Collider2D>();
        _orbitSign = Random.value < 0.5f ? -1f : 1f;
        _wanderSeed = Random.Range(0f, 1000f);
        _separationRadiusMultiplier = Random.Range(0.82f, 1.18f);
        _separationStrengthMultiplier = Random.Range(0.8f, 1.25f);
        _desiredDistanceMultiplier = Random.Range(0.8f, 1.2f);
        currentHealth = maxHealth;
    }

    private void OnEnable()
    {
        RegisterEnemy();
        CachePlayer();
        _lastStuckPosition = rb != null ? rb.position : (Vector2)transform.position;
    }

    private void Start()
    {
        if (stats != null)
        {
            maxHealth = stats.maxHealthStat;
        }

        currentHealth = maxHealth;
    }

    private void OnDisable()
    {
        UnregisterEnemy();
    }

    public void TakeDamage(int damageTaken)
    {
        currentHealth -= damageTaken;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void UpdateMaxHealth(int newMaxHealth)
    {
        maxHealth = newMaxHealth;

        if (currentHealth < maxHealth)
        {
            currentHealth = maxHealth;
        }

        Debug.Log("Max HP: " + maxHealth + " | Current HP: " + currentHealth);
    }

    private void FixedUpdate()
    {
        if (rb == null)
        {
            return;
        }

        damageCooldownTimer -= Time.fixedDeltaTime;
        _repathTimer -= Time.fixedDeltaTime;
        _stuckTimer -= Time.fixedDeltaTime;

        if (!EnsurePlayer())
        {
            rb.linearVelocity = Vector2.zero;
            _currentVelocity = Vector2.zero;
            _appliedVelocity = Vector2.zero;
            return;
        }

        if (_stuckTimer <= 0f)
        {
            UpdateStuckState();
            _stuckTimer = StuckCheckInterval;
        }

        Vector2 position = rb.position;
        Vector2 toPlayer = (Vector2)playerTransform.position - position;
        float distanceToPlayer = toPlayer.magnitude;
        Vector2 toPlayerDirection = distanceToPlayer > 0.001f ? toPlayer / distanceToPlayer : Vector2.zero;

        if (_repathTimer <= 0f)
        {
            _cachedPathDirection = CalculatePathDirection(position, toPlayerDirection);
            _repathTimer = RepathInterval;
        }

        var steeringDirection = _cachedPathDirection;
        steeringDirection += CalculateSeparation(position) * separationStrength * _separationStrengthMultiplier;
        steeringDirection += CalculateOrbit(position, distanceToPlayer);
        steeringDirection += CalculateWander();
        steeringDirection += CalculateRecoveryDirection();

        if (steeringDirection.sqrMagnitude < 0.0001f)
        {
            steeringDirection = toPlayerDirection;
        }

        steeringDirection.Normalize();

        float speedMultiplier = stats != null ? stats.speedMultiplier : DefaultSpeedMultiplier;
        float targetSpeed = moveSpeed * speedMultiplier;

        var personalDesiredDistance = desiredPlayerDistance * _desiredDistanceMultiplier;
        if (distanceToPlayer < personalDesiredDistance)
        {
            targetSpeed *= Mathf.Clamp01(distanceToPlayer / personalDesiredDistance);
        }

        _desiredVelocity = steeringDirection * targetSpeed;
        _currentVelocity = Vector2.MoveTowards(
            _currentVelocity,
            _desiredVelocity,
            acceleration * Time.fixedDeltaTime * Mathf.Max(1f, speedMultiplier * 0.15f));

        _appliedVelocity = Vector2.Lerp(_appliedVelocity, _currentVelocity, SteeringSmoothing * Time.fixedDeltaTime);
        var smoothedVelocity = _appliedVelocity;
        rb.MovePosition(position + smoothedVelocity * Time.fixedDeltaTime);
        rb.linearVelocity = Vector2.zero;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (damageCooldownTimer > 0f || collision.gameObject == null || !collision.gameObject.CompareTag("Player"))
        {
            return;
        }

        Player player = collision.gameObject.GetComponent<Player>();
        if (player == null)
        {
            return;
        }

        float damageMultiplier = stats != null ? stats.damageMultiplier : DefaultDamageMultiplier;
        player.TakeDamage(Mathf.Max(1, Mathf.RoundToInt(baseDamage * damageMultiplier)));
        damageCooldownTimer = ContactDamageInterval;
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

        var dropCount = Random.Range(minXpOrbDrops, maxXpOrbDrops + 1);
        var deathPosition = transform.position;
        var placedPositions = new Vector3[dropCount];

        for (var i = 0; i < dropCount; i++)
        {
            var spawnPosition = FindXpOrbSpawnPosition(deathPosition, placedPositions, i);
            placedPositions[i] = spawnPosition;
            Instantiate(xpOrbPrefab, spawnPosition, xpOrbPrefab.transform.rotation);
        }
    }

    private Vector3 FindXpOrbSpawnPosition(Vector3 center, Vector3[] placedPositions, int placedCount)
    {
        const int maxAttempts = 12;

        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            var offset = placedCount == 0
                ? Random.insideUnitCircle * (xpDropRadius * 0.5f)
                : Random.insideUnitCircle * xpDropRadius;
            var candidate = center + new Vector3(offset.x, offset.y, 0f);

            if (IsFarEnoughFromOtherDrops(candidate, placedPositions, placedCount))
            {
                return candidate;
            }
        }

        var angle = placedCount * Mathf.PI;
        Vector2 fallbackOffset = new(Mathf.Cos(angle), Mathf.Sin(angle));
        return center + new Vector3(fallbackOffset.x, fallbackOffset.y, 0f) * minXpOrbSpacing;
    }

    private bool IsFarEnoughFromOtherDrops(Vector3 candidate, Vector3[] placedPositions, int placedCount)
    {
        for (var i = 0; i < placedCount; i++)
        {
            if (Vector2.Distance(candidate, placedPositions[i]) < minXpOrbSpacing)
            {
                return false;
            }
        }

        return true;
    }

    // Samples several directions around the direct path to the player
    // and picks the clearest option, so enemies steer around obstacles
    // instead of always running straight into them.
    private Vector2 CalculatePathDirection(Vector2 position, Vector2 directDirection)
    {
        var bestDirection = directDirection;
        var bestScore = ScoreDirection(position, directDirection, directDirection, 0f);

        var left = (Vector2)(Quaternion.Euler(0f, 0f, ObstacleSideProbeAngle * 0.5f) * directDirection);
        var leftScore = ScoreDirection(position, left, directDirection, 0.1f);
        if (leftScore > bestScore)
        {
            bestScore = leftScore;
            bestDirection = left;
        }

        var right = (Vector2)(Quaternion.Euler(0f, 0f, -ObstacleSideProbeAngle * 0.5f) * directDirection);
        var rightScore = ScoreDirection(position, right, directDirection, 0.1f);
        if (rightScore > bestScore)
        {
            bestScore = rightScore;
            bestDirection = right;
        }

        var hardLeft = (Vector2)(Quaternion.Euler(0f, 0f, ObstacleSideProbeAngle) * directDirection);
        var hardLeftScore = ScoreDirection(position, hardLeft, directDirection, 0.25f);
        if (hardLeftScore > bestScore)
        {
            bestScore = hardLeftScore;
            bestDirection = hardLeft;
        }

        var hardRight = (Vector2)(Quaternion.Euler(0f, 0f, -ObstacleSideProbeAngle) * directDirection);
        var hardRightScore = ScoreDirection(position, hardRight, directDirection, 0.25f);
        if (hardRightScore > bestScore)
        {
            bestScore = hardRightScore;
            bestDirection = hardRight;
        }

        var reverseLeft = (Vector2)(Quaternion.Euler(0f, 0f, ObstacleSideProbeAngle * 1.5f) * directDirection);
        var reverseLeftScore = ScoreDirection(position, reverseLeft, directDirection, 0.45f);
        if (reverseLeftScore > bestScore)
        {
            bestScore = reverseLeftScore;
            bestDirection = reverseLeft;
        }

        var reverseRight = (Vector2)(Quaternion.Euler(0f, 0f, -ObstacleSideProbeAngle * 1.5f) * directDirection);
        var reverseRightScore = ScoreDirection(position, reverseRight, directDirection, 0.45f);
        if (reverseRightScore > bestScore)
        {
            bestDirection = reverseRight;
        }

        return bestDirection.sqrMagnitude > 0.0001f ? bestDirection.normalized : directDirection;
    }

    // Scores a candidate movement direction by balancing two things:
    // how much open space is ahead, and how closely it still points
    // toward the player.
    private float ScoreDirection(Vector2 position, Vector2 candidateDirection, Vector2 preferredDirection, float directionPenalty)
    {
        if (candidateDirection.sqrMagnitude < 0.0001f)
        {
            return float.MinValue;
        }

        candidateDirection.Normalize();
        var hitDistance = ProbeObstacleDistance(position, candidateDirection);
        var clearanceScore = (hitDistance / obstacleProbeDistance) * obstacleAvoidanceStrength;
        var alignment = Vector2.Dot(candidateDirection, preferredDirection.normalized);
        return clearanceScore + alignment - directionPenalty;
    }

    // Checks how far this direction stays clear before hitting an obstacle.
    // Used by the path scoring step to prefer routes with more space.
    private float ProbeObstacleDistance(Vector2 origin, Vector2 direction)
    {
        var hitCount = Physics2D.CircleCast(
            origin,
            ObstacleClearanceRadius,
            direction,
            ObstacleContactFilter,
            ObstacleHits,
            obstacleProbeDistance);

        var bestDistance = obstacleProbeDistance;

        for (var i = 0; i < hitCount; i++)
        {
            var hitCollider = ObstacleHits[i].collider;
            if (!IsObstacle(hitCollider))
            {
                continue;
            }

            bestDistance = Mathf.Min(bestDistance, ObstacleHits[i].distance);
        }

        return bestDistance;
    }

    // Pushes this enemy away from nearby enemies so the swarm spreads out
    // instead of collapsing into a single overlapping clump.
    private Vector2 CalculateSeparation(Vector2 position)
    {
        var separation = Vector2.zero;
        var personalSeparationRadius = separationRadius * _separationRadiusMultiplier;

        for (var i = 0; i < ActiveEnemies.Count; i++)
        {
            var other = ActiveEnemies[i];
            if (other == null || other == this || other.rb == null)
            {
                continue;
            }

            var offset = position - other.rb.position;
            var distance = offset.magnitude;
            if (distance <= 0.001f || distance > personalSeparationRadius)
            {
                continue;
            }

            var weight = 1f - (distance / personalSeparationRadius);
            var randomBias = 1f + Mathf.Sin((Time.time * 2.7f) + _wanderSeed + (i * 0.73f)) * SeparationRandomness;
            separation += (offset / distance) * (weight * randomBias);
        }

        return separation;
    }

    // Adds a sideways movement bias near the player so enemies circle in
    // slightly instead of all aiming for the exact same point.
    private Vector2 CalculateOrbit(Vector2 position, float distanceToPlayer)
    {
        if (playerTransform == null || distanceToPlayer > (separationRadius * _separationRadiusMultiplier) * 2.35f)
        {
            return Vector2.zero;
        }

        var toPlayer = (Vector2)playerTransform.position - position;
        if (toPlayer.sqrMagnitude < 0.0001f)
        {
            return Vector2.zero;
        }

        Vector2 tangent = new(-toPlayer.y, toPlayer.x);
        tangent.Normalize();

        var personalDesiredDistance = desiredPlayerDistance * _desiredDistanceMultiplier;
        var orbitWeight = Mathf.InverseLerp(separationRadius * _separationRadiusMultiplier * 2.35f, personalDesiredDistance, distanceToPlayer);
        return tangent * (orbitStrength * orbitWeight * _orbitSign);
    }

    // Adds a small drifting offset so enemies feel less uniform and the
    // swarm movement stays a little messy.
    private Vector2 CalculateWander()
    {
        var noiseX = Mathf.PerlinNoise(_wanderSeed, Time.time * WanderFrequency) - 0.5f;
        var noiseY = Mathf.PerlinNoise(Time.time * WanderFrequency, _wanderSeed) - 0.5f;
        Vector2 wander = new(noiseX, noiseY);
        return wander * wanderStrength;
    }

    // Applies a temporary escape direction after the enemy is detected as
    // stuck, helping it slide away from obstacles and rejoin the chase.
    private Vector2 CalculateRecoveryDirection()
    {
        if (_recoveryTimer <= 0f)
        {
            return Vector2.zero;
        }

        _recoveryTimer -= Time.fixedDeltaTime;
        return _recoveryDirection * StuckSideBias;
    }

    // Detects enemies that are barely moving and picks a clearer sideways
    // direction to help them break free from obstacles.
    private void UpdateStuckState()
    {
        var currentPosition = rb.position;
        var movedDistance = Vector2.Distance(currentPosition, _lastStuckPosition);

        if (movedDistance < StuckDistanceThreshold && EnsurePlayer())
        {
            var toPlayer = ((Vector2)playerTransform.position - currentPosition).normalized;
            var leftClearance = ProbeObstacleDistance(currentPosition, (Vector2)(Quaternion.Euler(0f, 0f, ObstacleSideProbeAngle) * toPlayer));
            var rightClearance = ProbeObstacleDistance(currentPosition, (Vector2)(Quaternion.Euler(0f, 0f, -ObstacleSideProbeAngle) * toPlayer));
            var chosenSign = leftClearance > rightClearance ? 1f : -1f;
            _recoveryDirection = ((Vector2)(Quaternion.Euler(0f, 0f, ObstacleSideProbeAngle * chosenSign) * toPlayer)).normalized;
            _recoveryTimer = StuckRecoveryTime;
        }

        _lastStuckPosition = currentPosition;
    }

    private bool EnsurePlayer()
    {
        if (playerTransform != null)
        {
            return true;
        }

        CachePlayer();
        return playerTransform != null;
    }

    private void CachePlayer()
    {
        var player = FindAnyObjectByType<TestMovement>();
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    private void RegisterEnemy()
    {
        if (ActiveEnemies.Contains(this))
        {
            return;
        }

        foreach(var other in ActiveEnemies) {
            if (other == null || other.enemyCollider == null || enemyCollider == null)
            {
                continue;
            }

            Physics2D.IgnoreCollision(enemyCollider, other.enemyCollider, true);
        }

        ActiveEnemies.Add(this);
    }

    private void UnregisterEnemy()
    {
        ActiveEnemies.Remove(this);
    }

    private bool IsObstacle(Collider2D colliderToCheck)
    {
        if (colliderToCheck == null || colliderToCheck == enemyCollider || colliderToCheck.isTrigger)
        {
            return false;
        }

        return !colliderToCheck.CompareTag("Enemy") && !colliderToCheck.CompareTag("Player");
    }
}
