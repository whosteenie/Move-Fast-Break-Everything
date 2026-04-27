using UnityEngine;

public class Bullet : MonoBehaviour
{

    [Header("Stats")]
    public float speed = 10f;
    public float lifetime = 3f;
    public float damage;
   
    private Vector2 moveDirection;
    private GameObject owner;
    public WeaponSO weaponSO;

    private Stats stats;




    public void Initialize(Vector2 direction, float bulletSpeed, WeaponSO weapon, GameObject bulletOwner)
    {
        weaponSO = weapon;
        owner = bulletOwner;
        stats = owner.GetComponent<Stats>();

        moveDirection = direction.normalized;
        speed = bulletSpeed;

        RotateBullet();
        Destroy(gameObject, lifetime);
    }

    void Awake()
    {
        stats = GetComponentInParent<Stats>();
       

    }

    void Update()
    {

        transform.position += (Vector3)(moveDirection * (speed * Time.deltaTime));
    }

    void RotateBullet()
    {
        //atan2 changes direction to angle
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90);

    }




    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (owner == null)
        {
            Debug.LogError("OWNER IS NULL on bullet!");
            return;
        }

        damage = weaponSO.baseDamage;
        if (stats != null)
        {
            damage *= stats.damageMultiplier;
        }
        if (collision.gameObject == owner) return;

        Player player = collision.GetComponent<Player>();
        Enemy enemy = collision.GetComponent<Enemy>();
        BossController boss = collision.GetComponent<BossController>();

        if (owner.GetComponent<Enemy>() != null && player != null)
        {
            player.TakeDamage(damage);
            Destroy(gameObject);
        }
        else if (owner.GetComponent<Player>() != null && enemy != null)
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject);
        } else if (boss != null) {
            boss.TakeDamage(damage);
            Debug.Log("Bullet hit enemy for " + damage + " damage.");
            Destroy(gameObject);
        }
    }


}