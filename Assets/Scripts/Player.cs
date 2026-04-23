using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Stats stats;
    private SpeedTower speedTower;
    private PlayerHealthBar healthBar;
    public int maxHealth = 10;
    private int currentHealth;

    private const int debugHealAmount = 10;

    public event Action<int, int> OnHealthChanged;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    private void Start()
    {
        if (stats != null)
        {
            maxHealth = stats.maxHealthStat;
        }

        currentHealth = maxHealth;
        EnsureHealthBar();
        NotifyHealthChanged();
    }

    private void Awake()
    {
        stats = GetComponent<Stats>();
        EnsureHealthBar();
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
        NotifyHealthChanged();
    }

    public void Heal(int healAmount)
    {

        currentHealth += healAmount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        Debug.Log("Player HP: " + currentHealth);
        NotifyHealthChanged();
    }



    public void TakeDamage(int damageTaken)
    {

        currentHealth -= damageTaken;
        currentHealth = Mathf.Max(currentHealth, 0);
        Debug.Log("Player HP: " + currentHealth);
        NotifyHealthChanged();
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

    private void EnsureHealthBar()
    {
        if (healthBar == null)
        {
            healthBar = GetComponentInChildren<PlayerHealthBar>(true);
        }

        if (healthBar != null)
        {
            healthBar.Initialize(this);
            return;
        }

        GameObject healthBarObject = new("PlayerHealthBar");
        healthBarObject.transform.SetParent(transform, false);
        healthBar = healthBarObject.AddComponent<PlayerHealthBar>();
        healthBar.Initialize(this);
    }

    private void NotifyHealthChanged()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        if(healthBar != null) healthBar.Refresh(currentHealth, maxHealth);
    }

}
