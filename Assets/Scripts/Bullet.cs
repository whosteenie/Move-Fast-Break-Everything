using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Stats")]
    public float speed = 10f;
    public float lifetime = 3f;
    public int damage = 1;

    private Vector2 moveDirection;


    public void SetDirection(Vector2 direction, float bulletSpeed)
    {
        moveDirection = direction.normalized;
        speed = bulletSpeed;



        RotateBullet();
        Destroy(gameObject, lifetime);
    }

    void Update()
    {

        transform.position += (Vector3)(moveDirection * speed * Time.deltaTime);
        RotateBullet();
    }

    void RotateBullet()
    {
        //atan2 changes direction to angle
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
        }
    }

}
