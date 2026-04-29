using System.Collections;
using UnityEngine;

public class AutoAim : MonoBehaviour
{

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate;
    public float bulletSpeed = 10f;
    [SerializeField] private SoundDefinition shootSound;

    [Header("Targeting")]
    public float detectionRange = 10f;
    public float targetRefreshRate = 0.3f;

    private GameObject currentTarget;

    private Stats stats;
    public int baseDamage = 1;

    public WeaponSO weaponSO;
    void Start()
    {
        StartCoroutine(UpdateTargetRoutine());
        StartCoroutine(ShootRoutine());
        fireRate = weaponSO.coolDown;
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
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

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

       
        SoundManager.Play(shootSound);

        

        Bullet bulletScript = Instantiate(weaponSO.bulletPrefab, firePoint.position, Quaternion.identity).GetComponent<Bullet>();


      
        bulletScript.weaponSO = weaponSO;
        
        Stats stats = GetComponentInParent<Stats>();
        int finalDamage = Mathf.RoundToInt(baseDamage * stats.damageMultiplier);

        Debug.Log("Multiplier: " + stats.damageMultiplier + " Final Damage: " + finalDamage);

 

        bulletScript.Initialize(direction, weaponSO.bulletSpeed, weaponSO, gameObject);
    }

    void RotateTowards(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        firePoint.rotation = Quaternion.Euler(0, 0, angle);
    }
}
