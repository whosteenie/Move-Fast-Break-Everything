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
    [Header("Drops")]
    [SerializeField] private GameObject xpOrbPrefab;
    [SerializeField] private int minXpOrbDrops = 1;
    [SerializeField] private int maxXpOrbDrops = 2;
    [SerializeField] private float xpDropRadius = 0.6f;
    [SerializeField] private float minXpOrbSpacing = 0.35f;

    [Header("Combat")]
    [SerializeField] private int maxHealth = 10;
    [SerializeField] private float baseDamage = 1f;
    [SerializeField] private float contactDamageInterval = 0.5f;

    [Header("Swarm Movement")]
    [SerializeField] private float moveSpeed = DefaultMoveSpeed;
    [SerializeField] private float acceleration = 18f;
    [SerializeField] private float steeringSmoothing = 12f;
    [SerializeField] private float desiredPlayerDistance = 0.85f;
    [SerializeField] private float orbitStrength = 0.75f;
    [SerializeField] private float separationRadius = 1.2f;
    [SerializeField] private float separationStrength = 1.8f;
    [SerializeField] private float obstacleProbeDistance = 1.2f;
    [SerializeField] private float obstacleAvoidanceStrength = 2.5f;
    [SerializeField] private float obstacleClearanceRadius = 0.25f;
    [SerializeField] private float obstacleSideProbeAngle = 55f;
    [SerializeField] private float repathInterval = 0.2f;
    [SerializeField] private float stuckCheckInterval = 0.45f;
    [SerializeField] private float stuckDistanceThreshold = 0.08f;
    [SerializeField] private float stuckRecoveryTime = 0.6f;
    [SerializeField] private float stuckSideBias = 1.35f;
    [SerializeField] private float wanderStrength = 0.4f;
    [SerializeField] private float wanderFrequency = 1.1f;
    [SerializeField] private float separationRandomness = 0.35f;

    private EnemyStats stats;
    private Rigidbody2D rb;
    private Collider2D enemyCollider;
    private Transform playerTransform;

    private int currentHealth;
    private float damageCooldownTimer;
    private float repathTimer;
    private float stuckTimer;
    private float recoveryTimer;
    private float orbitSign;
    private float wanderSeed;
    private float separationRadiusMultiplier;
    private float separationStrengthMultiplier;
    private float desiredDistanceMultiplier;
    private Vector2 desiredVelocity;
    private Vector2 currentVelocity;
    private Vector2 appliedVelocity;
    private Vector2 lastStuckPosition;
    private Vector2 cachedPathDirection;
    private Vector2 recoveryDirection;

    private void Awake()
    {
        stats = GetComponent<EnemyStats>();
        rb = GetComponent<Rigidbody2D>();
        enemyCollider = GetComponent<Collider2D>();
        orbitSign = Random.value < 0.5f ? -1f : 1f;
        wanderSeed = Random.Range(0f, 1000f);
        separationRadiusMultiplier = Random.Range(0.82f, 1.18f);
        separationStrengthMultiplier = Random.Range(0.8f, 1.25f);
        desiredDistanceMultiplier = Random.Range(0.8f, 1.2f);
        currentHealth = maxHealth;
    }

    private void OnEnable()
    {
        RegisterEnemy();
        CachePlayer();
        lastStuckPosition = rb != null ? rb.position : (Vector2)transform.position;
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
        repathTimer -= Time.fixedDeltaTime;
        stuckTimer -= Time.fixedDeltaTime;

        if (!EnsurePlayer())
        {
            rb.linearVelocity = Vector2.zero;
            currentVelocity = Vector2.zero;
            appliedVelocity = Vector2.zero;
            return;
        }

        if (stuckTimer <= 0f)
        {
            UpdateStuckState();
            stuckTimer = stuckCheckInterval;
        }

        Vector2 position = rb.position;
        Vector2 toPlayer = (Vector2)playerTransform.position - position;
        float distanceToPlayer = toPlayer.magnitude;
        Vector2 toPlayerDirection = distanceToPlayer > 0.001f ? toPlayer / distanceToPlayer : Vector2.zero;

        if (repathTimer <= 0f)
        {
            cachedPathDirection = CalculatePathDirection(position, toPlayerDirection);
            repathTimer = repathInterval;
        }

        Vector2 steeringDirection = cachedPathDirection;
        steeringDirection += CalculateSeparation(position) * separationStrength * separationStrengthMultiplier;
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

        float personalDesiredDistance = desiredPlayerDistance * desiredDistanceMultiplier;
        if (distanceToPlayer < personalDesiredDistance)
        {
            targetSpeed *= Mathf.Clamp01(distanceToPlayer / personalDesiredDistance);
        }

        desiredVelocity = steeringDirection * targetSpeed;
        currentVelocity = Vector2.MoveTowards(
            currentVelocity,
            desiredVelocity,
            acceleration * Time.fixedDeltaTime * Mathf.Max(1f, speedMultiplier * 0.15f));

        appliedVelocity = Vector2.Lerp(appliedVelocity, currentVelocity, steeringSmoothing * Time.fixedDeltaTime);
        Vector2 smoothedVelocity = appliedVelocity;
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
        damageCooldownTimer = contactDamageInterval;
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

        int dropCount = Random.Range(minXpOrbDrops, maxXpOrbDrops + 1);
        Vector3 deathPosition = transform.position;
        Vector3[] placedPositions = new Vector3[dropCount];

        for (int i = 0; i < dropCount; i++)
        {
            Vector3 spawnPosition = FindXpOrbSpawnPosition(deathPosition, placedPositions, i);
            placedPositions[i] = spawnPosition;
            Instantiate(xpOrbPrefab, spawnPosition, xpOrbPrefab.transform.rotation);
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

    private Vector2 CalculatePathDirection(Vector2 position, Vector2 directDirection)
    {
        Vector2 bestDirection = directDirection;
        float bestScore = ScoreDirection(position, directDirection, directDirection, 0f);

        Vector2 left = Rotate(directDirection, obstacleSideProbeAngle * 0.5f);
        float leftScore = ScoreDirection(position, left, directDirection, 0.1f);
        if (leftScore > bestScore)
        {
            bestScore = leftScore;
            bestDirection = left;
        }

        Vector2 right = Rotate(directDirection, -obstacleSideProbeAngle * 0.5f);
        float rightScore = ScoreDirection(position, right, directDirection, 0.1f);
        if (rightScore > bestScore)
        {
            bestScore = rightScore;
            bestDirection = right;
        }

        Vector2 hardLeft = Rotate(directDirection, obstacleSideProbeAngle);
        float hardLeftScore = ScoreDirection(position, hardLeft, directDirection, 0.25f);
        if (hardLeftScore > bestScore)
        {
            bestScore = hardLeftScore;
            bestDirection = hardLeft;
        }

        Vector2 hardRight = Rotate(directDirection, -obstacleSideProbeAngle);
        float hardRightScore = ScoreDirection(position, hardRight, directDirection, 0.25f);
        if (hardRightScore > bestScore)
        {
            bestScore = hardRightScore;
            bestDirection = hardRight;
        }

        Vector2 reverseLeft = Rotate(directDirection, obstacleSideProbeAngle * 1.5f);
        float reverseLeftScore = ScoreDirection(position, reverseLeft, directDirection, 0.45f);
        if (reverseLeftScore > bestScore)
        {
            bestScore = reverseLeftScore;
            bestDirection = reverseLeft;
        }

        Vector2 reverseRight = Rotate(directDirection, -obstacleSideProbeAngle * 1.5f);
        float reverseRightScore = ScoreDirection(position, reverseRight, directDirection, 0.45f);
        if (reverseRightScore > bestScore)
        {
            bestDirection = reverseRight;
        }

        return bestDirection.sqrMagnitude > 0.0001f ? bestDirection.normalized : directDirection;
    }

    private float ScoreDirection(Vector2 position, Vector2 candidateDirection, Vector2 preferredDirection, float directionPenalty)
    {
        if (candidateDirection.sqrMagnitude < 0.0001f)
        {
            return float.MinValue;
        }

        candidateDirection.Normalize();
        float hitDistance = ProbeObstacleDistance(position, candidateDirection);
        float clearanceScore = (hitDistance / obstacleProbeDistance) * obstacleAvoidanceStrength;
        float alignment = Vector2.Dot(candidateDirection, preferredDirection.normalized);
        return clearanceScore + alignment - directionPenalty;
    }

    private float ProbeObstacleDistance(Vector2 origin, Vector2 direction)
    {
        int hitCount = Physics2D.CircleCast(
            origin,
            obstacleClearanceRadius,
            direction,
            ObstacleContactFilter,
            ObstacleHits,
            obstacleProbeDistance);

        float bestDistance = obstacleProbeDistance;

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hitCollider = ObstacleHits[i].collider;
            if (!IsObstacle(hitCollider))
            {
                continue;
            }

            bestDistance = Mathf.Min(bestDistance, ObstacleHits[i].distance);
        }

        return bestDistance;
    }

    private Vector2 CalculateSeparation(Vector2 position)
    {
        Vector2 separation = Vector2.zero;
        float personalSeparationRadius = separationRadius * separationRadiusMultiplier;

        for (int i = 0; i < ActiveEnemies.Count; i++)
        {
            Enemy other = ActiveEnemies[i];
            if (other == null || other == this || other.rb == null)
            {
                continue;
            }

            Vector2 offset = position - other.rb.position;
            float distance = offset.magnitude;
            if (distance <= 0.001f || distance > personalSeparationRadius)
            {
                continue;
            }

            float weight = 1f - (distance / personalSeparationRadius);
            float randomBias = 1f + Mathf.Sin((Time.time * 2.7f) + wanderSeed + (i * 0.73f)) * separationRandomness;
            separation += (offset / distance) * (weight * randomBias);
        }

        return separation;
    }

    private Vector2 CalculateOrbit(Vector2 position, float distanceToPlayer)
    {
        if (playerTransform == null || distanceToPlayer > (separationRadius * separationRadiusMultiplier) * 2.35f)
        {
            return Vector2.zero;
        }

        Vector2 toPlayer = (Vector2)playerTransform.position - position;
        if (toPlayer.sqrMagnitude < 0.0001f)
        {
            return Vector2.zero;
        }

        Vector2 tangent = new(-toPlayer.y, toPlayer.x);
        tangent.Normalize();

        float personalDesiredDistance = desiredPlayerDistance * desiredDistanceMultiplier;
        float orbitWeight = Mathf.InverseLerp((separationRadius * separationRadiusMultiplier) * 2.35f, personalDesiredDistance, distanceToPlayer);
        return tangent * (orbitStrength * orbitWeight * orbitSign);
    }

    private Vector2 CalculateWander()
    {
        float noiseX = Mathf.PerlinNoise(wanderSeed, Time.time * wanderFrequency) - 0.5f;
        float noiseY = Mathf.PerlinNoise(Time.time * wanderFrequency, wanderSeed) - 0.5f;
        Vector2 wander = new(noiseX, noiseY);
        return wander * wanderStrength;
    }

    private Vector2 CalculateRecoveryDirection()
    {
        if (recoveryTimer <= 0f)
        {
            return Vector2.zero;
        }

        recoveryTimer -= Time.fixedDeltaTime;
        return recoveryDirection * stuckSideBias;
    }

    private void UpdateStuckState()
    {
        Vector2 currentPosition = rb.position;
        float movedDistance = Vector2.Distance(currentPosition, lastStuckPosition);

        if (movedDistance < stuckDistanceThreshold && EnsurePlayer())
        {
            Vector2 toPlayer = ((Vector2)playerTransform.position - currentPosition).normalized;
            float leftClearance = ProbeObstacleDistance(currentPosition, Rotate(toPlayer, obstacleSideProbeAngle));
            float rightClearance = ProbeObstacleDistance(currentPosition, Rotate(toPlayer, -obstacleSideProbeAngle));
            float chosenSign = leftClearance > rightClearance ? 1f : -1f;
            recoveryDirection = Rotate(toPlayer, obstacleSideProbeAngle * chosenSign).normalized;
            recoveryTimer = stuckRecoveryTime;
        }

        lastStuckPosition = currentPosition;
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
        TestMovement player = FindAnyObjectByType<TestMovement>();
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

        for (int i = 0; i < ActiveEnemies.Count; i++)
        {
            Enemy other = ActiveEnemies[i];
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

        if (colliderToCheck.CompareTag("Enemy") || colliderToCheck.CompareTag("Player"))
        {
            return false;
        }

        return true;
    }

    private static Vector2 Rotate(Vector2 vector, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);
        return new Vector2(
            vector.x * cos - vector.y * sin,
            vector.x * sin + vector.y * cos);
    }
}
