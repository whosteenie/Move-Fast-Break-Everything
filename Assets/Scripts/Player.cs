using UnityEngine;

public class Player : MonoBehaviour
{
    private Stats stats;
    private SpeedTower speedTower;
    public int maxHealth = 10;
    private int currentHealth;

    private const int debugHealAmount = 10;

    private void Start()
    {
        if (stats != null)
        {
            maxHealth = stats.maxHealthStat;
        }

        currentHealth = maxHealth;
    }

    private void Awake()
    {
        stats = GetComponent<Stats>();
    }

    public void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Q))
        {
            return;
        }

        Heal(debugHealAmount);

    }


    public void UpdateMaxHealth(int newMaxHealth)
    {
        maxHealth = newMaxHealth;

        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        Debug.Log("Max HP: " + maxHealth + " | Current HP: " + currentHealth);
    }

    public void Heal(int healAmount)
    {

        currentHealth += healAmount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        Debug.Log("Player HP: " + currentHealth);
    }



    public void TakeDamage(int damageTaken)
    {

        currentHealth -= damageTaken;
        Debug.Log("Player HP: " + currentHealth);
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        GameManager.Instance.ShowGameOver();
        Destroy(gameObject);
    }


}
