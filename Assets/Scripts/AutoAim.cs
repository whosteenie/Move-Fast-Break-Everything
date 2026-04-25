using System.Collections;
using UnityEngine;

public class AutoAim : MonoBehaviour
{

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 2f;
    public float bulletSpeed = 10f;

    [Header("Targeting")]
    public float detectionRange = 10f;
    public float targetRefreshRate = 0.3f;

    private GameObject currentTarget;

    private Stats stats;
    public int baseDamage = 1;

    void Start()
    {
        StartCoroutine(UpdateTargetRoutine());
        StartCoroutine(ShootRoutine());
    }

    void Awake()
    {
        stats = GetComponentInParent<Stats>();
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
        GameObject closest = null;
        float minDistance = dis;
        closest = FindClosestTargetOfType<Enemy>(closest, ref minDistance);
        closest = FindClosestTargetOfType<DestructibleObstacle>(closest, ref minDistance);

        return closest;
    }

    GameObject FindClosestTargetOfType<T>(GameObject currentClosest, ref float minDistance) where T : MonoBehaviour
    {
        T[] targets = FindObjectsByType<T>();

        foreach (T target in targets)
        {
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
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        Bullet bulletScript = bullet.GetComponent<Bullet>();

        Stats stats = GetComponentInParent<Stats>();
        int finalDamage = Mathf.RoundToInt(baseDamage * stats.damageMultiplier);

        Debug.Log("Multiplier: " + stats.damageMultiplier + " Final Damage: " + finalDamage);

        if (bulletScript != null)
        {
            bulletScript.SetDirection(direction, bulletSpeed, finalDamage);
        }
    }

    void RotateTowards(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        firePoint.rotation = Quaternion.Euler(0, 0, angle);
    }
}
