using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Stats")]
    public float speed = 10f;
    public float lifetime = 3f;
    public int damage;
    private float pierce;

    private Vector2 moveDirection;
    private Collider2D bulletCollider;
    [SerializeField] private GameObject owner;
    [SerializeField] private AutoAim.TargetTeam projectileTeam = AutoAim.TargetTeam.Enemy;
    private Collider2D[] ownerColliders;



    void Awake()
    {
        bulletCollider = GetComponent<Collider2D>();
    }

    void Update()
    {

        transform.position += (Vector3)(moveDirection * (speed * Time.deltaTime));
    }
    public void SetDirection(
        Vector2 direction,
        float bulletSpeed,
        int damageAmount,
        float pierceAmount,
        GameObject projectileOwner = null,
        AutoAim.TargetTeam targetTeam = AutoAim.TargetTeam.Enemy,
        Collider2D[] cachedOwnerColliders = null)
    {
        moveDirection = direction.normalized;
        speed = bulletSpeed;
        damage = damageAmount;
        pierce = pierceAmount;
        owner = projectileOwner;
        projectileTeam = targetTeam;
        ownerColliders = cachedOwnerColliders;

        IgnoreOwnerCollisions();

        RotateBullet();
        Destroy(gameObject, lifetime);
    }

    void RotateBullet()
    {
        //atan2 changes direction to angle
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90);

    }



    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (owner != null && (collision.gameObject == owner || collision.transform.IsChildOf(owner.transform))) {
            return;
        }

        if (projectileTeam == AutoAim.TargetTeam.Player) {
            var player = collision.GetComponentInParent<Player>();
            if (player != null) {
                player.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }
        }

        var enemy = collision.GetComponentInParent<Enemy>();
        var boss = collision.GetComponentInParent<BossController>();
        var destructibleObstacle = collision.GetComponentInParent<DestructibleObstacle>();

        if (projectileTeam == AutoAim.TargetTeam.Enemy && enemy != null) {
            enemy.TakeDamage(damage, pierce);
            Debug.Log("Bullet hit enemy for " + damage + " damage and" + pierce + "pierce damage");
            Destroy(gameObject);
        } else if (projectileTeam == AutoAim.TargetTeam.Enemy && boss != null) {
            boss.TakeDamage(damage);
            Debug.Log("Bullet hit enemy for " + damage + " damage.");
            Destroy(gameObject);
        } else if (projectileTeam == AutoAim.TargetTeam.Enemy && destructibleObstacle != null) {
            destructibleObstacle.TakeDamage(damage);
            Debug.Log("Bullet hit obstacle for " + damage + " damage.");
            Destroy(gameObject);
        }
    }

    private void IgnoreOwnerCollisions()
    {
        if (owner == null) return;

        if (bulletCollider == null) return;

        if (ownerColliders == null) {
            ownerColliders = System.Array.Empty<Collider2D>();
        }

        foreach (var ownerCollider in ownerColliders) {
            if (ownerCollider != null) {
                Physics2D.IgnoreCollision(bulletCollider, ownerCollider, true);
            }
        }
    }


}
