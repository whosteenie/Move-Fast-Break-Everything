using System.Collections;
using UnityEngine;

public class AutoAim : MonoBehaviour
{
    public enum TargetTeam
    {
        Player,
        Enemy
    }

    [Header("Shooting")]
    public Bullet bulletPrefab;
    public Transform firePoint;
    public float fireRate = 2f;
    public float bulletSpeed = 10f;
    [SerializeField] private SoundDefinition shootSound;

    [Header("Targeting")]
    public float detectionRange = 10f;
    public float targetRefreshRate = 0.3f;
    [SerializeField] private TargetTeam targetTeam = TargetTeam.Enemy;
    [SerializeField] private GameObject owner;

    private GameObject currentTarget;

    private Stats stats;
    private Collider2D[] ownerColliders;
    public int baseDamage = 1;

    void Start()
    {
        StartCoroutine(UpdateTargetRoutine());
        StartCoroutine(ShootRoutine());
    }

    void Awake()
    {
        stats = GetComponentInParent<Stats>();
        ownerColliders = owner.GetComponentsInChildren<Collider2D>(true);
    }

    IEnumerator UpdateTargetRoutine()
    {
        while (true)
        {
            currentTarget = FindClosestEnemy();
            yield return new WaitForSeconds(targetRefreshRate);
        }
    }

    IEnumerator ShootRoutine()
    {
        while (true)
        {
            if (currentTarget != null)
            {
                Vector2 direction = (currentTarget.transform.position - firePoint.position).normalized;

                RotateTowards(direction);
                Shoot(direction);
            }
            float finalFireRate = (stats != null) ? stats.GetFireRate(fireRate) : fireRate;

            yield return new WaitForSeconds(1f / finalFireRate);
        }
    }
    [SerializeField]
    float dis = 10f;
    GameObject FindClosestEnemy()
    {
        return targetTeam == TargetTeam.Player
            ? FindClosestPlayerTarget()
            : FindClosestEnemyTarget();
    }

    GameObject FindClosestEnemyTarget()
    {
        GameObject closest = null;
        float minDistance = dis;
        closest = FindClosestTargetOfType<Enemy>(closest, ref minDistance);
        closest = FindClosestTargetOfType<DestructibleObstacle>(closest, ref minDistance);
        return closest;
    }

    private GameObject FindClosestPlayerTarget()
    {
        var player = FindAnyObjectByType<Player>();

        if (player == null)
        {
            return null;
        }

        var distance = Vector2.Distance(transform.position, player.transform.position);
        return distance <= detectionRange && distance < dis ? player.gameObject : null;
    }

    GameObject FindClosestTargetOfType<T>(GameObject currentClosest, ref float minDistance) where T : MonoBehaviour
    {
        T[] targets = FindObjectsByType<T>();

        foreach (T target in targets)
        {
            if (owner != null && target.gameObject == owner)
            {
                continue;
            }

            float distance = Vector2.Distance(transform.position, target.transform.position);

            if (distance < minDistance && distance <= detectionRange)
            {
                minDistance = distance;
                currentClosest = target.gameObject;
            }
        }

        return currentClosest;
    }

    void Shoot(Vector2 direction)
    {
        Bullet bulletInstance = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        SoundManager.Play(shootSound);
        
        // int finalDamage = Mathf.RoundToInt(baseDamage * stats.rangedDamageMultiplier);

        // Debug.Log("Multiplier: " + stats.rangedDamageMultiplier + " Final Damage: " + finalDamage);

        if (bulletInstance != null)
        {
            int finalDamage = stats != null ? stats.GetDamage(baseDamage) : baseDamage;
            float pierce = stats != null ? stats.GetPierce() : 0f;
            bulletInstance.SetDirection(direction, bulletSpeed, finalDamage, pierce, owner, targetTeam, ownerColliders);
        }
    }

    void RotateTowards(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        firePoint.rotation = Quaternion.Euler(0, 0, angle);
    }
}
