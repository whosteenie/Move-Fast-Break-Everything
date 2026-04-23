using System.Collections;
using UnityEngine;

public class AutoAim : MonoBehaviour
{
    public WeaponSO weaponSO;
    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed;
    public float fireRate;

    [Header("Targeting")]
    public float detectionRange = 10f;
    public float targetRefreshRate = 0.3f;

    private GameObject currentTarget;

    private Stats stats;
    

    void Start()
    {
        bulletSpeed = weaponSO.bulletSpeed;
        fireRate = weaponSO.fireRate;
        
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

            yield return new WaitForSeconds(1f / fireRate);
        }
    }
    [SerializeField]
    float dis = 10f;
    public string tag = "";
    GameObject FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(tag);

        GameObject closest = null;

        float minDistance = dis;

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);

            if (distance < minDistance && distance <= detectionRange)
            {
                minDistance = distance;
                closest = enemy;
            }
        }

        return closest;
    }

    void Shoot(Vector2 direction)
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        bullet.GetComponent<Bullet>().SetOwner(gameObject);
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            int finalDamage = bulletScript.damage;

            bulletScript.SetDirection(direction, bulletSpeed);
        }
    }

    void RotateTowards(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        firePoint.rotation = Quaternion.Euler(0, 0, angle);
    }
}
