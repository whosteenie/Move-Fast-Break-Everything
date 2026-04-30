using System.Collections;
using UnityEngine;

public class AutoAim : MonoBehaviour
{
    public WeaponSO weaponSO;

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate;
    public float bulletSpeed;
    [SerializeField] private SoundDefinition shootSound;

    [Header("Targeting")]
    public float targetRefreshRate = 0.3f;

    private GameObject currentTarget;

    private Stats stats;
    public int baseDamage = 1;

    void Awake()
    {
        stats = GetComponentInParent<Stats>();
    }

    void Start()
    {
        bulletSpeed = weaponSO.bulletSpeed;
        fireRate = weaponSO.coolDown;

        StartCoroutine(UpdateTargetRoutine());
        StartCoroutine(ShootRoutine());
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


    public string tag = "";
    public float dis = 10f;

    GameObject FindClosestEnemy()
    {
        if (GetComponentInParent<Player>() != null)
        {
            return FindClosestPlayerTarget();
        }

        GameObject[] targets = GameObject.FindGameObjectsWithTag(tag);

        GameObject closest = null;
        float minDistance = dis;

        foreach (GameObject target in targets)
        {
            float distance = Vector2.Distance(transform.position, target.transform.position);

            if (distance < minDistance)
            {
                minDistance = distance;
                closest = target;
            }
        }

        return closest;
    }

    GameObject FindClosestPlayerTarget()
    {
        GameObject closest = null;
        float minDistance = dis;

        foreach (Enemy enemy in FindObjectsByType<Enemy>())
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);

            if (distance < minDistance)
            {
                minDistance = distance;
                closest = enemy.gameObject;
            }
        }

        foreach (DestructibleObstacle obstacle in FindObjectsByType<DestructibleObstacle>())
        {
            float distance = Vector2.Distance(transform.position, obstacle.transform.position);

            if (distance < minDistance)
            {
                minDistance = distance;
                closest = obstacle.gameObject;
            }
        }

        return closest;
    }
    

    void Shoot(Vector2 direction)
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        SoundManager.Play(shootSound);
        bullet.GetComponent<Bullet>().SetOwner(gameObject);
        Bullet bulletScript = bullet.GetComponent<Bullet>();

        if (bulletScript != null)
        {
            int finalDamage = stats != null ? stats.GetDamage(baseDamage) : baseDamage;
            float pierce = stats != null ? stats.GetPierce() : 0f;

            bulletScript.SetDirection(direction, bulletSpeed, finalDamage, pierce);
        }
    }

    void RotateTowards(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        firePoint.rotation = Quaternion.Euler(0, 0, angle);
    }
}
