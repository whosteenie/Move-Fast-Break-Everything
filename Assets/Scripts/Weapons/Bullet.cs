using UnityEngine;

public class Bullet : MonoBehaviour
{

    [Header("Stats")]
    public float speed = 10f;
    public float lifetime = 3f;
    public int damage;
    private float pierce;

    private GameObject owner;
    public WeaponSO weaponSO;
    private Vector2 moveDirection;

    private Stats stats;
    void Awake(){
        stats = GetComponentInParent<Stats>();
    }


    void Update()
    {

        transform.position += (Vector3)(moveDirection * (speed * Time.deltaTime));
    }
    public void SetDirection(Vector2 direction, float bulletSpeed, int damageAmount, float pierceAmount)
    {
        moveDirection = direction.normalized;
        speed = bulletSpeed;
        damage = damageAmount;
        pierce = pierceAmount;

        RotateBullet();
        Destroy(gameObject, lifetime);
    }

    void RotateBullet()
    {
        //atan2 changes direction to angle
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90);

    }
    public void SetOwner(GameObject bulletOwner)
    {
        owner = bulletOwner;
    }


   private void OnTriggerEnter2D(Collider2D collision)
    {

        if (owner == null)
        {
            Destroy(gameObject);
            return;
        }

        if (collision.gameObject == owner) return;

        Player player = collision.GetComponent<Player>();
        Enemy enemy = collision.GetComponent<Enemy>();

        if (owner.GetComponent<Enemy>() != null && player != null)
        {
            player.TakeDamage(damage);
            Destroy(gameObject);
        }
        else if (owner.GetComponent<Player>() != null && enemy != null)
        {
            enemy.TakeDamage(damage, pierce);
            Destroy(gameObject);
        }
    }




}
