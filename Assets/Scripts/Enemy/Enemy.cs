using UnityEngine;

public class Enemy : MonoBehaviour
{
    private EnemyStats stats;
    private float moveSpeed = 0.5f;
    [SerializeField] private GameObject xpOrbPrefab;
    [SerializeField] private SoundDefinition hurtSound;
    [SerializeField] private int minXpOrbDrops = 1;
    [SerializeField] private int maxXpOrbDrops = 2;
    [SerializeField] private float xpDropRadius = 0.6f;
    [SerializeField] private float minXpOrbSpacing = 0.35f;

    private Transform playerLocation;


    public float maxHealth = 10;

    private float currentHealth;

    // public int damageMultiplier;
    //damage mult will be increased when enemy levls up using similar level up system to player, but for now just a base damage
    public float baseDamage = 1;

    private void Start()
    {
        currentHealth = maxHealth;
        if (stats != null)
        {
            maxHealth = stats.maxHealthStat;
        }

        currentHealth = maxHealth;
    }
    private void Awake()
    {
        stats = GetComponent<EnemyStats>();
    }


    public void TakeDamage(float damageTaken)
    {
        if (damageTaken <= 0)
        {
            return;
        }

        currentHealth -= damageTaken;
        currentHealth = Mathf.Max(currentHealth, 0);
        SoundManager.Play(hurtSound);
        //Debug.Log("Enemy HP: " + currentHealth);
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        SpawnXpOrbs();
        Destroy(gameObject);
    }

    private void SpawnXpOrbs()
    {
        if (xpOrbPrefab == null)
        {
            return;
        }

        int dropCount = Random.Range(minXpOrbDrops, maxXpOrbDrops + 1);
        Vector3 deathPosition = transform.position;
        Vector3[] placedPositions = new Vector3[dropCount];

        for (int i = 0; i < dropCount; i++)
        {
            Vector3 spawnPosition = FindXpOrbSpawnPosition(deathPosition, placedPositions, i);
            placedPositions[i] = spawnPosition;
            Instantiate(xpOrbPrefab, spawnPosition, xpOrbPrefab.transform.rotation);
        }
    }

    private Vector3 FindXpOrbSpawnPosition(Vector3 center, Vector3[] placedPositions, int placedCount)
    {
        const int MaxAttempts = 12;

        for (int attempt = 0; attempt < MaxAttempts; attempt++)
        {
            Vector2 offset = placedCount == 0
                ? Random.insideUnitCircle * (xpDropRadius * 0.5f)
                : Random.insideUnitCircle * xpDropRadius;
            Vector3 candidate = center + new Vector3(offset.x, offset.y, 0f);

            if (IsFarEnoughFromOtherDrops(candidate, placedPositions, placedCount))
            {
                return candidate;
            }
        }

        float angle = placedCount * Mathf.PI;
        Vector2 fallbackOffset = new(Mathf.Cos(angle), Mathf.Sin(angle));
        return center + new Vector3(fallbackOffset.x, fallbackOffset.y, 0f) * minXpOrbSpacing;
    }

    private bool IsFarEnoughFromOtherDrops(Vector3 candidate, Vector3[] placedPositions, int placedCount)
    {
        for (int i = 0; i < placedCount; i++)
        {
            if (Vector2.Distance(candidate, placedPositions[i]) < minXpOrbSpacing)
            {
                return false;
            }
        }

        return true;
    }

    public void UpdateMaxHealth(int newMaxHealth)
    {
        maxHealth = newMaxHealth;

        if (currentHealth < maxHealth)
        {
            currentHealth = maxHealth;
        }
        Debug.Log("Max HP: " + maxHealth + " | Current HP: " + currentHealth);
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        //Seems a bit jank maybe fix this at some point
        //Currently it grabs the player by finding it's movement script, but considering it's called test movement
        //Doesn't exactly seem likely to stick around for long
        //So might want to replace with a method that finds the player in a more abstract way.
        float speedMult = (stats != null) ? stats.speedMultiplier : 5f;
        TestMovement player = FindAnyObjectByType<TestMovement>();

        if (player != null)
        {
            //Small note, for some reason the enemy is in front of the trees because it teleports to z 0
            playerLocation = FindAnyObjectByType<TestMovement>().transform;
            Vector3 newPosition = Vector3.MoveTowards(transform.localPosition, playerLocation.localPosition, moveSpeed * speedMult * Time.fixedDeltaTime);

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
        float damageMultiplier = (stats != null) ? stats.damageMultiplier : 2f;
        if (collision.gameObject != null && collision.gameObject.CompareTag("Player"))
        {
            Player player = collision.gameObject.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage((int)(baseDamage * damageMultiplier));
            }
            //Leads to fun lose screen by accident, all the enemies just fall down.
        }
    }
}
