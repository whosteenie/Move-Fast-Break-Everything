using UnityEngine;

public class Enemy : MonoBehaviour
{
    private float moveSpeed = 0.05f;

    private Transform playerLocation;


    public int maxHealth = 10;
    private int currentHealth;
    private float damageCooldown = 1f;
    private float damageTimer = 0f;

    // public int damageMultiplier;
    //damage mult will be increased when enemy levls up using similar level up system to player, but for now just a base damage
    public int baseDamage = 1;

    private void Start()
    {
        currentHealth = maxHealth;
    }



    public void TakeDamage(int damageTaken)
    {
        currentHealth -= damageTaken;
        //Debug.Log("Enemy HP: " + currentHealth);
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //Seems a bit jank maybe fix this at some point
        //Currently it grabs the player by finding it's movement script, but considering it's called test movement
        //Doesn't exactly seem likely to stick around for long
        //So might want to replace with a method that finds the player in a more abstract way.
        TestMovement player = FindAnyObjectByType<TestMovement>();

        if (player != null)
        {
            //Small note, for some reason the enemy is in front of the trees because it teleports to z 0
            playerLocation = FindAnyObjectByType<TestMovement>().transform;
            Vector3 newPosition = Vector3.MoveTowards(transform.localPosition, playerLocation.localPosition, moveSpeed);

            //Replaced with rigidbody to stay more consistent
            //Maybe delete the collider if the physics is too annoying, and maybe constrain rotation
            //transform.localPosition = newPosition;

            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            rb.MovePosition(newPosition);

            //Attempt to keep the position behind trees.
            //It failed preserved for future attempts
            // transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, -1);
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject != null && collision.gameObject.tag == "Player")
        {
            //Replace this with a damage player call
            if (collision.gameObject.CompareTag("Player"))
            {
                damageTimer -= Time.deltaTime;
                if (damageTimer <= 0f)
                {
                    Player player = collision.gameObject.GetComponent<Player>();
                    if (player != null)
                    {
                        player.TakeDamage(baseDamage);
                        damageTimer = damageCooldown;
                    }
                }
            }
            //Leads to fun lose screen by accident, all the enemies just fall down.
        }
    }
}
