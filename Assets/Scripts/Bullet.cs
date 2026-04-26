using UnityEngine;

public class Bullet : MonoBehaviour
{

    [Header("Stats")]
    public float speed = 10f;
    public float lifetime = 3f;
    public int damage;
    private float pierce;

    private Vector2 moveDirection;



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



    private void OnTriggerEnter2D(Collider2D collision)
    {
        Enemy enemy = collision.GetComponent<Enemy>();

        if (enemy != null)
        {
            enemy.TakeDamage(damage, pierce);
            Debug.Log("Bullet hit enemy for " + damage + " damage and" + pierce + "pierce damage");
            Destroy(gameObject);
        }
    }


}